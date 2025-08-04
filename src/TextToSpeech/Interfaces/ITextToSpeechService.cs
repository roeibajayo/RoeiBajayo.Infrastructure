using Infrastructure.Utils.TextToSpeech.Models;
using System.Threading.Tasks;

namespace Infrastructure.Utils.TextToSpeech.Interfaces;

public interface ITextToSpeechService
{
    /// <summary>
    /// Generate MP3 file from the provided text in Hebrew language.
    /// </summary>
    /// <param name="text">The text to be spoken.</param>
    Task<byte[]> GenerateAsync(string text) =>
        GenerateAsync(TextToSpeechFormats.Mp3, TextToSpeechLanguages.Hebrew, text);


    /// <summary>
    /// Generate MP3 file from the provided text in the specified language.
    /// </summary>
    /// <param name="language">The language in which the text should be spoken.</param>
    /// <param name="text">The text to be spoken.</param>
    Task<byte[]> GenerateAsync(TextToSpeechLanguages language, string text) =>
        GenerateAsync(TextToSpeechFormats.Mp3, language, text);


    /// <summary>
    /// Generate speech from the provided text in the specified format and language.
    /// </summary>
    /// <param name="format"> The file format of the audio file to be generated.</param>
    /// <param name="language">The language in which the text should be spoken.</param>
    /// <param name="text">The text to be spoken.</param>
    Task<byte[]> GenerateAsync(TextToSpeechFormats format, TextToSpeechLanguages language, string text);
}
