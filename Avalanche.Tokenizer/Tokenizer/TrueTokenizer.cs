// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Decorates tokenizer so that it always returns true value.</summary>
public class TrueTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Element tokenizer</summary>
    protected ITokenizer<T> tokenizer;
    /// <summary>Element tokenizer</summary>
    public virtual ITokenizer<T> Tokenizer => tokenizer;

    /// <summary></summary>
    /// <param name="tokenizer"></param>
    public TrueTokenizer(ITokenizer<T> tokenizer)
    {
        this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
    }

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Take with element tokenizer
        if (tokenizer.TryTake(text, out token)) return true;
        // Get 0-length slice
        token = new T() { Memory = text.Slice(0, 0) };
        // Done
        return true;
    }
}

