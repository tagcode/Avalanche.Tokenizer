// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;
using Avalanche.Utilities;

/// <summary>Text token represents value.</summary>
public struct ValueToken : IToken
{
    /// <summary>Create token</summary>
    public ValueToken() { }
    /// <summary>Text source</summary>
    public ReadOnlyMemory<char> Memory { get; set; } = default;
    /// <summary>Children of structural token. Each child must be contained in the range of this parent token.</summary>
    public IToken[] Children { get; set; } = Array.Empty<IToken>();
    /// <summary>Accept visitor.</summary>
    public bool Accept(ITokenVisitor visitor) { if (visitor is ITokenVisitor<ValueToken> c) { c.Visit(this); return true; } else return false; }
    /// <summary>Print token as string</summary>
    public override string ToString() { int index = Memory.Index(); return $"[{index}:{index + Memory.Length}] {GetType().Name} \"{Memory}\""; }
}
