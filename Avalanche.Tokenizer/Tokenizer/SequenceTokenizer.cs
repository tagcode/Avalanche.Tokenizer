// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Linq;
using System.Runtime.Serialization;
using Avalanche.Utilities;

/// <summary>Takes with a sequence of tokenizers.</summary>
public class SequenceTokenizer<T> : TokenizerBase<T>, IReadOnly where T : IToken, new()
{
    /// <summary>Component tokenizers</summary>
    protected List<Entry> tokenizers = new();
    /// <summary>Component tokenizers</summary>
    public virtual IEnumerable<ITokenizerBase> Tokenizers => tokenizers.Select(e => e.tokenizer);

    /// <summary>Is read-only state</summary>
    [IgnoreDataMember] protected bool @readonly;
    /// <summary>Is read-only state</summary>
    [IgnoreDataMember] bool IReadOnly.ReadOnly { get => @readonly; set { if (@readonly == value) return; if (!value) throw new InvalidOperationException("Read-only"); if (value) setReadOnly(); } }
    /// <summary>Override this to modify assignment action.</summary>
    protected virtual void setReadOnly() => @readonly = true;

    /// <summary>Add <paramref name="tokenizer"/>.</summary>
    public SequenceTokenizer<T> Add(ITokenizer<IToken> tokenizer, bool required = true, bool yieldChildren = true) { this.AssertWritable().tokenizers.Add(new Entry(tokenizer, required, yieldChildren)); return this; }

    /// <summary>Create sequence of tokenizers</summary>
    public SequenceTokenizer(params ITokenizer<IToken>[] tokenizers)
    {
        this.tokenizers.AddRange(tokenizers.Select(t => new Entry(t, required: true, yieldChildren: false)));
    }

    /// <summary>Create sequence of tokenizers</summary>
    public SequenceTokenizer(params (ITokenizer<IToken> tokenizer, bool required)[] tokenizers2)
    {
        this.tokenizers.AddRange(tokenizers2.Select(t => new Entry(t.Item1, t.Item2, false)));
    }

    /// <summary>Create sequence of tokenizers</summary>
    public SequenceTokenizer(params (ITokenizer<IToken> tokenizer, bool required, bool yieldChildren)[] tokenizers3)
    {
        this.tokenizers.AddRange(tokenizers3.Select(t => new Entry(t.tokenizer, t.required, t.yieldChildren)));
    }

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T compositeToken)
    {
        // Place here tokens
        StructList8<IToken> tokens = new StructList8<IToken>();
        // Work slice
        ReadOnlyMemory<char> workArea = text;
        // Result slice
        ReadOnlyMemory<char> resultArea = default;
        // Try each
        foreach (Entry tokenizer in tokenizers)
        {
            // Try take
            if (!tokenizer.tokenizer.TryTake(workArea, out IToken token))
            {
                // Is required
                if (tokenizer.required) { compositeToken = default!; return false; }
                // Not required
                continue;
            }
            // Slice
            workArea = workArea.SliceAfter(token.Memory);
            // Merge result
            resultArea = tokens.Count == 0 ? token.Memory : MemoryExtensions.UnifyStringWith(resultArea, token.Memory);
            // Assign token's children
            if (token.Children != null && token.Children.Length>0 && tokenizer.yieldChildren)
            {
                foreach (IToken child in token.Children)
                    tokens.Add(child);
            }
            else
            {
                // Assign to array
                tokens.Add(token);
            }
        }
        // Create composite token
        compositeToken = new T() { Memory = resultArea, Children = tokens.ToArray() };
        // Done
        return true;
    }

    /// <summary>Tokenizer info entry</summary>
    public record struct Entry(ITokenizer<IToken> tokenizer, bool required, bool yieldChildren);
}

/// <summary>Takes with a sequence of tokenizers.</summary>
public class SequenceTokenizer : SequenceTokenizer<CompositeToken>
{
    /// <summary>Create sequence of tokenizers</summary>
    public SequenceTokenizer(params ITokenizer<IToken>[] tokenizers) : base(tokenizers) { }
    /// <summary>Create sequence of tokenizers</summary>
    public SequenceTokenizer(params (ITokenizer<IToken> tokenizer, bool required)[] tokenizers2) : base(tokenizers2) { }
    /// <summary>Create sequence of tokenizers</summary>
    public SequenceTokenizer(params (ITokenizer<IToken> tokenizer, bool required, bool yieldChildren)[] tokenizers3) : base(tokenizers3) { }
}
