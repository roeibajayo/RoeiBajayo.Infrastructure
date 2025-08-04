using MediatorCore;

namespace RoeiBajayo.Infrastructure.Social.Models;

public class TelegramPushMessageRequest : IQueueMessage
{
    public required string Key { get; set; }
    public required string Channel { get; set; }
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public ParseModes ParseMode { get; set; } = ParseModes.MarkdownV2;
    public bool Escape { get; set; }
    public bool DisableNotification { get; set; }
}
