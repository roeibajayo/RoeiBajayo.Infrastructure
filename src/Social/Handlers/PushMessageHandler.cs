using RoeiBajayo.Infrastructure.Http;
using RoeiBajayo.Infrastructure.Http.Models;
using RoeiBajayo.Infrastructure.Social.Models;
using MediatorCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Social.Handlers;

internal class PushMessageHandler(IServiceScopeFactory serviceScopeFactory) :
    IQueueHandler<TelegramPushMessageRequest>
{
    public async Task HandleAsync(TelegramPushMessageRequest message)
    {
        var isPhoto = !string.IsNullOrEmpty(message.ImageUrl);

        var apiMethod = isPhoto ? "sendPhoto" : "sendMessage";
        var querystring = new Dictionary<string, object?>()
        {
            { "chat_id", message.Channel },
            { "parse_mode", message.ParseMode },
            { "disable_notification", message.DisableNotification }
        };

        if (isPhoto)
        {
            querystring.Add("photo", message.ImageUrl!);

            if (!string.IsNullOrEmpty(message.Text))
                querystring.Add("caption", message.Escape ? TelegramUtils.EscapeMarkdownV2Text(message.Text) : message.Text);
        }
        else
        {
            if (string.IsNullOrEmpty(message.Text))
                throw new ArgumentException("Text is required for sending a message");

            querystring.Add("text", message.Escape ? TelegramUtils.EscapeMarkdownV2Text(message.Text) : message.Text);
        }

        using var scope = serviceScopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IRestClient>();
        await client.GetAsync(apiMethod,
            new RestCallOptions
            {
                BaseUrl = $"https://api.telegram.org/bot{message.Key}",
                Querystring = querystring
            });
    }

    public Task? HandleExceptionAsync(TelegramPushMessageRequest messages, Exception exception, int retries, Func<Task> retry)
    {
        //log is not needed here, the IRestClient will log the exception
        return Task.CompletedTask;
    }
}
