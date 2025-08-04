using RoeiBajayo.Infrastructure.Text;
using Xunit;
using System;
using System.Linq;

namespace UnitTestProject;



public class Text
{
    [Fact]
    public void ReplaceBetween()
    {
        var source = "abcdefg";

        var result = source.ReplaceBetween("d", "f", "5");
        Assert.Equal("abcd5fg", result);

        result = source.ReplaceBetween("a", "f", "8");
        Assert.Equal("a8fg", result);

        result = source.ReplaceBetween("a", "d", "2");
        Assert.Equal("a2defg", result);

        result = source.ReplaceBetween("f", "g", "6");
        Assert.Equal("abcdef6g", result);

        result = source.RemoveBetween(0, 3);
        Assert.Equal("defg", result);

        result = source.RemoveBetween(3, 4);
        Assert.Equal("abcefg", result);

        result = source.RemoveBetween(3, 5);
        Assert.Equal("abcfg", result);

        result = source.RemoveBetween(6, 7);
        Assert.Equal("abcdef", result);
    }

    [Fact]
    public void StringProcessor()
    {
        var source = "abcdefg";
        var result = new StringProcessor(source);

        Assert.Equal(source.IndexOf("a"), result.IndexOf("a"));
        Assert.Equal(source.IndexOf("a", 1), result.IndexOf("a", 1));
        Assert.Equal(source.LastIndexOf("f"), result.LastIndexOf("f"));
        Assert.Equal(source.LastIndexOf("f", source.Length), result.LastIndexOf("f", source.Length));
        Assert.Equal(source.GetTextBetween("b", "e"), result.GetTextBetween("b", "e"));
        Assert.Equal(source.GetTextBetween("b", "e"), result.GetTextBetween("B", "E", stringComparison: StringComparison.OrdinalIgnoreCase));
        Assert.NotEqual(source.GetTextBetween("b", "e"), result.GetTextBetween("B", "E").ToString());

        Assert.Equal(source.StartsWith("abc"), result.StartsWith("abc"));
        Assert.Equal(source.StartsWith('a'), result.StartsWith('a'));
        Assert.Equal(source.StartsWith('g'), result.StartsWith('g'));
        Assert.Equal(source.EndsWith("efg"), result.EndsWith("efg"));

        Assert.Equal(source.EndsWith("abc"), result.EndsWith("abc"));
        Assert.Equal(source.EndsWith('a'), result.EndsWith('a'));
        Assert.Equal(source.EndsWith('g'), result.EndsWith('g'));
        Assert.Equal(source.StartsWith("efg"), result.StartsWith("efg"));

        Assert.Equal(source[..1], result.Substring(0, 1));
        Assert.Equal(source[..4], result.Substring(0, 4));
        Assert.Equal(source.Substring(4, 2), result.Substring(4, 2));

        Assert.Equal(source[0..6], result.GetRange(0, 6));
        Assert.Equal(source[4..6], result.GetRange(4, 6));
        Assert.Equal(source[4..], result.GetRangeFrom(4));
        Assert.Equal(source[..4], result.GetRangeTo(4));

        source = "  abcdefg ";
        result = new StringProcessor(source);
        Assert.Equal(source.Trim(), result.Trim());
        Assert.Equal(source.TrimStart(), result.TrimStart());
        Assert.Equal(source.TrimEnd(), result.TrimEnd());


        source = "a1a2a3a";
        result = new StringProcessor(source);

        var a = source.Split('a');
        var b = result.Split('a').ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

        a = source.Split('1');
        b = result.Split('1').ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

        a = source.Split('4');
        b = result.Split('4').ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

        a = source.Split("a1");
        b = result.Split("a1").ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

        a = source.Split("a2");
        b = result.Split("a2")  .ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

        a = source.Split("a4");
        b = result.Split("a4").ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

        a = source.Split("a");
        b = result.Split("a").ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            Assert.Equal(a[i], b[i]);
        }

    }

    [Fact]
    public void Html()
    {
        var source = "<html>" +
            "<body>" +
            "<title class=\"c1\">My>Title</title>" +
            "<br /><br>" +
            "<a id=\"atts\" att1 att2=\"att\\\"2val\" att3='att3val' att4 />" +
            "<div id=\"asd1\">" +
            "<a href=\"a1\">a1t<b>e</b>xt</a>" +
            "<a claSs=\"c1 c2 c3\" href='a2' id=\"myA\">a2te<asd />xt</a>" +
            "<a href=a3>a3text</a>" +
            "</div>" +
            "<span>roei</span>" +
            "<div><a href=\"google\"><span class=\"c3\">roei2</span></a><body></div>" +
            "<test><body></test></boDY>" +
            "</html>";

        var html = HtmlTagParser.Parse(source);

        Assert.Equal("html", html.TagName);
        Assert.Equal("My>Title", html.QuerySelector("title").InnerText);

        Assert.Equal(2, html.QuerySelectorAll("div").Count());
        Assert.Equal(3, html.QuerySelector("div").Children.Count());
        Assert.Equal(5, html.QuerySelector("div").GetDescendants().Count());
        Assert.Single(html.QuerySelectorAll("#asd1"));
        Assert.Empty(html.QuerySelectorAll("#asd"));
        Assert.Single(html.QuerySelectorAll("#asd1 .c1"));
        Assert.Single(html.QuerySelectorAll("#asd1 a.c1"));
        Assert.Single(html.QuerySelectorAll("#asd1 a.c1:first-child"));
        Assert.Empty(html.QuerySelectorAll("#asd1 b.c1"));
        Assert.Equal(3, html.QuerySelectorAll("#asd1 a").Count());
        Assert.Single(html.QuerySelectorAll("#asd1 a:first-child"));
        Assert.Equal(2, html.QuerySelectorAll(".c1").Count());
        Assert.Single(html.QuerySelectorAll("[href=a1]"));
        Assert.Single(html.QuerySelectorAll("[href=\"a1\"]"));
        Assert.Single(html.QuerySelectorAll("[href='a1']"));
        Assert.Single(html.QuerySelectorAll("div [href=a1]"));
        Assert.Single(html.QuerySelectorAll(".c2"));
        Assert.Single(html.QuerySelectorAll(".c1.c2"));
        Assert.Equal(2, html.QuerySelectorAll("br").Count());
        Assert.Equal(4, html.QuerySelectorAll("br,span").Count());
        Assert.Equal(2, html.QuerySelectorAll("div .c3").Count());

        Assert.Single(html.QuerySelectorAll("div > .c3"));
        Assert.Single(html.QuerySelectorAll("div > a .c3"));
        Assert.Single(html.QuerySelectorAll("div > a > .c3"));
        Assert.Single(html.QuerySelectorAll("div > [href=google] .c3"));
        Assert.Single(html.QuerySelectorAll("div > [href=google] > .c3"));

        var first = html.QuerySelectorAll("#asd1 a:first-child").ToArray();
        Assert.Single(first);
        Assert.Equal("a1", first[0].GetAttribute("href"));

        first = html.QuerySelectorAll("#asd1 > a:first-child").ToArray();
        Assert.Single(first);
        Assert.Equal("a1", first[0].GetAttribute("href"));

        first = html.QuerySelectorAll("#asd1 > a:nth-child(1)").ToArray();
        Assert.Single(first);
        Assert.Equal("a1", first[0].GetAttribute("href"));

        var last = html.QuerySelectorAll("#asd1 a:last-child").ToArray();
        Assert.Single(last);
        Assert.Equal("a3", last[0].GetAttribute("href"));

        last = html.QuerySelectorAll("#asd1 > a:last-child").ToArray();
        Assert.Single(last);
        Assert.Equal("a3", last[0].GetAttribute("href"));

        last = html.QuerySelectorAll("#asd1 > a:nth-child(3)").ToArray();
        Assert.Single(last);
        Assert.Equal("a3", last[0].GetAttribute("href"));

        var nth = html.QuerySelectorAll("#asd1 > a:nth-child(2n-1)").ToArray(); // 1,3
        Assert.Equal(2, nth.Length);
        Assert.Equal("a1", nth[0].GetAttribute("href"));
        Assert.Equal("a3", nth[1].GetAttribute("href"));

        nth = html.QuerySelectorAll("#asd1 > a:nth-child(1n + 1)").ToArray(); // 2,3
        Assert.Equal(2, nth.Length);
        Assert.Equal("a2", nth[0].GetAttribute("href"));
        Assert.Equal("a3", nth[1].GetAttribute("href"));

        nth = html.QuerySelectorAll("#asd1 > a:nth-child(2n )").ToArray(); // 2,4,6
        Assert.Single(nth);
        Assert.Equal("a2", nth[0].GetAttribute("href"));

        var attsEle = html.QuerySelector("#atts");
        var atts = attsEle.Attributes.ToDictionary(x => x.Key, x => x.Value);
        Assert.Equal(5, atts.Count);
        Assert.Equal("atts", atts["id"]);
        Assert.Equal("true", atts["att1"]);
        Assert.Equal("att\"2val", atts["att2"]);
        Assert.Equal("att3val", atts["att3"]);
        Assert.Equal("true", atts["att4"]);

        var aTags = html.QuerySelectorAll("a").ToArray();

        Assert.Equal(5, aTags.Length);

        aTags = aTags.Skip(1).ToArray();

        Assert.Equal("a1", aTags[0].GetAttribute("href"));
        Assert.Equal("a1text", aTags[0].InnerText);
        Assert.Equal("a1t<b>e</b>xt", aTags[0].InnerHtml);

        Assert.Equal("a2", aTags[1].GetAttribute("href"));
        Assert.Equal("a2text", aTags[1].InnerText);
        Assert.Equal("a2te<asd />xt", aTags[1].InnerHtml);

        Assert.False(aTags[1].IsQueryMatch("div"));
        Assert.False(aTags[1].IsQueryMatch("a1"));
        Assert.True(aTags[1].IsQueryMatch("a"));
        Assert.False(aTags[1].IsQueryMatch(".a"));
        Assert.True(aTags[1].IsQueryMatch(".c1"));
        Assert.True(aTags[1].IsQueryMatch(".c1.c2"));
        Assert.True(aTags[1].IsQueryMatch(".c1.c2.c3"));
        Assert.True(aTags[1].IsQueryMatch(".c1.c3"));
        Assert.False(aTags[1].IsQueryMatch(".c4"));
        Assert.False(aTags[1].IsQueryMatch(".c1.c4"));

        Assert.True(aTags[1].IsQueryMatch("[href=a2]"));
        Assert.True(aTags[1].IsQueryMatch("[href='a2']"));
        Assert.True(aTags[1].IsQueryMatch("[href=\"a2\"]"));
        Assert.True(aTags[1].IsQueryMatch("#myA"));
        Assert.True(aTags[1].IsQueryMatch("#myA[href=a2]"));
        Assert.True(aTags[1].IsQueryMatch("#myA.c1"));
        Assert.True(aTags[1].IsQueryMatch("#myA.c1[href=a2]"));
        Assert.True(aTags[1].IsQueryMatch(".c1[href=a2]"));

        Assert.False(aTags[1].IsQueryMatch("[href=a3]"));
        Assert.False(aTags[1].IsQueryMatch("[href='a3']"));
        Assert.False(aTags[1].IsQueryMatch("[href=\"a3\"]"));
        Assert.False(aTags[1].IsQueryMatch("#myA2"));
        Assert.False(aTags[1].IsQueryMatch("#myA2[href=a2]"));
        Assert.False(aTags[1].IsQueryMatch("#myA2.c1"));
        Assert.False(aTags[1].IsQueryMatch("#myA.c5"));
        Assert.False(aTags[1].IsQueryMatch("#myA2.c1[href=a2]"));
        Assert.False(aTags[1].IsQueryMatch("#myA.c5[href=a2]"));
        Assert.False(aTags[1].IsQueryMatch(".c5[href=a2]"));

        Assert.Equal("a3", aTags[2].GetAttribute("href"));
        Assert.Equal("a3text", aTags[2].InnerText);
        Assert.Equal("a3text", aTags[2].InnerHtml);
    }
}