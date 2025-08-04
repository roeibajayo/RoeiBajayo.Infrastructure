using Infrastructure.Utils.DependencyInjection.Interfaces;
using Infrastructure.Utils.TextToSpeech.Interfaces;
using Infrastructure.Utils.TextToSpeech.Models;
using MediatorCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Utils.TextToSpeech;

internal class TextToSpeech(
    IPublisher publisher,
    ILogger<TextToSpeech> logger,
    IServiceProvider serviceProvider)
    : ITextToSpeech, IScopedService<ITextToSpeech>
{
    public Task<byte[]> GenerateAsync(TextToSpeechFormats format, TextToSpeechLanguages language, string text)
    {
        var service = serviceProvider.GetService<ITextToSpeechService>();

        if (service is null)
        {
            logger.LogWarning("ITextToSpeechService is not registered. Cannot generate speech.");
            return Task.FromResult(Array.Empty<byte>());
        }

        return service.GenerateAsync(format, language, text);
    }

    public void Speech(TextToSpeechLanguages language, string text, bool persist)
    {
        publisher.Publish(new SpeechMessage(language, text, persist));
    }
}
