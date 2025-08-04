using MediatorCore;

namespace RoeiBajayo.Infrastructure.TextToSpeech.Models;

internal record SpeechMessage(TextToSpeechLanguages Language, string Text, bool Persist) : IQueueMessage;