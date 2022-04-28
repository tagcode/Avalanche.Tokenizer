// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;

/// <summary>Takes the next character as <see cref="MalformedToken"/>.</summary>
public class MalformedTokenizer<T> : TokenizerBase<T> where T : IToken, new()
{
    /// <summary>Singleton</summary>
    static MalformedTokenizer<T> instance = new MalformedTokenizer<T>();
    /// <summary>Singleton</summary>
    public static MalformedTokenizer<T> Instance => instance;

    /// <summary>Try take format string.</summary>
    public override bool TryTake(ReadOnlyMemory<char> text, out T token)
    {
        // Empty length
        if (text.IsEmpty) { token = default!; return false; }
        // Get single char token
        token = text.As<T>(0, 1);
        return true;
    }
}

/// <summary>Takes the next character as <see cref="MalformedToken"/>.</summary>
public class MalformedTokenizer : MalformedTokenizer<MalformedToken>
{
    /// <summary>Singleton</summary>
    static MalformedTokenizer instance = new MalformedTokenizer();
    /// <summary>Singleton</summary>
    public new static MalformedTokenizer Instance => instance;
}
