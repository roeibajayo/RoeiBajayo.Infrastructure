using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Text;

public sealed class HtmlTagParser
{
    private const int STREAM_BUFFER_SIZE = 100 * 1024;

    public async static Task<HtmlTag> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var parser = new HtmlTagParser();

        using var reader = new StreamReader(stream);
        return await parser.ParseAsync(reader.ToChunksAsync(STREAM_BUFFER_SIZE, cancellationToken: cancellationToken));
    }

    public static HtmlTag Parse(string htmlDocument)
    {
        ArgumentNullException.ThrowIfNull(htmlDocument);

        var parser = new HtmlTagParser();
        return parser.Parse(Enumerable.Repeat(htmlDocument.Trim(), 1));
    }

    private async Task<HtmlTag> ParseAsync(IAsyncEnumerable<string> chunks)
    {
        PrepareParse();

        await foreach (var chunk in chunks)
        {
            AppendChunk(chunk);
        }

        CompleteParse();
        return root!;
    }
    private HtmlTag Parse(IEnumerable<string> chunks)
    {
        PrepareParse();

        foreach (var chunk in chunks)
        {
            AppendChunk(chunk);
        }

        CompleteParse();
        return root!;
    }

    private void PrepareParse()
    {
        anyOf = ['<'];
        first = true;
        html = StringProcessor.Empty;
    }
    private void CompleteParse()
    {
        if (root is null)
            return;

        root.outerHtml = html;

        if (root.outerHtml.EndsWith('>'))
        {
            var indexOf = root.outerHtml.LastIndexOf('<');
            if (indexOf != -1)
            {
                root.outerHtml = root.outerHtml.CutTo(indexOf);
            }
        }

        root.CloseChildren();
    }

    private void AppendChunk(string chunk)
    {
        html.Append(chunk);

        //remove self
        if (first)
        {
            first = false;
            if (html.StartsWith('<'))
            {
                var indexOf = html.IndexOf('>');
                if (indexOf != -1)
                {
                    var newHtml = html.CutFrom(indexOf + 1);

                    // skip only if its  <!DOCUMENT>
                    if (newHtml.StartsWith("<!"))
                    {
                        html = newHtml;
                    }
                }
            }
        }

        ContinuousParse(html);
    }

    bool first;
    StringProcessor html = new();
    char? cWrapper;
    int currentStartIndex;
    HtmlTag? root;
    HtmlTag? parent;
    char[] anyOf = [];
    bool inTagContext;
    int i = -1;
    private void ContinuousParse(StringProcessor html)
    {
        char currentChar;
        int newI;
        while ((newI = html.IndexOfAny(i + 1, anyOf)) != -1)
        {
            i = newI;
            currentChar = html[i];

            if (inTagContext)
            {
                if (cWrapper == null && (currentChar == '\'' || currentChar == '\"') && html[i - 1] == '=')
                {
                    //attribute context
                    cWrapper = currentChar;
                    anyOf = [cWrapper.Value];
                    continue;
                }
                else if (currentChar == cWrapper && html[i - 1] != '\\')
                {
                    //exit attribute context
                    cWrapper = null;
                    anyOf = ['>', '\'', '\"'];
                    continue;
                }
            }

            if (currentChar == '>')
            {
                inTagContext = false;
                anyOf = ['<'];

                //its completed tag like: <asd>, </asd>, <asd/>, <asd a1="roei">
                var tag = new HtmlTag(html.Cut(currentStartIndex, i + 1))
                {
                    Parent = parent
                };

                currentStartIndex = -1;

                //check autoclose
                if (tag.outerHtml.EndsWith("/>") || tag.outerHtml.StartsWith("<!"))
                {
                    tag.open = false;
                    if (!tag.TagName!.Equals("!doctype", StringComparison.OrdinalIgnoreCase))
                        parent?.children.Add(tag);
                    continue;
                }

                var tagName = tag.TagName;

                if (tagName is not null)
                {
                    if (tagName.Equals("script", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!tag.outerHtml.StartsWith("</") && !tag.outerHtml.EndsWith("/>"))
                        {
                            newI = html.IndexOf("</script>", i, StringComparison.OrdinalIgnoreCase);
                            if (newI != -1)
                            {
                                i = newI + "</script>".Length - 1;
                                tag.outerHtml.endIndex = html.startIndex + i + 1;
                                tag.open = false;
                                parent?.children.Add(tag);
                                continue;
                            }
                        }
                    }
                    else if (tagName.Equals("style", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!tag.outerHtml.StartsWith("</") && !tag.outerHtml.EndsWith("/>"))
                        {
                            newI = html.IndexOf("</style>", i, StringComparison.OrdinalIgnoreCase);
                            if (newI != -1)
                            {
                                i = newI + "</style>".Length - 1;
                                tag.outerHtml.endIndex = html.startIndex + i + 1;
                                tag.open = false;
                                parent?.children.Add(tag);
                                continue;
                            }
                        }
                    }


                    //check autoclose
                    if (HtmlTag.AutoselfCloseTags.Contains(tagName))
                    {
                        tag.open = false;
                        parent?.children.Add(tag);
                        continue;
                    }

                }

                //check close
                if (tag.outerHtml.StartsWith("</"))
                {
                    var currentTagParent = parent;

                    while (currentTagParent != null && (currentTagParent.TagName != tagName || !currentTagParent.open))
                        currentTagParent = currentTagParent.Parent;

                    if (currentTagParent != null) //starting tag found
                    {
                        currentTagParent.open = false;
                        currentTagParent.outerHtml.endIndex = tag.outerHtml.endIndex;
                        parent = currentTagParent.Parent ?? this.root;
                    }
                    //else: ignore tag
                    continue;
                }

                //its opening tag:
                if (parent == null)
                {
                    this.root = tag;
                }
                else
                {
                    parent.children.Add(tag);
                }

                parent = tag;
                continue;
            }

            if (currentChar == '<')
            {
                if (i + 1 < html.Length && html[i + 1] == '!')
                {
                    if (i + 7 <= html.Length && html.Substring(i, 5) == "<!--[if")
                    {
                        newI = html.IndexOf("if]>", i + 7);
                        if (newI != -1)
                        {
                            var indexOf = newI + 4 - 1;
                            if (indexOf > 0)
                            {
                                i = indexOf;
                                continue;
                            }
                        }
                    }
                    if (i + 5 <= html.Length && html.Substring(i, 5) == "<!--[")
                    {
                        newI = html.IndexOf("]>", i + 5);
                        if (newI != -1)
                        {
                            var indexOf = newI + 2 - 1;
                            if (indexOf > 0)
                            {
                                i = indexOf;
                                continue;
                            }
                        }
                    }
                    if (i + 4 <= html.Length && html.Substring(i, 4) == "<!--")
                    {
                        newI = html.IndexOf("-->", i + 4);
                        if (newI != -1)
                        {
                            var indexOf = newI + 3 - 1;
                            if (indexOf > 0)
                            {
                                i = indexOf;
                                continue;
                            }
                        }
                    }
                }

                currentStartIndex = i;
                inTagContext = true;
                anyOf = ['>', '\'', '\"'];
                continue;
            }
        }
    }
}
