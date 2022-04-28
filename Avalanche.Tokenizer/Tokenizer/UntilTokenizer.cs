// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>
/// Takes chars until <see cref="EndConditionTokenizer"/> is detectd.
/// 
/// Escape character and following character are disregarded from being considered as end condition.
/// </summary>
public class UntilTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Condition that ends </summary>
    protected ITokenizer<IToken> endConditionTokenizer;
    /// <summary>Policy whether <see cref="EndConditionTokenizer"/> must consume all remaining text.</summary>
    protected bool endsWithEndCondition;
    /// <summary>Optional escape character, e.g. '\\'</summary>
    protected char? escapeCharacter;

    /// <summary>Condition that ends </summary>
    public virtual ITokenizer<IToken> EndConditionTokenizer => endConditionTokenizer;
    /// <summary>Policy whether <see cref="EndConditionTokenizer"/> must consume all remaining text.</summary>
    public virtual bool EndsWithEndCondition => endsWithEndCondition;
    /// <summary>Optional escape character, e.g. '\\'</summary>
    public virtual char? EscapeCharacter => escapeCharacter;

    /// <summary>Create until tokenizer</summary>
    /// <param name="endConditionTokenizer">End condition tokenizer</param>
    /// <param name="endsWithEndCondition">Policy whether <paramref name="endConditionTokenizer"/> must consume all remaining text</param>
    /// <param name="escapeCharacter">Optional escape character, e.g. '\\'</param>
    public UntilTokenizer(ITokenizer<IToken> endConditionTokenizer, bool endsWithEndCondition = false, char? escapeCharacter = null)
    {
        this.endConditionTokenizer = endConditionTokenizer ?? throw new ArgumentNullException(nameof(endConditionTokenizer));
        this.endsWithEndCondition = endsWithEndCondition;
        this.escapeCharacter = escapeCharacter;
    }

    /// <summary>Take until end condition</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Empty length
        if (text.Length == 0) { token = default!; return false; }
        // Number of chars accepted as token
        int ix = 0;
        // Scan valid chars
        for (int i = 0; i < text.Length; i++)
        {
            // Got escape char
            if (escapeCharacter.HasValue && text.Span[i] == escapeCharacter.Value)
            {
                // Accept one or two chars
                ix += text.Length > 1 ? 2 : 1;
                //
                continue;
            }
            // Remaining chars
            ReadOnlyMemory<char> remaining = text.Slice(ix);
            // Process with all remaining characters consuming end condition
            if (endsWithEndCondition)
            {
                // End condition found.
                if (endConditionTokenizer.TryTake(remaining, out IToken token0))
                {
                    // End condition found.
                    if (i + token0.Memory.Length == text.Length) break;
                }
            }
            // Peek suffices
            else {
                // End condition found.
                if (endConditionTokenizer.Peek(remaining)) break;
            }
            // Accept char
            ix = i+1;
        }
        // No accepted chars
        if (ix == 0) { token = default!; return false; }
        // Create token
        token = text.As<T>(0, ix);
        // Return 
        return true;
    }
}

/// <summary>Takes chars until condition token is detected.</summary>
public class UntilTokenizer : UntilTokenizer<TextToken>
{
    /// <summary>Create until tokenizer</summary>
    /// <param name="endConditionTokenizer">End condition tokenizer</param>
    /// <param name="endsWithEndCondition">Policy whether <paramref name="endConditionTokenizer"/> must consume all remaining text</param>
    /// <param name="escapeChar">Optional escape character, e.g. '\\'</param>
    public UntilTokenizer(ITokenizer<IToken> endConditionTokenizer, bool endsWithEndCondition = false, char? escapeChar = null) : base(endConditionTokenizer, endsWithEndCondition, escapeChar) { }
}
