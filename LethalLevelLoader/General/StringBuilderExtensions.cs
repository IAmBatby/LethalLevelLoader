using System;
using System.Text;

namespace LethalLevelLoader.General;
internal static class StringBuilderExtensions
{
    public static StringBuilder TrimEnd(this StringBuilder sb, char trimChar)
    {
        if (sb.Length == 0)
        {
            return sb;
        }

        var startIndex = sb.Length - 1;
        while (startIndex >= 0 && sb[startIndex] == trimChar)
        {
            startIndex--;
        }

        sb.Length = startIndex + 1;
        return sb;
    }

    public static StringBuilder AppendValue(this StringBuilder sb, int value)
    {
        // StringBuilder.Append(T) allocates string under the hood in Mono, using Span<char> to prevent that
        Span<char> buffer = stackalloc char[32];
        value.TryFormat(buffer, out var charsWritten);

        sb.Append(buffer.Slice(0, charsWritten));
        return sb;
    }

    public static StringBuilder AppendValue(this StringBuilder sb, float value)
    {
        Span<char> buffer = stackalloc char[32];
        value.TryFormat(buffer, out var charsWritten);

        sb.Append(buffer.Slice(0, charsWritten));
        return sb;
    }
}
