using MediatorCore;

namespace Infrastructure.Utils.TextToSpeech.Models;

internal record SpeechMessage(TextToSpeechLanguages Language, string Text, bool Persist) : IQueueMessage;