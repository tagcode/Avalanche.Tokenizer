// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Takes new line character '\n'.</summary>
public class NewLineTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Singleton</summary>
    static NewLineTokenizer<T> instance = new NewLineTokenizer<T>();
    /// <summary>Singleton</summary>
    public static NewLineTokenizer<T> Instance => instance;

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Get span
        ReadOnlySpan<char> span = text.Span;
        // Got '\n'
        if (span.Length>0 && span[0] == '\n') { token = new T { Memory = text.Slice(0, 1) }; return true; }
        // No new line
        token = default!; 
        return false;
    }
}

/// <summary>Takes whitespace characters. Uses <see cref="char.IsWhiteSpace(char)"/> for evaluating which characters are white space.</summary>
public class NewLineTokenizer : NewLineTokenizer<NewLineToken>
{
    /// <summary>Singleton</summary>
    static NewLineTokenizer instance = new NewLineTokenizer();
    /// <summary>Singleton</summary>
    public new static NewLineTokenizer Instance => instance;
}

