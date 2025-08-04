using System;
using System.Collections.Generic;
using System.Linq;

namespace RoeiBajayo.Infrastructure.Text;

public sealed class HtmlTag : ICssSelectable<HtmlTag>
{
    internal static readonly HashSet<string> AutoselfCloseTags =
        ["br", "hr", "input", "textarea", "img", "link", "meta", "iframe"];

    internal StringProcessor outerHtml;
    internal bool open = true;

    private readonly Lazy<string?> tagName;
    private readonly Lazy<IEnumerable<KeyValuePair<string, string>>> attributes;
    private readonly Lazy<StringProcessor> innerHtml;
    internal List<HtmlTag> children = [];

    public HtmlTag()
    {
        tagName = new Lazy<string?>(GetTagName);
        attributes = new Lazy<IEnumerable<KeyValuePair<string, string>>>(GetAttributesInternal);
        innerHtml = new Lazy<StringProcessor>(GetInnerHtml);
        outerHtml = StringProcessor.Empty;
    }
    internal HtmlTag(StringProcessor otherHtml) : this()
    {
        outerHtml = otherHtml;
    }

    public HtmlTag? Parent { get; internal set; }
    public IReadOnlyList<HtmlTag> Children => children;

    public StringProcessor OuterHtml =>
        outerHtml!;
    public StringProcessor InnerHtml =>
        innerHtml.Value;
    public string? TagName =>
        tagName.Value;
    public IEnumerable<KeyValuePair<string, string>> Attributes =>
        attributes.Value;
    public string GetAttribute(string attributeName)
    {
        if (string.IsNullOrEmpty(attributeName))
            throw new ArgumentNullException(nameof(attributeName));

        return Attributes
            .FirstOrDefault(x => x.Key.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            .Value;
    }

    private string? GetTagName()
    {
        if (!outerHtml!.StartsWith('<'))
            return null;

        if (outerHtml.StartsWith("</"))
            return outerHtml.Cut(2, -1).Trim().ToString();

        var content = outerHtml.CutFrom(1).TrimStart();
        var indexOf = content.IndexOfAny(1, [' ', '>']);
        content = content.CutTo(indexOf);
        if (content.EndsWith('/'))
            content = content.CutTo(-1);
        return content.ToString();
    }
    private IEnumerable<KeyValuePair<string, string>> GetAttributesInternal()
    {
        if (!outerHtml!.TrimStart().StartsWith('<'))
            yield break;

        var firstAttributeIndex = outerHtml.IndexOfAny([' ', '>']); //navigate to the end of root's name

        if (firstAttributeIndex == -1)
            yield break;

        if (outerHtml[firstAttributeIndex] == '>') //contains attributes and not end of root
            yield break;

        //get first root only, from first attribute and on
        char? cWrapper = null;
        var content = outerHtml.Cut(firstAttributeIndex + 1, -1).Trim();
        if (content.EndsWith('/'))
            content = content.CutTo(-1).Trim();
        var to = -1;
        var from = 0;
        var anyOf = new[] { '\'', '\"', ' ', '>' };
        char c;
        while ((to = content.IndexOfAny(to + 1, anyOf)) != -1)
        {
            //a1 a2="a3"
            //a1 a2
            c = content[to];
            if (cWrapper is null)
            {
                if (c == ' ')
                {
                    if (from != to) //not empty
                    {
                        yield return ParseAttribute(content.Cut(from, to));
                    }

                    from = to + 1;
                    continue;
                }
                else if (c == '>')
                {
                    break;
                }
                else // if (c == '\'' || c == '\"')
                {
                    cWrapper = c;
                    anyOf = [c];
                }
            }
            else
            {
                if (c == cWrapper &&
                    content[to - 1] != '\\' &&
                    (content.Length == to || content.Length == to + 1 || content[to + 1] == ' ' || content[to + 1] == '>'))
                {
                    //end of wrapper and not escaped or end of root or no space like this: "aaaa"a"
                    cWrapper = null;
                    yield return ParseAttribute(content.Cut(from, to + 1));
                    anyOf = ['\'', '\"', ' ', '>'];
                    from = to + 1;
                }
            }
        }

        if (from != to && to != -1)
            yield return ParseAttribute(content.Cut(from == -1 ? 0 : from, to));


    }
    private static KeyValuePair<string, string> ParseAttribute(StringProcessor att)
    {
        var indexOf = att.IndexOf('=');
        string? value = null;

        if (indexOf != -1)
        {
            var val = att.CutFrom(indexOf + 1);
            var first = val[0];
            if (first == '\'' || first == '\"')
            {
                value = val.Cut(1, -1).ToString().Replace("\\" + first, first.ToString());
            }
            else
            {
                value = val.ToString();
            }
        }

        return new KeyValuePair<string, string>(
            indexOf == -1 ? att.ToString() : att.GetRangeTo(indexOf),
            value ?? "true");
    }
    private StringProcessor GetInnerHtml()
    {
        if (outerHtml is null)
            return StringProcessor.Empty;

        if (outerHtml.EndsWith("/>"))
            return StringProcessor.Empty;

        if (TagName is not null && AutoselfCloseTags.Contains(TagName))
            return StringProcessor.Empty;

        var from = outerHtml.IndexOf('>') + 1;

        if (from == -1)
            return StringProcessor.Empty;

        if (from == outerHtml.Length - 1)
            return StringProcessor.Empty;

        var to = outerHtml.LastIndexOf('<');
        if (to <= 0)
            return StringProcessor.Empty;

        return outerHtml.Cut(from, to);
    }

    internal void CloseChildren()
    {
        var count = children.Count;
        for (var i = 0; i < count; i++)
        {
            children[i].CloseChildren();

            if (!Children[i].open)
                continue;

            children[i].children.ForEach(child => child.Parent = this);
            children.AddRange(children[i].children);
            children[i].children.Clear();
            children[i].open = false;
        }
    }


    public string InnerText =>
        InnerTextNonTrim.Trim();
    public string InnerTextNonTrim =>
        System.Web.HttpUtility.HtmlDecode(InnerHtml.ToString().RemoveHtmlTags(true));

    IEnumerable<HtmlTag> ICssSelectable<HtmlTag>.Children =>
        Children;

    public override string ToString()
    {
        return OuterHtml.ToString();
    }
}
