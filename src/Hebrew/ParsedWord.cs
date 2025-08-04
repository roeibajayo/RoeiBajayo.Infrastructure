namespace Infrastructure.Utils.Hebrew;

public class ParsedWord(int index, string text, WordType type)
{
    public int Index { get; set; } = index;
    public string Text { get; set; } = text;
    public WordType Type { get; set; } = type;
    public int Length => Text.Length;
    public override string ToString() => Text;
}
