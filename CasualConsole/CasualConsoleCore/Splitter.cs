using System;
using System.Collections.Generic;

namespace CasualConsoleCore;

readonly ref struct Splitter
{
    private readonly ReadOnlySpan<char> text;
    private readonly List<(int, int)> parts;

    public readonly int Length => parts.Count;

    public readonly ReadOnlySpan<char> this[int index]
    {
        get
        {
            var (start, end) = parts[index];
            return text[start..end];
        }
    }

    private Splitter(ReadOnlySpan<char> text, List<(int, int)> parts)
    {
        this.text = text;
        this.parts = parts;
    }

    public static Splitter Split(ReadOnlySpan<char> text, char c)
    {
        var parts = new List<(int, int)>();
        int i = 0;
        while (i < text.Length)
        {
            var index = IndexOf(text, c, i);
            if (index < 0)
                break;
            parts.Add((i, index));
            i = index + 1;
        }
        if (i < text.Length)
            parts.Add((i, text.Length));
        return new Splitter(text, parts);
    }

    public static Splitter Split(ReadOnlySpan<char> text, ReadOnlySpan<char> c)
    {
        var parts = new List<(int, int)>();
        int i = 0;
        while (i < text.Length)
        {
            var index = IndexOf(text, c, i);
            if (index < 0)
                break;
            parts.Add((i, index));
            i = index + 1;
        }
        if (i < text.Length)
            parts.Add((i, text.Length));
        return new Splitter(text, parts);
    }

    private static int IndexOf(ReadOnlySpan<char> s, char c, int i)
    {
        var index = s[i..].IndexOf(c);
        return index < 0 ? -1 : index + i;
    }

    private static int IndexOf(ReadOnlySpan<char> s, ReadOnlySpan<char> c, int i)
    {
        var index = s[i..].IndexOf(c);
        return index < 0 ? -1 : index + i;
    }
}