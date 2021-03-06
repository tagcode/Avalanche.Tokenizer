// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;
using Avalanche.Utilities;

/// <summary>Indicates that token represents a operand, e.g. "="</summary>
public struct OperandToken : IToken
{
    /// <summary>Create token</summary>
    public OperandToken() { }
    /// <summary>Text source</summary>
    public ReadOnlyMemory<char> Memory { get; set; } = default;
    /// <summary>Children of structural token. Each child must be contained in the range of this parent token.</summary>
    public IToken[] Children { get; set; } = Array.Empty<IToken>();
    /// <summary>Accept visitor.</summary>
    public bool Accept(ITokenVisitor visitor) { if (visitor is ITokenVisitor<OperandToken> c) { c.Visit(this); return true; } else return false; }
    /// <summary>Print token as string</summary>
    public override string ToString() { int index = Memory.Index(); return $"[{index}:{index + Memory.Length}] {GetType().Name} \"{Memory}\""; }

}

