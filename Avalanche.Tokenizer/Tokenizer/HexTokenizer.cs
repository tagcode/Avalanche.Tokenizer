// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Hex tokenizer</summary>
public class HexTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. No prefix '0x'.</summary>
    static HexTokenizer<T> withoutPrefix = new HexTokenizer<T>(false);
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. Expects prefix '0x'.</summary>
    static HexTokenizer<T> withPrefix = new HexTokenizer<T>(true);
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. No prefix '0x'.</summary>
    public static HexTokenizer<T> WithoutPrefix => withoutPrefix;
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. Expects prefix '0x'.</summary>
    public static HexTokenizer<T> WithPrefix => withPrefix;

    /// <summary>Take 0x prefix</summary>
    protected bool take0x;
    /// <summary>Take 0x prefix</summary>
    public bool Take0x => take0x;

    /// <summary></summary>
    public HexTokenizer(bool take0x)
    {
        this.take0x = take0x;
    }

    /// <summary>Try take token.</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Empty length
        if (text.IsEmpty) { token = default!; return false; }
        // Get span
        ReadOnlySpan<char> span = text.Span;
        // Accept '0x'
        if (take0x)
        {
            // Nope
            if (span.Length < 2 || span[0] != '0' || span[1] != 'x') { token = default!; return false; }
            // Slice
            span = span.Slice(2);
        }
        // Scan valid chars
        while (span.Length>0)
        {
            // Get char
            char c = span[0];
            // Accept char
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')) span = span.Slice(1);
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
public class HexTokenizer : HexTokenizer<DecimalToken>
{
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. No prefix '0x'.</summary>
    static HexTokenizer withoutPrefix = new HexTokenizer(false);
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. Expects prefix '0x'.</summary>
    static HexTokenizer withPrefix = new HexTokenizer(true);
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. No prefix '0x'.</summary>
    public static new HexTokenizer WithoutPrefix => withoutPrefix;
    /// <summary>Hex decimal tokenizer. Any case 'a-f' and 'A-F'. Expects prefix '0x'.</summary>
    public static new HexTokenizer WithPrefix => withPrefix;

    /// <summary></summary>
    public HexTokenizer(bool take0X) : base(take0X) { }
}
