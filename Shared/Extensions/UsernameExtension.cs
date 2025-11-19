using System;
using System.Text.RegularExpressions;

namespace Shared.Extensions;

public static partial class UsernameExtension
{
    public static string GenerateFirstNameString(this string firstName)
    {
        if (firstName != null && firstName.Length >= 3)
            return firstName[..3].ToLower(); //firstName.Substring(0, 3);
        else if (firstName != null && firstName.Length < 3)
            return firstName.PadRight(3, 'x').ToLower();
        else
            return CreateRandomUsername();
    }

    public static string GenerateLastNameString(this string lastName)
    {
        if (lastName != null && lastName.RemoveVowels().Length >= 3)
        {
            var vowellessLastName = lastName.RemoveVowels();
            var generatedLastName = vowellessLastName[0].ToString() +
                vowellessLastName[1].ToString() +
                vowellessLastName[^1].ToString(); //vowellessLastName[vowellessLastName.Length - 1];

            return generatedLastName.ToLower();
        }
        else if (lastName != null && lastName.RemoveVowels().Length < 3)
            return lastName.RemoveVowels().PadRight(3, 'x').ToLower();
        else
            return CreateRandomUsername();
    }

    public static string RemoveVowels(this string text)
    {
        return VowellessRegex().Replace(text, "");
    }

    [GeneratedRegex("[aeiouAEIOU]")]
    private static partial Regex VowellessRegex();

    private static string CreateRandomUsername(int length = 3)
    {
        // Create a string of characters, numbers, special characters that allowed in the password
        string validChars = "abcdefghjklmnopqrstuvwxyz";
        Random random = new();

        // Select one random character at a time from the string
        // and create an array of chars
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = validChars[random.Next(0, validChars.Length)];
        }
        return new string(chars);
    }
}
