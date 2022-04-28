// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>Extension methods for <see cref="ITokenizer{T}"/>.</summary>
public static class TokenizerExtensions
{
    /// <summary>Try take a <typeparamref name="T"/> token from <paramref name="text"/>.</summary>
    public static bool TryTake<T>(this ITokenizer<T> tokenizer, string text, out T token) where T : IToken => tokenizer.TryTake(text.AsMemory(), out token);
    /// <summary>Take token</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerHidden]
    public static T? Take<T>(this ITokenizer<T> tokenizer, ReadOnlyMemory<char> memory) where T : IToken => tokenizer.TryTake(memory, out T token) ? token : default!;
    /// <summary>Take token</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerHidden]
    public static T? Take<T>(this ITokenizer<T> tokenizer, string text) where T : IToken => tokenizer.TryTake(text.AsMemory(), out T token) ? token : default!;

    /// <summary>Take <paramref name="memory"/> completely</summary>
    /// <exception cref="InvalidOperationException">If <paramref name="tokenizer"/> did not tokenize <paramref name="memory"/> completely</exception>
    public static T TakeAll<T>(this ITokenizer<T> tokenizer, ReadOnlyMemory<char> memory) where T : IToken
    {
        // Take taken
        if (tokenizer.TryTake(memory, out T token))
        {
            // Not complete
            if (token.Memory.Length != memory.Length) throw new InvalidOperationException($"{tokenizer} did not consume text completely.");
            //
            return token;
        }
        // Did not take token
        throw new InvalidOperationException($"{tokenizer} did not consume text");
    }

    /// <summary>Take token</summary>
    public static T TakeAll<T>(this ITokenizer<T> tokenizer, string text) where T : IToken
    {
        // Take taken
        if (tokenizer.TryTake(text.AsMemory(), out T token))
        {
            // Not complete
            if (token.Memory.Length != text.Length) throw new InvalidOperationException($"{tokenizer} did not consume text completely.");
            //
            return token;
        }
        // Did not take token
        throw new InvalidOperationException($"{tokenizer} did not consume text");
    }
}
