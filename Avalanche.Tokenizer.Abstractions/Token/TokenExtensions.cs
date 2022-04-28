// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Runtime.InteropServices;

/// <summary>Extension methods for <see cref="IToken"/>.</summary>
public static class TokenExtensions
{
    /// <summary>Create token <typeparamref name="T"/> that uses <paramref name="memory"/>.</summary>
    public static T As<T>(this ReadOnlyMemory<char> memory) where T : IToken, new() => new T { Memory = memory };
    /// <summary>Create token <typeparamref name="T"/> that uses <paramref name="memory"/>.</summary>
    public static T As<T>(this ReadOnlyMemory<char> memory, int start) where T : IToken, new() => new T { Memory = memory[start..] };
    /// <summary>Create token <typeparamref name="T"/> that uses <paramref name="memory"/>.</summary>
    public static T As<T>(this ReadOnlyMemory<char> memory, int start, int length) where T : IToken, new() => new T { Memory = memory.Slice(start, length) };

    /// <summary>Print refered memory span as string</summary>
    public static string Text<T>(this T token) where T : IToken
    {
        // Get memory
        ReadOnlyMemory<char> memory = token.Memory;
        // Get string
        if (MemoryMarshal.TryGetString(memory, out string? text, out int start, out int length) && text != null) if (start == 0 && length == text.Length) return text;
        // Create string
        text = new string(memory.Span);
        return text;

    }

}
