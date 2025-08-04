namespace RoeiBajayo.Infrastructure.Http.Models;

public class TextBodyRestCallOptions : BodyRestCallOptions
{
    public required string Text { get; set; }
}