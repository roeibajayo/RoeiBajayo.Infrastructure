using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Infrastructure.Utils.Text;

public interface ICssSelectable<T>
{
    public string? TagName { get; }
    public string? GetAttribute(string name);
    public T? Parent { get; }
    public IEnumerable<T> Children { get; }
}

public static class ICssSelecableExtenions
{
    //auto methods
    public static string? GetId<T>(this T tag) where T : ICssSelectable<T> =>
        tag.GetAttribute("id");

    public static string[] GetClasses<T>(this T tag) where T : ICssSelectable<T> =>
        tag.GetAttribute("class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

    public static IEnumerable<T> GetDescendants<T>(this T tag) where T : ICssSelectable<T>
    {
        if (tag?.Children is null)
            yield break;

        foreach (var item in tag.Children)
        {
            yield return item;

            foreach (var children in item.GetDescendants())
                yield return children;
        }
    }
    public static IEnumerable<T> GetAncestors<T>(this T tag) where T : ICssSelectable<T>
    {
        var parent = tag.Parent;
        while (parent is not null)
        {
            yield return parent;
            parent = parent.Parent;
        }
    }
    public static IEnumerable<T> GetSibilings<T>(this T tag) where T : ICssSelectable<T> =>
        tag.Parent?.Children.Where(x => !x.Equals(tag)) ?? [];

    public static T? QuerySelector<T>(this T tag, string query) where T : ICssSelectable<T> =>
        CssSelector<T>.QuerySelector(tag, query);

    public static IEnumerable<T> QuerySelectorAll<T>(this T tag, string query) where T : ICssSelectable<T> =>
        CssSelector<T>.QuerySelectorAll(tag, query);

    public static bool IsQueryMatch<T>(this T tag, string query) where T : ICssSelectable<T> =>
        CssSelector<T>.IsQueryMatch(tag, query);
}

public static partial class CssSelector<T> where T : ICssSelectable<T>
{
    private static bool ContainsAllClasses(T root, string[] className)
    {
        if (className == null || className.Length == 0)
            return false;

        var classes = root.GetClasses();
        if (classes == null || classes.Length < className.Length)
            return false;

        return className.All(x => classes.Contains(x, StringComparer.OrdinalIgnoreCase));
    }

    public static T? QuerySelector(T root, string query) =>
        QuerySelectorAll(root, query).FirstOrDefault();
    public static IEnumerable<T> QuerySelectorAll(T root, string query) =>
        QuerySelectorAll(root, query, root.GetDescendants());
    private static IEnumerable<T> QuerySelectorAll(T root, string query, IEnumerable<T> source)
    {
        if (string.IsNullOrEmpty(query))
            yield break;

        // fix [:nth-child(2n + 1)] to [:nth-child(2n+1)]
        var q = new StringProcessor(NthChildRegex().Replace(query, m => m.Groups[0].Value.Replace(" ", ""))).Trim();

        if (q.Contains(','))
        {
            var concatQuery = q.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
            if (concatQuery.Length > 1)
            {
                var tags = concatQuery.SelectMany(x => QuerySelectorAll(root, x.ToString())).Distinct();
                foreach (var tag in tags)
                    yield return tag;

                yield break;
            }
            else
            {
                throw new ArgumentException(null, nameof(query));
            }
        }

        var split = q.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (split.Length == 1)
        {
            var querySource = source;
            var currentQuery = split[0].ToString();
            var currentPosition = 0;
            var currentSemiQuery = NextQuery.Tag;
            var nextSemiQuery = NextQuery.Tag; //0=root,1=id,2=class,3=att,4=att_end,5=pesuder
            for (var i = 0; i < currentQuery.Length; i++)
            {
                if (currentQuery[i] == '#')
                    nextSemiQuery = NextQuery.Id;
                else if (currentQuery[i] == '@')
                    nextSemiQuery = NextQuery.ElementName;
                else if (currentQuery[i] == '.')
                    nextSemiQuery = NextQuery.Class;
                else if (currentQuery[i] == '[')
                    nextSemiQuery = NextQuery.Attribute;
                else if (currentPosition == 3 && currentQuery[i] == ']')
                {
                    nextSemiQuery = NextQuery.AttributeEnd;
                    continue;
                }
                else if (currentQuery[i] == ':' && nextSemiQuery != NextQuery.ElementName)
                    nextSemiQuery = NextQuery.Pseudo;

                //if (currentSemiQuery > nextSemiQuery && !(currentSemiQuery == 4 && nextSemiQuery == 3))
                //    throw new ArgumentException(nameof(currentQuery));

                if (currentSemiQuery != nextSemiQuery && currentPosition < i)
                {
                    var nextQuery = QuerySelectorAllInternal(querySource, currentQuery, currentSemiQuery, nextSemiQuery, currentPosition, i);
                    if (nextQuery != null)
                        querySource = nextQuery;
                    currentPosition = i;
                }

                currentSemiQuery = nextSemiQuery;
            }

            if (currentPosition < currentQuery.Length)
            {
                var nextQuery = QuerySelectorAllInternal(querySource, currentQuery, currentSemiQuery, nextSemiQuery, currentPosition, currentQuery.Length);
                if (nextQuery != null)
                    querySource = nextQuery;
            }

            foreach (var result in querySource)
                yield return result;
        }
        else
        {
            source = QuerySelectorAll(root, split[0].ToString(), source);

            var from = 1;
            if (split[1][0] == '>')
            {
                from = 2;
                source = source.SelectMany(x => x.Children);
            }
            else
            {
                source = source.SelectMany(x => x.GetDescendants());
            }

            foreach (var result in QuerySelectorAll(root, string.Join(' ', split.Skip(from)), source))
                yield return result;
        }
    }

    private enum NextQuery
    {
        Tag = 0,
        Id = 1,
        Class = 2,
        Attribute = 3,
        AttributeEnd = 4,
        Pseudo = 5,
        ElementName = 6
    }

    private static IEnumerable<T> QuerySelectorAllInternal(IEnumerable<T> querySource,
        string currentQuery, NextQuery currentSemiQuery, NextQuery nextSemiQuery, int currentPosition, int i)
    {
        if (currentSemiQuery is NextQuery.Tag or NextQuery.ElementName)
        {
            var tagName = currentQuery[(currentSemiQuery == NextQuery.Tag ? currentPosition : (currentPosition + 1))..i];
            return querySource.Where(x => x.TagName?.Equals(tagName, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        if (currentSemiQuery == NextQuery.Id)
        {
            var id = currentQuery[(currentPosition + 1)..i];
            var found = querySource.Where(x => x.GetId() == id).FirstOrDefault();
            return found != null ? Enumerable.Repeat(found, 1) : [];
        }

        if (currentSemiQuery == NextQuery.Class)
        {
            var classes = currentQuery[(currentPosition + 1)..i];
            var target = classes.Split('.');
            return querySource.Where(x => ContainsAllClasses(x, target));
        }

        if (currentSemiQuery == NextQuery.AttributeEnd && nextSemiQuery == NextQuery.Attribute)
        {
            currentPosition = i;
        }

        if (currentSemiQuery == NextQuery.Attribute)
        {
            var attribute = currentQuery[(currentPosition + 1)..(i - 1)].Split('=');
            var name = attribute[0].Trim();
            var value = attribute.Length == 1 ? null : attribute[1].Trim();
            if (value != null)
            {
                if (value[0] == '\'') value = value.Trim('\'');
                else if (value[0] == '\"') value = value.Trim('\"');
            }
            return querySource.Where(x => value == null ?
                x.GetAttribute(name) != null :
                x.GetAttribute(name)?.Equals(value, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        if (currentSemiQuery == NextQuery.Pseudo)
        {
            var pseudos = currentQuery[(currentPosition + 1)..].Split(':');
            return WithPseudos(querySource, pseudos);
        }

        return [];
    }
    private static IEnumerable<T> WithPseudos(IEnumerable<T> querySource, IEnumerable<string> pseudos)
    {
        if (pseudos != null)
        {
            foreach (var pseudo in pseudos)
            {
                if (pseudo.Equals("first-child", StringComparison.OrdinalIgnoreCase))
                    querySource = querySource.Take(1);
                else if (pseudo.Equals("last-child", StringComparison.OrdinalIgnoreCase))
                    querySource = querySource.Reverse().Take(1);
                else if (pseudo.StartsWith("not(", StringComparison.OrdinalIgnoreCase))
                {
                    var not = pseudo.GetTextBetween("(", ")");
                    querySource = querySource.Where(x => !IsQueryMatch(x, not!));
                }
                else if (pseudo.StartsWith("nth-child(", StringComparison.OrdinalIgnoreCase))
                {
                    var nth = pseudo.GetTextBetween("(", ")")!.Trim();
                    if (nth.Equals("odd", StringComparison.OrdinalIgnoreCase)) nth = "2n-1";
                    else if (nth.Equals("even", StringComparison.OrdinalIgnoreCase)) nth = "2n+0";
                    if (int.TryParse(nth, out int i))
                    {
                        querySource = querySource.Skip(i - 1).Take(1);
                    }
                    else
                    {
                        if (!nth.Contains('-') && !nth.Contains('+'))
                            nth += "+0";

                        var indexOf = nth.IndexOfAny(['-', '+']);
                        var n = int.Parse(nth[..(indexOf - 1)]);
                        var offest = int.Parse(nth[indexOf..].Replace("+", "")) * -1;

                        querySource = querySource.WhereIndex((tag, index) =>
                        {
                            var result = 1 + index + offest;
                            return result > 0 && result % n == 0;
                        });
                    }
                }
                else
                    throw new ArgumentException(pseudo, nameof(pseudos));
            }
        }
        return querySource;
    }

    public static bool IsQueryMatch(T tag, string query)
    {
        query = query.Trim();

        if (string.IsNullOrEmpty(query) || query.Contains(' ') || query.Contains(':'))
            throw new ArgumentException(null, nameof(query));

        return QuerySelectorAll(tag, query, [tag]).Any();
    }

    [GeneratedRegex(":nth-child\\(.+\\)", RegexOptions.Compiled)]
    private static partial Regex NthChildRegex();
}
