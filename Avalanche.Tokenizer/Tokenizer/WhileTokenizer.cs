// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using Avalanche.Utilities;

/// <summary>Takes tokens while <see cref="ElementTokenizer"/> yields tokens.</summary>
public class WhileTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Element tokenizer</summary>
    protected ITokenizer<IToken> elementTokenizer;
    /// <summary>Add child tokens</summary>
    protected bool yieldChildren;
    /// <summary>Element tokenizer</summary>
    public virtual ITokenizer<IToken> ElementTokenizer => elementTokenizer;
    /// <summary>Add child tokens</summary>
    public virtual bool YieldChildren => yieldChildren;

    /// <summary></summary>
    /// <param name="elementTokenizer"></param>
    /// <param name="yieldChildren">Yield child elements from <paramref name="elementTokenizer"/>.</param>
    public WhileTokenizer(ITokenizer<IToken> elementTokenizer, bool yieldChildren = false)
    {
        this.elementTokenizer = elementTokenizer ?? throw new ArgumentNullException(nameof(elementTokenizer));
        this.yieldChildren = yieldChildren;
    }

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T compositeToken)
    {
        // Place here tokens
        StructList6<IToken> list = new();
        // Work area
        ReadOnlyMemory<char> workArea = text;
        // 
        while (workArea.Length>0 && elementTokenizer.TryTake(workArea, out IToken elementToken))
        {
            // No length
            if (elementToken.Memory.Length == 0) break;
            // Add children
            if (elementToken.Children != null && elementToken.Children.Length > 0 && yieldChildren)
            {
                foreach (IToken t in elementToken.Children) 
                    list.Add(t);
            } 
            else
            {
                // Add to list
                list.Add(elementToken);
            }
            // Slice
            workArea = workArea.SliceAfter(elementToken.Memory);
        }
        // Nothing was captured
        if (list.Count == 0) { compositeToken = default!; return false; }
        // Create result
        compositeToken = new T() { Memory = text.Slice(0, text.Length - workArea.Length), Children = list.ToArray() };
        return true;
    }
}

/// <summary>Takes tokens while elementTokenizer yields tokens.</summary>
public class WhileTokenizer : WhileTokenizer<CompositeToken>
{
    /// <summary></summary>
    /// <param name="elementTokenizer"></param>
    /// <param name="yieldChildren">Yield child elements from <paramref name="elementTokenizer"/>.</param>
    public WhileTokenizer(ITokenizer<IToken> elementTokenizer, bool yieldChildren = false) : base(elementTokenizer, yieldChildren) { }
}

