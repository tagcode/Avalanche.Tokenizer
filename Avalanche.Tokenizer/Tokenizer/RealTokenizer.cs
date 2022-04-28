// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Globalization;

/// <summary>Real tokenizer</summary>
public class RealTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Singleton</summary>
    static RealTokenizer<T> withGroupSeparator = new RealTokenizer<T>(CultureInfo.InvariantCulture.NumberFormat, true);
    /// <summary>Singleton</summary>
    static RealTokenizer<T> withoutGroupSeparator = new RealTokenizer<T>(CultureInfo.InvariantCulture.NumberFormat, false);
    /// <summary>Singleton</summary>
    public static RealTokenizer<T> Instance => withGroupSeparator;
    /// <summary>Singleton</summary>
    public static RealTokenizer<T> WithoutGroupSeparator => withoutGroupSeparator;

    /// <summary></summary>
    protected NumberFormatInfo numberFormat;
    /// <summary></summary>
    protected bool useGroupSeparator;
    /// <summary></summary>
    public virtual NumberFormatInfo NumberFormat => numberFormat;
    /// <summary></summary>
    public virtual bool UseGroupSeparator => useGroupSeparator;

    /// <summary></summary>
    public RealTokenizer(NumberFormatInfo numberFormat, bool useGroupSeparator = true)
    {
        this.numberFormat = numberFormat ?? throw new ArgumentNullException(nameof(numberFormat));
        this.useGroupSeparator = useGroupSeparator;
    }

    /// <summary>Try take token.</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Empty length
        if (text.IsEmpty) { token = default!; return false; }
        // Get span
        ReadOnlySpan<char> span = text.Span;

        // 0    = Sign
        // 1    = NaN/Infinity
        // 2    = digits with group separator 1,000,000,00
        // 3    = fractions
        // 4..5 = exponent
        // 6    = End
        int phase = 0;

        // Scan valid chars
        while(span.Length>0)
        {
            // Sign
            if (phase == 0)
            {
                // Accept '-'
                if (span.StartsWith(numberFormat.NegativeSign)) { span = span.Slice(numberFormat.NegativeSign.Length); phase = 1; continue; }
                // Accept '+'
                else if (span.StartsWith(numberFormat.PositiveSign)) { span = span.Slice(numberFormat.PositiveSign.Length); phase = 1; continue; }
            }
            // Sign
            if (phase <= 1)
            {
                // NaN
                if (span.StartsWith(numberFormat.NaNSymbol)) { span = span.Slice(numberFormat.NaNSymbol.Length); phase = 6; break; }
                // +Infinity
                else if (span.StartsWith(numberFormat.PositiveInfinitySymbol)) { span = span.Slice(numberFormat.PositiveInfinitySymbol.Length); phase = 6; break; }
                // -Infinity
                else if (span.StartsWith(numberFormat.NegativeInfinitySymbol)) { span = span.Slice(numberFormat.NegativeInfinitySymbol.Length); phase = 6; break; }
            }

            // 
            bool gotDigit = false;
            // Accept digit
            foreach (string digit in numberFormat.NativeDigits)
                if (span.StartsWith(digit)) { span = span.Slice(digit.Length); gotDigit = true; break; }
            if (gotDigit) { if (phase <= 2) phase = 2; continue; }

            // Group separator ','
            if (useGroupSeparator && phase == 2 && span.StartsWith(numberFormat.NumberGroupSeparator)) { span = span.Slice(numberFormat.NumberGroupSeparator.Length); continue; }
            // Decimal separator '.'
            if (phase <= 2 && span.StartsWith(numberFormat.NumberDecimalSeparator)) { span = span.Slice(numberFormat.NumberDecimalSeparator.Length); phase = 3; continue; }
            // Get char
            char ch = span[0];
            // Exponent 'e' or 'c'
            if (phase <= 3 &&  (ch == 'e' || ch == 'E' || ch == 'c' || ch == 'C')) { span = span.Slice(1); phase = 4; continue; }
            // Exponent sign '-'
            if (phase == 4)
            {
                // Accept '-'
                if (span.StartsWith(numberFormat.NegativeSign)) { span = span.Slice(numberFormat.NegativeSign.Length); phase = 5; continue; }
                // Accept '+'
                else if (span.StartsWith(numberFormat.PositiveSign)) { span = span.Slice(numberFormat.PositiveSign.Length); phase = 5; continue; }
            }
            //
            break;
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

/// <summary>Real tokenizer</summary>
public class RealTokenizer : RealTokenizer<DecimalToken>
{
    /// <summary>Singleton</summary>
    static RealTokenizer withGroupSeparator = new RealTokenizer(CultureInfo.InvariantCulture.NumberFormat, true);
    /// <summary>Singleton</summary>
    static RealTokenizer withoutGroupSeparator = new RealTokenizer(CultureInfo.InvariantCulture.NumberFormat, false);
    /// <summary>Singleton</summary>
    public new static RealTokenizer Instance => withGroupSeparator;
    /// <summary>Singleton</summary>
    public new static RealTokenizer WithoutGroupSeparator => withoutGroupSeparator;

    /// <summary></summary>
    public RealTokenizer(NumberFormatInfo numberFormat, bool useGroupSeparator = true) : base(numberFormat, useGroupSeparator) { }
}
