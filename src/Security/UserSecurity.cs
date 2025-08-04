using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoeiBajayo.Infrastructure.Security;

public class UserSecurity
{
    public readonly static UserSecurity Default = new();

    protected const string DEFAULT_CHARS = "abcdefghjklmnpqrstwxyzABCDEFGHJKLMNPQRSTWXYZ";
    protected const string DEFAULT_NUMBERS = "0123456789";
    protected const string DEFAULT_SPECIAL = "!@#$%&_+-*.~:;|";

    public HashSet<char> Chars { get; set; } = [.. DEFAULT_CHARS.ToCharArray()];
    public HashSet<char> UsernameChars { get; set; } = [.. (DEFAULT_CHARS + ".").ToCharArray()];
    public HashSet<char> Numbers { get; set; } = [.. DEFAULT_NUMBERS.ToCharArray()];
    public HashSet<char> SpecialLetters { get; set; } = [.. DEFAULT_SPECIAL.ToCharArray()];

    public string GeneratePassword(int length = 8, int minimumNumbers = 3, int minimumSpecial = 1)
    {
        var arrChars = Chars.ToArray();

        var sb = new StringBuilder(length);
        var random = new Random();

        for (int i = 0; i < length; i++)
        {
            sb.Append(random.PickRandom(arrChars));
        }

        if (minimumNumbers > 0 || minimumSpecial > 0)
        {
            var served = new List<int>(minimumNumbers + minimumSpecial);

            if (minimumNumbers > 0)
            {
                var arrNumbers = Numbers.ToArray();
                for (int i = 0; i < minimumNumbers; i++)
                {
                    int index;
                    do
                    {
                        index = random.Next(0, length - 1);
                        if (served.Contains(index))
                        {
                            index = -1;
                        }
                    }
                    while (index == -1);
                    served.Add(index);
                    sb[index] = random.PickRandom(arrNumbers);
                }
            }

            if (minimumSpecial > 0)
            {
                var arrSpecial = SpecialLetters.ToArray();
                for (int i = 0; i < minimumSpecial; i++)
                {
                    int index;
                    do
                    {
                        index = random.Next(0, length - 1);
                        if (served.Contains(index))
                        {
                            index = -1;
                        }
                    }
                    while (index == -1);
                    served.Add(index);
                    sb[index] = random.PickRandom(arrSpecial);
                }
            }
        }

        return sb.ToString();
    }

    public enum ValidateResult
    {
        Valid,
        Empty, MinimumLength, MinimumChars, MinimumSpecial, MinimumNumbers, IncrementNumbers, DuplicateChars
    }
    public ValidateResult ValidateString(string? input, int minimumLength = 0,
        int minimumChars = 0, int minimumNumbers = 0, int minimumSpecial = 0,
        bool duplicatedChars = false, bool incrementNumbers = false)
    {
        input = input?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return ValidateResult.Empty;
        }

        if (minimumLength > 0 && input.Length < minimumLength)
        {
            return ValidateResult.MinimumLength;
        }

        if (!ValidateChars(Chars, input, minimumChars))
        {
            return ValidateResult.MinimumChars;
        }

        if (!ValidateChars(Numbers, input, minimumNumbers))
        {
            return ValidateResult.MinimumNumbers;
        }

        if (!ValidateChars(SpecialLetters, input, minimumSpecial))
        {
            return ValidateResult.MinimumSpecial;
        }

        if (duplicatedChars && ContainsDuplicatedChars(input))
        {
            return ValidateResult.DuplicateChars;
        }

        if (incrementNumbers && ContainsIncrementNumbers(input))
        {
            return ValidateResult.IncrementNumbers;
        }

        return ValidateResult.Valid;
    }
    public static bool ContainsDuplicatedChars(string input)
    {
        char lastChar = '-';
        for (var i = 0; i < input.Length; i++)
        {
            if (i > 0)
            {
                if (input[i] == lastChar)
                    return true;
            }
            lastChar = input[i];
        }
        return false;
    }
    public static bool ContainsIncrementNumbers(string input)
    {
        int lastNumber = -2;
        for (var i = 0; i < input.Length; i++)
        {
            if (!int.TryParse(input[i].ToString(), out int number))
            {
                number = -2;
            }
            if (i > 0 && lastNumber > -1)
            {
                if ((lastNumber + 1) == number || (lastNumber - 1) == number)
                    return true;
            }
            lastNumber = number;
        }
        return false;
    }
    private static bool ValidateChars(HashSet<char> chars, string input, int min)
    {
        if (min <= 0)
            return true;

        int counter = 0;
        for (var i = 0; i < input.Length; i++)
        {
            if (chars.Contains(input[i]))
            {
                counter++;
                if (counter == min)
                    return true;
            }
        }

        return false;
    }
    public static bool IsValidId(string? personOrCompanyId)
    {
        personOrCompanyId = personOrCompanyId?.Trim();

        if (string.IsNullOrEmpty(personOrCompanyId) || personOrCompanyId.Length > 9 || personOrCompanyId.Length < 4)
        {
            return false;
        }

        for (var i = 0; i < personOrCompanyId.Length; i++)
        {
            if (!char.IsDigit(personOrCompanyId[i]))
            {
                return false;
            }
        }

        if (personOrCompanyId.Length < 9)
        {
            personOrCompanyId = personOrCompanyId.PadLeft(9, '0');
        }

        var sum = 0;
        int incNum;
        for (var i = 0; i < personOrCompanyId.Length; i++)
        {
            incNum = (personOrCompanyId[i] - '0') * ((i % 2) + 1);  // Multiply number by 1 or 2
            sum += (incNum > 9) ? incNum - 9 : incNum;  // Sum the digits up and add to total
        }
        return sum % 10 == 0;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "<Pending>")]
    public static bool IsValidPhone(string? phone)
    {
        phone = phone?.Replace("-", "").Trim();

        if (string.IsNullOrEmpty(phone) || phone.Length < 9 || phone.Length > 10)
        {
            return false;
        }

        return Regex.IsMatch(phone, "^(1(599|700|80|9)|0[2345789][123456789]{6,})");
    }
}
