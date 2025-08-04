using Infrastructure.Utils.TextToSpeech.Models;

namespace Infrastructure.Utils.TextToSpeech.Interfaces;

public interface ITextToSpeech : ITextToSpeechService
{
    /// <summary>
    /// Plays the provided text as speech in Hebrew.
    /// </summary>
    /// <param name="text">The text to be spoken.</param>
    void Speech(string text) =>
        Speech(TextToSpeechLanguages.Hebrew, text, false);

    /// <summary>
    /// Plays the provided text as speech in Hebrew, with an option to persist the audio file.
    /// </summary>
    /// <param name="text">The text to be spoken.</param>
    /// <param name="persist">Whether to persist the audio file, so no API call will be made next time.</param>
    void Speech(string text, bool persist) =>
        Speech(TextToSpeechLanguages.Hebrew, text, persist);

    /// <summary>
    /// Plays the provided text as speech in the specified language, with an option to persist the audio file.
    /// </summary>
    /// <param name="language">The language in which the text should be spoken.</param>
    /// <param name="text">The text to be spoken.</param>
    /// <param name="persist">Whether to persist the audio file, so no API call will be made next time.</param>
    void Speech(TextToSpeechLanguages language, string text, bool persist);
}
