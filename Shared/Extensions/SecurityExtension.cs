using System;

namespace Shared.Extensions;

public static class SecurityExtension
{
    // Default size of random password is 15
    public static string CreateRandomPassword(int length = 15)
    {
        // Create a string of characters, numbers, special characters that allowed in the password
        string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
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

    public static string CreateRandomPasswordWithRandomLength()
    {
        // Create a string of characters, numbers, special characters that allowed in the password
        string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new();

        // Minimum size 8. Max size is number of all allowed chars.
        int size = random.Next(8, validChars.Length);

        // Select one random character at a time from the string
        // and create an array of chars
        char[] chars = new char[size];
        for (int i = 0; i < size; i++)
        {
            chars[i] = validChars[random.Next(0, validChars.Length)];
        }
        return new string(chars);
    }

    // Default size of random verification code is 6
    public static string CreateRandomVerificationCode(int length = 6)
    {
        // Create a string of numbers that allowed in the verification code
        string validChars = "0123456789";
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
