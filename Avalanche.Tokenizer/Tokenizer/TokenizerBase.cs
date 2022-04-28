// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>Tokenizer base class</summary>
public abstract class TokenizerBase_<T> : ITokenizer<T> where T : IToken
{
    /// <summary>Peek to test whether <paramref name="text"/> starts with <typeparamref name="T"/>.</summary>
    public virtual bool Peek(ReadOnlyMemory<char> text) => TryTake(text, out T token);
    /// <summary>Try take format string.</summary>
    public abstract bool TryTake(ReadOnlyMemory<char> text, out T token);
}

/// <summary>Tokenizer base class</summary>
public abstract class TokenizerBase<T> : TokenizerBase_<T>, ITokenizer<IToken> where T : IToken
{
    /// <summary>Peek to test whether <paramref name="text"/> starts with <typeparamref name="T"/>.</summary>
    public new virtual bool Peek(ReadOnlyMemory<char> text) => TryTake(text, out T token);

    /// <summary>Try take format string.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerHidden]
    public bool TryTake(ReadOnlyMemory<char> text, out IToken token)
    {
        // Try take
        if (!this.TryTake(text, out T _token)) { token = default!; return false; }
        // Ok
        token = _token;
        return true;
    }
}
