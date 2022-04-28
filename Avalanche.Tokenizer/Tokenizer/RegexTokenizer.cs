// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalanche.Utilities;

/// <summary>Tokenizes with <see cref="System.Text.RegularExpressions.Regex"/>.</summary>
public class RegexTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary></summary>
    protected Regex regex;
    /// <summary></summary>
    public virtual Regex Regex => regex;

    /// <summary></summary>
    public RegexTokenizer(Regex regex)
    {
        this.regex = regex;
    }

    /// <summary></summary>
    public RegexTokenizer(string pattern) : this(new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline)) { }

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Get-or-create string
        if (!MemoryMarshal.TryGetString(text, out string? @string, out int start, out int length))
        {
            @string = text.AsString();
            start = 0;
            length = text.Length;
        }
        // Do match
        Match match = Regex.Match(@string, start, length);
        // No match
        if (!match.Success) { token = default!; return false; }
        //
        ReadOnlyMemory<char> slice = text.Slice(match.Index-start, match.Length);
        //
        token = new T() { Memory = slice };
        return true;
    }
}

/// <summary>Tokenizes with <see cref="Regex"/>.</summary>
public class RegexTokenizer : RegexTokenizer<TextToken>
{
    // Saved from the old tokenizer code //
    /// <summary></summary>
    const RegexOptions opts = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
    /// <summary></summary>
    static RegexTokenizer integer = new RegexTokenizer(new Regex(@"^\d+", opts));
    /// <summary></summary>
    static RegexTokenizer hex = new RegexTokenizer(new Regex(@"0[xX][0-9a-fA-F]+", opts)); // 0x000000000000000
    /// <summary></summary>
    static RegexTokenizer @float = new RegexTokenizer(new Regex(@"^\d+((\.\d+)([eE][-+]?\d+)?)|((\.\d+)?([eEc][-+]?\d+))", opts)); // "5.0" "5e0" "5e0.0"
    /// <summary></summary>
    static RegexTokenizer fieldModifiers = new RegexTokenizer(new Regex(@"^[\#\-]{1,2}", opts));
    /// <summary></summary>
    static RegexTokenizer whitespace = new RegexTokenizer(new Regex(@"^[ \t\u00ff]+", opts));
    /// <summary></summary>
    static RegexTokenizer linefeed = new RegexTokenizer(new Regex(@"^\r*\n", opts));
    /// <summary></summary>
    static RegexTokenizer pow = new RegexTokenizer(new Regex(@"^pow", opts));
    /// <summary></summary>
    static RegexTokenizer ellipsis = new RegexTokenizer(new Regex(@"^(…|\.\.\.)", opts));
    /// <summary></summary>
    static RegexTokenizer range = new RegexTokenizer(new Regex(@"^\.\.", opts));
    /// <summary></summary>
    static RegexTokenizer coalesce = new RegexTokenizer(new Regex(@"^\?\?", opts));
    /// <summary></summary>
    static RegexTokenizer nameLiteral = new RegexTokenizer(new Regex(@"^(\\[0-9])?([^\./\\:\!#\-\+\^(\)\{\}\[\]&\<\>\|"",\n\t\r=\*\^\?;§…\$%~ ]|(\\[\."",ntr/\\:\!#&\-\+\^,\(\)\{\}\[\]\|=\*\^\?;§…\$%~ ]))+", opts));
    /// <summary></summary>
    static RegexTokenizer atNameLiteral = new RegexTokenizer(new Regex(@"@([^/\\:\!#-\+\^(\)\{\}\[\]&\<\>\|"",\n\t\r=\*\^\? ]|(\\["",ntr/\\:\!#&-\+\^,\(\)\{\}\[\]\|=\*\^\? ]))+", opts));
    /// <summary></summary>
    static RegexTokenizer atQuotedNameLiteral = new RegexTokenizer(new Regex(@"^@""([^\\""\n\t\r]|(\\[""ntr\\]))+""", opts));
    /// <summary></summary>
    static RegexTokenizer atQuotedLongNameLiteral = new RegexTokenizer(new Regex("^@\"\"\".*\"\"\"+", opts));
    /// <summary></summary>
    static RegexTokenizer stringLiteral = new RegexTokenizer(new Regex(@"^""([^\\""\n\r]|\\[""ntr\\]|\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{2})+""", opts));
    /// <summary></summary>
    static RegexTokenizer longStringLiteral = new RegexTokenizer(new Regex("^\"\"\".*\"\"\"+", opts));
    /// <summary></summary>
    static RegexTokenizer lineCommentLiteral = new RegexTokenizer(new Regex("^//[^\r\n]*", opts));
    /// <summary></summary>
    static RegexTokenizer documentCommentLiteral = new RegexTokenizer(new Regex("^///(?!/)[^\r\n]*", opts));
    /// <summary></summary>
    static RegexTokenizer blockCommentLiteral = new RegexTokenizer(new Regex("^/\\*.*\\*/", opts));

    /// <summary></summary>
    public static RegexTokenizer Integer => integer;
    /// <summary></summary>
    public static RegexTokenizer Hex => hex;
    /// <summary></summary>
    public static RegexTokenizer Float => @float;
    /// <summary></summary>
    public static RegexTokenizer FieldModifiers => fieldModifiers;
    /// <summary></summary>
    public static RegexTokenizer Whitespace => whitespace;
    /// <summary></summary>
    public static RegexTokenizer Linefeed => linefeed;
    /// <summary></summary>
    public static RegexTokenizer Pow => pow;
    /// <summary></summary>
    public static RegexTokenizer Ellipsis => ellipsis;
    /// <summary></summary>
    public static RegexTokenizer Range => range;
    /// <summary></summary>
    public static RegexTokenizer Coalesce => coalesce;
    /// <summary></summary>
    public static RegexTokenizer NameLiteral => nameLiteral;
    /// <summary></summary>
    public static RegexTokenizer AtNameLiteral => atNameLiteral;
    /// <summary></summary>
    public static RegexTokenizer AtQuotedNameLiteral => atQuotedNameLiteral;
    /// <summary></summary>
    public static RegexTokenizer AtQuotedLongNameLiteral => atQuotedLongNameLiteral;
    /// <summary></summary>
    public static RegexTokenizer StringLiteral => stringLiteral;
    /// <summary></summary>
    public static RegexTokenizer LongStringLiteral => longStringLiteral;
    /// <summary></summary>
    public static RegexTokenizer LineCommentLiteral => lineCommentLiteral;
    /// <summary></summary>
    public static RegexTokenizer DocumentCommentLiteral => documentCommentLiteral;
    /// <summary></summary>
    public static RegexTokenizer BlockCommentLiteral => blockCommentLiteral;

    /// <summary></summary>
    static Regex NameLiteralEscape = new Regex(@"[""\!,\n\t\r\\:#&-\+\^,\(\)\{\}\[\]\|=\*\^\? ]", opts);
    /// <summary></summary>
    static Regex NameLiteralUnescape = new Regex(@"\\(.)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    /// <summary></summary>
    static Regex StringLiteralEscape = new Regex(@"[""\n\t\r\\]", opts);
    /// <summary></summary>
    static Regex StringLiteralUnescape = new Regex(@"\\(u[0-9a-fA-F]{4}|x[0-9a-fA-F]{2}|.)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    /// <summary></summary>
    public static String EscapeNameLiteral(String input) => NameLiteralEscape.Replace(input, EscapeChar);
    /// <summary></summary>
    public static String UnescapeNameLiteral(String input) => NameLiteralUnescape.Replace(input, UnescapeChar);
    /// <summary></summary>
    public static String EscapeStringLiteral(String input) => StringLiteralEscape.Replace(input, EscapeChar);
    /// <summary></summary>
    public static String UnescapeStringLiteral(String input) => StringLiteralUnescape.Replace(input, UnescapeChar);
    /// <summary></summary>
    public static String EscapeChar(Match m) => @"\" + m.Value;
    /// <summary></summary>
    public static String UnescapeChar(Match m)
    {
        string capture = m.Groups[1].Value;
        switch (capture[0])
        {
            case 't': return "\t";
            case 'n': return "\n";
            case 'r': return "\r";
            case 'u':
                char ch = (char)Avalanche.Utilities.Hex.ToUInt(capture.Substring(1));
                return new string(ch, 1);
            case 'x':
                char c = (char)Avalanche.Utilities.Hex.ToUInt(capture.Substring(1));
                return new string(c, 1);
            default: return capture;
        }
    }
    // Saved from the old tokenizer code ^^ //

    /// <summary></summary>
    public RegexTokenizer(Regex regex) : base(regex) { }
    /// <summary></summary>
    public RegexTokenizer(string pattern) : base(pattern) { }
}

