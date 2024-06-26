﻿static class Guard
{
    public static void AgainstNullOrEmpty(string value, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }
    public static void AgainstEmpty(CharSpan value, string argumentName)
    {
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }

    public static void AgainstEmpty(string? value, string argumentName)
    {
        if (value is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }
}