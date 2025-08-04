using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Utils.Social;


//https://api.telegram.org/bot<TOKEN>/getUpdates
public static class TelegramUtils
{
    static readonly HashSet<char> EscapableChars = [
        '_', '*', '[', ']', '(', ')', '~', '`', '>',
        '#', '+', '-', '=', '|', '{', '}', '.', '!'
    ];

    public static string EscapeMarkdownV2Text(string content)
    {
        StringBuilder result = new();
        foreach (var c in content)
        {
            if (EscapableChars.Contains(c))
                result.Append('\\');
            result.Append(c);
        }
        return result.ToString();
    }

    //public async Task<ApiResponse<GetUpdatesResponse[]>> GetUpdatesAsync(string key)
    //{
    //    return await client.GetAsync<ApiResponse<GetUpdatesResponse[]>>("getUpdates", new RestCallOptions
    //    {
    //        BaseUrl = $"https://api.telegram.org/bot{key}",
    //    });
    //}
}
