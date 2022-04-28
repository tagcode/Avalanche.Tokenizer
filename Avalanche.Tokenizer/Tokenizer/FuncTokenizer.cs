// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Adapts delegate into <see cref="ITokenizer{T}"/>.</summary>
public class FuncTokenizer<T> : TokenizerBase<T> where T : IToken
{
    /// <summary>Tokenizer as delegate</summary>
    public delegate bool TokenizerFunc(ReadOnlyMemory<char> text, out T token);
    /// <summary>Delegate</summary>
    protected Func<ReadOnlyMemory<char>, T> func;
    /// <summary>Delegate</summary>
    public virtual Func<ReadOnlyMemory<char>, T> Func => func;

    /// <summary>Create tokenizer</summary>
    public FuncTokenizer(Func<ReadOnlyMemory<char>, T> func) : base()
    {
        this.func = func ?? throw new ArgumentNullException(nameof(func));
    }

    /// <summary></summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Try take
        token = this.Func(text);
        // Return
        return token.Memory.Length > 0;
    }
}
