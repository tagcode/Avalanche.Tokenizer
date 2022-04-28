// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Takes whitespace characters. Uses <see cref="char.IsWhiteSpace(char)"/> for evaluating which characters are white space.</summary>
public class WhitespaceTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Singleton</summary>
    static WhitespaceTokenizer<T> any = new WhitespaceTokenizer<T>(true);
    /// <summary>Singleton</summary>
    static WhitespaceTokenizer<T> allButNewLine = new WhitespaceTokenizer<T>(false);
    /// <summary>Singleton</summary>
    public static WhitespaceTokenizer<T> Any => any;
    /// <summary>Singleton</summary>
    public static WhitespaceTokenizer<T> AllButNewLine => allButNewLine;

    /// <summary></summary>
    protected bool includeNewLine;
    /// <summary></summary>
    public virtual bool IncludeNewLine => includeNewLine;

    /// <summary></summary>
    public WhitespaceTokenizer(bool includeNewLine)
    {
        this.includeNewLine = includeNewLine;
    }

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Get span
        ReadOnlySpan<char> span = text.Span;
        // Accepted characters
        int ix = 0;
        // Get chars
        for (int i = 0; i < span.Length; i++)
        {
            // Get char
            char ch = span[i];
            // New line
            if (ch == '\n' && !includeNewLine) break;
            // Not white-space
            if (!char.IsWhiteSpace(ch)) break;
            // Accept
            ix++;
        }
        // No white-spaces
        if (ix == 0) { token = default!; return false; }
        // Slice
        token = new T { Memory = text.Slice(0, ix) };
        return true;
    }
}

/// <summary>Takes whitespace characters. Uses <see cref="char.IsWhiteSpace(char)"/> for evaluating which characters are white space.</summary>
public class WhitespaceTokenizer : WhitespaceTokenizer<WhitespaceToken>
{
    /// <summary>Singleton</summary>
    static WhitespaceTokenizer any = new WhitespaceTokenizer(true);
    /// <summary>Singleton</summary>
    static WhitespaceTokenizer allButNewLine = new WhitespaceTokenizer(false);
    /// <summary>Singleton</summary>
    public new static WhitespaceTokenizer Any => any;
    /// <summary>Singleton</summary>
    public new static WhitespaceTokenizer AllButNewLine => allButNewLine;

    /// <summary></summary>
    public WhitespaceTokenizer(bool includeNewLine) : base(includeNewLine) { }
}

