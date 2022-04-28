// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;

/// <summary>Tokenizes all remaining characters as token of <typeparamref name="T"/>.</summary>
public class AllTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Singleton</summary>
    static AllTokenizer<T> instance = new AllTokenizer<T>();
    /// <summary>Singleton</summary>
    public static AllTokenizer<T> Instance => instance;

    /// <summary>Tokenize all remaining characters, if any are left</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // No text left
        if (text.Length == 0) { token = default!; return false; }

        // Return token
        token = new T { Memory = text };
        return true;
    }
}
