// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using Avalanche.Utilities;

/// <summary>Indicates malformed text span.</summary>
public struct MalformedToken : IToken
{
    /// <summary>No tokens singleton</summary>
    public static MalformedToken[] NO_TOKENS => Array.Empty<MalformedToken>();
    /// <summary>Create token</summary>
    public MalformedToken() { }
    /// <summary>Text source</summary>
    public ReadOnlyMemory<char> Memory { get; set; } = default;
    /// <summary>Children of structural token. Each child must be contained in the range of this parent token.</summary>
    public IToken[] Children { get; set; } = Array.Empty<IToken>();
    /// <summary>Accept visitor.</summary>
    public bool Accept(ITokenVisitor visitor) { if (visitor is ITokenVisitor<MalformedToken> c) { c.Visit(this); return true; } else return false; }
    /// <summary>Print token as string</summary>
    public override string ToString() { int index = Memory.Index(); return $"[{index}:{index + Memory.Length}] {GetType().Name} \"{Memory}\""; }

}

