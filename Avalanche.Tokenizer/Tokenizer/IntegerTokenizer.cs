// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Globalization;

/// <summary>Integer tokenizer</summary>
public class IntegerTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Singleton</summary>
    static IntegerTokenizer<T> instance = new IntegerTokenizer<T>(CultureInfo.InvariantCulture.NumberFormat);
    /// <summary>Singleton</summary>
    public static IntegerTokenizer<T> Instance => instance;

    /// <summary></summary>
    protected NumberFormatInfo numberFormat;
    /// <summary></summary>
    public virtual NumberFormatInfo NumberFormat => numberFormat;

    /// <summary></summary>
    public IntegerTokenizer(NumberFormatInfo numberFormat)
    {
        this.numberFormat = numberFormat ?? throw new ArgumentNullException(nameof(numberFormat));
    }

    /// <summary>Try take token.</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Empty length
        if (text.IsEmpty) { token = default!; return false; }
        // Get span
        ReadOnlySpan<char> span = text.Span;
        // Accept '-'
        if (span.StartsWith(numberFormat.NegativeSign)) { span = span.Slice(numberFormat.NegativeSign.Length); }
        // Accept '+'
        else if (span.StartsWith(numberFormat.PositiveSign)) { span = span.Slice(numberFormat.PositiveSign.Length); }

        // Scan valid chars
        while (span.Length > 0)
        {
            // Get char
            char c = span[0];
            // Accept digit
            if (c >= '0' && c <= '9') span = span.Slice(1);
            //
            else break;
        }
        // Number of chars accepted
        int count = text.Length - span.Length;
        // No accepted chars
        if (count == 0) { token = default!; return false; }
        // Return
        token = text.As<T>(0, count);
        return true;
    }
}

/// <summary>Integer tokenizer</summary>
public class IntegerTokenizer : IntegerTokenizer<DecimalToken>
{
    /// <summary>Singleton</summary>
    static IntegerTokenizer instance = new IntegerTokenizer(CultureInfo.InvariantCulture.NumberFormat);
    /// <summary>Singleton</summary>
    public new static IntegerTokenizer Instance => instance;

    /// <summary></summary>
    public IntegerTokenizer(NumberFormatInfo numberFormat) : base(numberFormat) { }
}
