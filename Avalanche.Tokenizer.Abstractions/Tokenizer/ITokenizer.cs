// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

// <docs>
/// <summary>Indicates implements tokenizer</summary>
public interface ITokenizerBase { } 
/// <summary>Dismantles tokens from char memory.</summary>
public interface ITokenizer<T> : ITokenizerBase where T : IToken
{
    /// <summary>Peek to test whether <paramref name="text"/> starts with <typeparamref name="T"/>.</summary>
    bool Peek(ReadOnlyMemory<char> text);
    /// <summary>Try take a <typeparamref name="T"/> token from <paramref name="text"/>.</summary>
    bool TryTake(ReadOnlyMemory<char> text, out T token);
}
// </docs>

