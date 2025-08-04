using RoeiBajayo.Infrastructure.TextToSpeech.Interfaces;
using RoeiBajayo.Infrastructure.TextToSpeech.Models;
using MediatorCore;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.TextToSpeech;

internal class SpeechHandler(ILogger<SpeechHandler> logger, ITextToSpeech service) : IQueueHandler<SpeechMessage>
{
    public async Task HandleAsync(SpeechMessage message)
    {
        var filepath = message.Persist ? ("mp3\\speech_" + message.Text.GetStaticHashCode() + ".mp3") : null;

        if (filepath is not null && File.Exists(filepath))
        {
            PlayMp3(File.ReadAllBytes(filepath));
            return;
        }

        var bytes = await service.GenerateAsync(TextToSpeechFormats.Mp3, message.Language, message.Text);

        if (bytes.Length == 0)
            return;

        if (filepath is not null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filepath)!);
            File.WriteAllBytes(filepath, bytes);
        }

        PlayMp3(bytes);
    }


    private static void PlayMp3(byte[] bytes)
    {
        using var mp3 = new MemoryStream(bytes);
        using var audioFile = new Mp3FileReader(mp3);
        using var outputDevice = new WaveOutEvent();
        outputDevice.Init(audioFile);
        outputDevice.Play();

        var wait = new ManualResetEvent(false);
        outputDevice.PlaybackStopped += (sender, args) => wait.Set();
        wait.WaitOne();
    }

    public Task? HandleExceptionAsync(SpeechMessage message, Exception exception, int retries, Func<Task> retry)
    {
        logger.LogError(exception, "Failed to handle message {message}", message);
        return Task.CompletedTask;
    }
}
