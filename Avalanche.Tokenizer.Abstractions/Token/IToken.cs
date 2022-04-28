// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

// <docs>
/// <summary>Token features</summary>
public interface IToken
{
    /// <summary>Memory source</summary>
    ReadOnlyMemory<char> Memory { get; set; }
    /// <summary>Children of structural token. Each child must be contained in the range of this parent token.</summary>
    IToken[] Children { get; set; }
    /// <summary>Accept visitor.</summary>
    bool Accept(ITokenVisitor visitor);
}

/// <summary>Token visitor base interface.</summary>
public interface ITokenVisitor { }
/// <summary>Token visitor for <typeparamref name="T"/>.</summary>
public interface ITokenVisitor<T> : ITokenVisitor where T : IToken
{
    /// <summary>Visit token</summary>
    void Visit(T token);
}
// </docs>
