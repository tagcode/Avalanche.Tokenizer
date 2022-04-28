using Avalanche.Tokenizer;

class example
{
    public static void Run()
    {
        // Create tokenizer
        var valueTokenizer = IntegerTokenizer.Instance;
        var commaTokenizer = new ConstantTokenizer<SeparatorToken>(",");
        var malformedTokenizer = new UntilTokenizer<MalformedToken>(new AnyTokenizer(new ConstantTokenizer(","), WhitespaceTokenizer.Any));
        var slotTokenizer = new SequenceTokenizer<ValueToken>(
            (WhitespaceTokenizer.Any, false, false),
            (commaTokenizer, false, false),
            (WhitespaceTokenizer.Any, false, false),
            (new AnyTokenizer(valueTokenizer, malformedTokenizer), true, true),
            (WhitespaceTokenizer.Any, false, false)
        );
        var tokenizer = new WhileTokenizer(slotTokenizer, false);
        // Tokenize string
        IToken token = tokenizer.TakeAll<IToken>("1, x, 3".AsMemory());
        // Print token
        System.Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
    }
}
