using System;

namespace LethalLevelLoader.General;
internal static class SpanExtensions
{
    public static bool IsLettersOrDigits(this ReadOnlySpan<char> span)
    {
        for (var i = 0; i < span.Length; i++)
        {
            if (char.IsLetterOrDigit(span[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsLetters(this ReadOnlySpan<char> span)
    {
        for (var i = 0; i < span.Length; i++)
        {
            if (char.IsLetter(span[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static ReadOnlySpan<char> TrimStartToLetters(this ReadOnlySpan<char> span)
    {
        var startIndex = 0;
        for (var i = 0; i < span.Length; i++)
        {
            if (char.IsLetter(span[i]))
            {
                break;
            }

            startIndex++;
        }

        return span.Slice(startIndex);
    }
}
