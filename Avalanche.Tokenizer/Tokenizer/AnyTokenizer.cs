// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Avalanche.Utilities;

/// <summary>Tries a group of tokens from a list</summary>
public class AnyTokenizer : ITokenizer<IToken>, IReadOnly
{
    /// <summary>Component tokenizers</summary>
    protected List<ITokenizer<IToken>> tokenizers = new();
    /// <summary>Component tokenizers</summary>
    public virtual IEnumerable<ITokenizer<IToken>> Tokenizers => tokenizers.ToArray();

    /// <summary>Is read-only state</summary>
    [IgnoreDataMember] protected bool @readonly;
    /// <summary>Is read-only state</summary>
    [IgnoreDataMember] bool IReadOnly.ReadOnly { get => @readonly; set { if (@readonly == value) return; if (!value) throw new InvalidOperationException("Read-only"); if (value) setReadOnly(); } }
    /// <summary>Override this to modify assignment action.</summary>
    protected virtual void setReadOnly() => @readonly = true;

    /// <summary>Add <paramref name="tokenizer"/>.</summary>
    public AnyTokenizer Add(ITokenizer<IToken> tokenizer, bool required = true) { this.AssertWritable().tokenizers.Add(tokenizer); return this; }

    /// <summary>Create with empty list</summary>
    public AnyTokenizer() : base() { }
    /// <summary>Create with internal list</summary>
    public AnyTokenizer(IEnumerable<ITokenizer<IToken>> initialList) : base() 
    {
        this.tokenizers.AddRange(initialList);
    }
    /// <summary>Create from params</summary>
    public AnyTokenizer(params ITokenizer<IToken>[] initialTokens) : base() 
    {
        this.tokenizers.AddRange(initialTokens);
    }

    /// <summary>Peek to test whether <paramref name="text"/> starts with token.</summary>
    public virtual bool Peek(ReadOnlyMemory<char> text) => TryTake(text, out IToken token);

    /// <summary>Try to take with each tokenizer.</summary>
    public virtual bool TryTake(ReadOnlyMemory<char> text, out IToken token)
    {
        // Try each tokenizer
        foreach (ITokenizer<IToken> tokenizer in tokenizers)
        {
            // Try take and cast as T
            if (tokenizer.TryTake(text, out token)) return true;
        }
        // Failed
        token = default!;
        return false;
    }

    /// <summary></summary>
    public override string ToString() => this.PrintTree();
}

/// <summary>Tries a group of tokens from a list</summary>
public class AnyTokenizer<T> : AnyTokenizer, ITokenizer<T> where T : IToken, new()
{
    /// <summary>Add <paramref name="tokenizer"/>.</summary>
    public new AnyTokenizer Add(ITokenizer<IToken> tokenizer, bool required = true) { this.AssertWritable().tokenizers.Add(tokenizer); return this; }

    /// <summary>Create with empty list</summary>
    public AnyTokenizer() : base() { }
    /// <summary>Create with internal list</summary>
    public AnyTokenizer(IEnumerable<ITokenizer<IToken>> initialList) : base(initialList) { }
    /// <summary>Create from params</summary>
    public AnyTokenizer(params ITokenizer<IToken>[] initialTokens) : base(initialTokens) { }

    /// <summary></summary>
    bool ITokenizer<T>.TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Try each tokenizer
        foreach (ITokenizer<IToken> tokenizer in tokenizers)
        {
            // Try take and cast as T
            if (tokenizer.TryTake(text, out IToken token_)) { token = token_ is T casted ? casted : new T() { Memory = token_.Memory, Children = token_.Children }; return true; }
        }
        // Failed
        token = default!;
        return false;

    }
}
