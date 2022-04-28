// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;

/// <summary>Tokenizes specific chars.</summary>
public class CharTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>_0-9 and letter tokenizer</summary>
    static CharTokenizer<T> alphaNumeric = new CharTokenizer<T>(mem => mem.Span[0] == '_' || char.IsLetterOrDigit(mem.Span[0]));
    /// <summary>0-9 tokenizer</summary>
    static CharTokenizer<T> numeric = new CharTokenizer<T>(mem => mem.Span[0] >= '0' && mem.Span[0] <= '9');
    /// <summary>All but '{' and '}'</summary>
    static CharTokenizer<T> nonBrace = new CharTokenizer<T>(mem => mem.Span[0] != '{' && mem.Span[0] != '}');
    /// <summary>All chars tokenizer, takes all chars</summary>
    static CharTokenizer<T> all = new CharTokenizer<T>(mem => true);
    /// <summary>_0-9 and letter tokenizer</summary>
    public static CharTokenizer<T> AlphaNumeric => alphaNumeric;
    /// <summary>0-9 tokenizer</summary>
    public static CharTokenizer<T> Numeric => numeric;
    /// <summary>All but '{' and '}'</summary>
    public static CharTokenizer<T> NonBrace => nonBrace;
    /// <summary>All chars tokenizer, takes all chars</summary>
    public static CharTokenizer<T> All => all;

    /// <summary>Character validator</summary>
    protected Func<ReadOnlyMemory<char>, bool> validator;
    /// <summary>Character validator</summary>
    public Func<ReadOnlyMemory<char>, bool> Validator => validator;

    /// <summary></summary>
    public CharTokenizer(Func<ReadOnlyMemory<char>, bool> charValidator)
    {
        this.validator = charValidator ?? throw new ArgumentNullException(nameof(charValidator));
    }

    /// <summary>Try take format string.</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Empty length
        if (text.Length == 0) { token = default!; return false; }
        // Number of chars accepted as token
        int ix = 0;
        // Scan valid chars
        for (int i = 0; i < text.Length; i++)
        {
            // Accept char
            if (validator(text.Slice(i))) ix++;
            //
            else break;
        }
        // No accepted chars
        if (ix == 0) { token = default!; return false; }
        // Return
        token = text.As<T>(0, ix);
        return true;
    }
}

/// <summary>Tokenizes specific chars.</summary>
public class CharTokenizer : CharTokenizer<TextToken>
{
    /// <summary></summary>
    public CharTokenizer(Func<ReadOnlyMemory<char>, bool> charValidator) : base(charValidator) { }
}
