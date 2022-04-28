// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Text.RegularExpressions;
using Avalanche.Utilities;

/// <summary>Extension methods for <see cref="ITokenizer{T}"/>.</summary>
public static class Tokenizers
{
    /// <summary>Takes a non-essential token such as: white-space block, '//', '///', and '/**/' comments.</summary>
    static AnyTokenizer nonEssential = new AnyTokenizer(WhitespaceTokenizer.Any, RegexTokenizer.LineCommentLiteral, RegexTokenizer.BlockCommentLiteral, RegexTokenizer.DocumentCommentLiteral);
    /// <summary>Takes non-essential tokens, such as: white-space block, '//', '///', and '/**/' comments.</summary>
    static WhileTokenizer nonEssentials = new WhileTokenizer(nonEssential);

    /// <summary>Takes white-spaces, '//', '///', and '/**/' comments.</summary>
    public static AnyTokenizer NonEssential => nonEssential;
    /// <summary>Takes non-essential tokens, such as: white-space block, '//', '///', and '/**/' comments.</summary>
    public static WhileTokenizer NonEssentials => nonEssentials;

    /// <summary>Takes white-spaces.</summary>
    public static WhitespaceTokenizer<WhitespaceToken> Whitespace => WhitespaceTokenizer<WhitespaceToken>.Any;
    /// <summary>Takes integer values, positive and negative, e.g. "+10000". Does not use group sparator</summary>
    public static IntegerTokenizer<DecimalToken> Integer => IntegerTokenizer.Instance;
    /// <summary>Takes real values, positive and negative. Uses sign, group separator, decimal separator, fraction, exponent, exponent sign. e.g. "+1.23e-45"</summary>
    public static RealTokenizer<DecimalToken> Real => RealTokenizer.Instance;
    /// <summary>Takes hex value without '0x' prefix. Any case 'a-f' and 'A-F'. No prefix '0x'.</summary>
    public static HexTokenizer Hex => HexTokenizer.WithoutPrefix;
    /// <summary>Takes hex value without '0x' prefix. Any case 'a-f' and 'A-F'. Expects prefix '0x'.</summary>
    public static HexTokenizer HexWithPrefix => HexTokenizer.WithPrefix;
    /// <summary>Takes the next character as <see cref="MalformedToken"/>.</summary>
    public static MalformedTokenizer Malformed => MalformedTokenizer.Instance;

    /// <summary>Takes regex pattern.</summary>
    /// <remarks>The pattern should begin with "^".</remarks>
    public static RegexTokenizer Regex(Regex regex) => new RegexTokenizer(regex);
    /// <summary>Takes regex pattern.</summary>
    /// <remarks>The pattern should begin with "^".</remarks>
    public static RegexTokenizer Regex(string pattern) => new RegexTokenizer(new Regex(pattern, RegexOptions.Compiled|RegexOptions.CultureInvariant));

    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> Until<T>(ITokenizer<IToken> endCondition) where T : IToken, new() => new UntilTokenizer<T>(endCondition, false, null);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> Until<T>(string endCondition) where T : IToken, new() => new UntilTokenizer<T>(new ConstantTokenizer(endCondition), false, null);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> Until<T>(ITokenizer<IToken> endCondition, char escapeChar) where T : IToken, new() => new UntilTokenizer<T>(endCondition, false, escapeChar);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> Until<T>(string endCondition, char escapeChar) where T : IToken, new() => new UntilTokenizer<T>(new ConstantTokenizer(endCondition), false, escapeChar);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> UntilEndsWith<T>(ITokenizer<IToken> endCondition) where T : IToken, new() => new UntilTokenizer<T>(endCondition, true, null);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> UntilEndsWith<T>(string endCondition) where T : IToken, new() => new UntilTokenizer<T>(new ConstantTokenizer(endCondition), true, null);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> UntilEndsWith<T>(ITokenizer<IToken> endCondition, char escapeChar) where T : IToken, new() => new UntilTokenizer<T>(endCondition, true, escapeChar);
    /// <summary>Takes until <paramref name="endCondition"/> is found.</summary>
    public static UntilTokenizer<T> UntilEndsWith<T>(string endCondition, char escapeChar) where T : IToken, new() => new UntilTokenizer<T>(new ConstantTokenizer(endCondition), true, escapeChar);

    /// <summary>Take while <paramref name="elementTokenizer"/> yields tokens.</summary>
    public static WhileTokenizer While(ITokenizer<IToken> elementTokenizer) => new WhileTokenizer(elementTokenizer);

    /// <summary>Decorates <paramref name="tokenizer"/> so that it always returns true value.</summary>
    public static TrueTokenizer<T> True<T>(ITokenizer<T> tokenizer) where T : IToken, new() => new TrueTokenizer<T>(tokenizer);

    /// <summary>Takes <paramref name="text"/>.</summary>
    public static ConstantTokenizer Constant(string text) => new ConstantTokenizer(text);

    /// <summary>Tokenizes all remaining characters as token of <typeparamref name="T"/>.</summary>
    public static AllTokenizer<T> All<T>() where T : IToken, new() => AllTokenizer<T>.Instance;
    /// <summary>Unifies multiple tokenizers.</summary>
    public static AnyTokenizer Any(params ITokenizer<IToken>[] tokenizers) => new AnyTokenizer(tokenizers).SetReadOnly();
    /// <summary>A sequence of all of <paramref name="tokenizers"/>.</summary>
    public static SequenceTokenizer Sequence(params ITokenizer<IToken>[] tokenizers) => new SequenceTokenizer(tokenizers);
    /// <summary>A sequence of all of <paramref name="tokenizers"/>.</summary>
    public static SequenceTokenizer Sequence(params (ITokenizer<IToken> tokenizer, bool required)[] tokenizers) => new SequenceTokenizer(tokenizers);

    /// <summary>Parenthesis "(x)" tokenizer. <paramref name="contentTokenizer"/> should be as greedy as possible.</summary>
    public static SequenceTokenizer<ParenthesisToken> Parenthesis(ITokenizer<IToken> contentTokenizer)
        => new SequenceTokenizer<ParenthesisToken>(
            new ConstantTokenizer<TextToken>("("),
            contentTokenizer,
            new ConstantTokenizer<TextToken>(")"));

    /// <summary>
    /// Tokenizes "Key = Value". 
    /// Captures white-spaces that surround key and value parts into <see cref="WhitespaceToken"/>.
    /// Key is captured into a <see cref="KeyToken"/> and value into <see cref="ValueToken"/>.
    /// 
    /// Returns <see cref="CompositeToken"/> that has following child tokens: 
    ///     [<see cref="WhitespaceToken"/>], 
    ///     <see cref="KeyToken"/>, 
    ///     [<see cref="WhitespaceToken"/>], 
    ///     <see cref="OperandToken"/>, 
    ///     [<see cref="WhitespaceToken"/>], 
    ///     <see cref="ValueToken"/>, 
    ///     [<see cref="WhitespaceToken"/>]
    /// </summary>
    static SequenceTokenizer<KeyValueToken> keyValueTokenizer
        => new SequenceTokenizer<KeyValueToken>(
                (WhitespaceTokenizer.Any, false),
                (new UntilTokenizer<KeyToken>(new SequenceTokenizer((WhitespaceTokenizer.Any, false), (new ConstantTokenizer("="), true)), false, '\\'), true),
                (WhitespaceTokenizer.Any, false),
                (new ConstantTokenizer<OperandToken>("="), true),
                (WhitespaceTokenizer.Any, false),
                (new UntilTokenizer<ValueToken>(WhitespaceTokenizer.Any, true, '\\'), true),
                (WhitespaceTokenizer.Any, false)
            );

    /// <summary>
    /// Tokenizes "Key = Value". 
    /// Captures white-spaces that surround key and value parts into <see cref="WhitespaceToken"/>.
    /// Key is captured into a <see cref="KeyToken"/> and value into <see cref="ValueToken"/>.
    /// 
    /// Returns <see cref="CompositeToken"/> that has following child tokens: 
    ///     [<see cref="WhitespaceToken"/>], 
    ///     <see cref="KeyToken"/>, 
    ///     [<see cref="WhitespaceToken"/>], 
    ///     <see cref="OperandToken"/>, 
    ///     [<see cref="WhitespaceToken"/>], 
    ///     <see cref="ValueToken"/>, 
    ///     [<see cref="WhitespaceToken"/>]
    /// </summary>
    public static SequenceTokenizer<KeyValueToken> KeyValueTokenizer => keyValueTokenizer;

    /// <summary>Adapt <paramref name="delegate"/> into tokenizer.</summary>
    public static FuncTokenizer<T> Func<T>(Func<ReadOnlyMemory<char>, T> @delegate) where T : IToken => new FuncTokenizer<T>(@delegate);


}

