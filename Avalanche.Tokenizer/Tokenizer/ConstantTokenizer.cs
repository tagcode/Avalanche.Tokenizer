// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Tokenizer that takes a constant string.</summary>
public class ConstantTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Text to take</summary>
    protected string text;
    /// <summary>Char count</summary>
    protected int length;

    /// <summary>Text to take</summary>
    public virtual string Text => text;

    /// <summary></summary>
    public ConstantTokenizer(string text)
    {
        // Assign
        this.text = text ?? throw new ArgumentNullException(nameof(text));
        this.length = text.Length;
    }

    /// <summary>Try take constant string</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Not enough chars
        if (text.Length < length) { token = default!; return false; }
        // Slice to equal length
        ReadOnlySpan<char> textSpan = text.Span.Slice(0, length), span2 = this.text.AsSpan();
        // Compare in this method
        if (length < 1024)
        {
            for (int i=0; i<length; i++) 
                if (span2[i] != textSpan[i])
                    { token = default!; return false; }
        }
        else
        // Use AVX etc for comparison
        {
            // Not same
            if (!System.MemoryExtensions.SequenceEqual(span2, textSpan)) { token = default!; return false; }
        }
        // Assign token
        token = text.Slice(0, length).As<T>();
        return true;
    }
}

/// <summary>Tokenizer that takes a constant string.</summary>
public class ConstantTokenizer : ConstantTokenizer<TextToken>
{
    /// <summary></summary>
    public ConstantTokenizer(string text) : base(text)
    {
    }
}
