using Avalanche.Tokenizer;

public class tokenizers
{
    public static void Run()
    {
        {
            var tokenizer = AllTokenizer<TextToken>.Instance;
            TextToken token = tokenizer.Take<TextToken>("123456");
            // [0:6] TextToken:  "123456"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new AnyTokenizer<TextToken>(IntegerTokenizer.Instance, WhitespaceTokenizer.Any);
            TextToken token = tokenizer.Take<TextToken>("1234567890");
            // [0:10] TextToken:  "1234567890"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new CharTokenizer<TextToken>(mem => mem.Span[0] == '_');
            TextToken token = tokenizer.Take<TextToken>("____abc");
            // [0:4] TextToken:  "____"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new ConstantTokenizer<SeparatorToken>(",");
            SeparatorToken token = tokenizer.Take<SeparatorToken>(",x");
            // [0:1] SeparatorToken:  ","
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new FuncTokenizer<TextToken>(mem => new TextToken { Memory = mem.Slice(0, 3) });
            TextToken token = tokenizer.Take<TextToken>("____abc");
            // [0:3] TextToken:  "___"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = Tokenizers.Func<TextToken>(mem => new TextToken { Memory = mem.Slice(0, 3) });
            TextToken token = tokenizer.Take<TextToken>("____abc");
            // [0:3] TextToken:  "___"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = HexTokenizer<ValueToken>.WithoutPrefix;
            IToken token = tokenizer.Take<IToken>("0123456789ABCDEF")!;
            // [0:16] ValueToken:  "0123456789ABCDEF"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = HexTokenizer<DecimalToken>.WithPrefix;
            IToken token = tokenizer.Take<IToken>("0x0123456789ABCDEF")!;
            // [0:18] DecimalToken:  "0x0123456789ABCDEF"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }

        {
            var tokenizer = IntegerTokenizer<ValueToken>.Instance;
            IToken token = tokenizer.Take<IToken>("0123456789")!;
            // [0:10] ValueToken:  "0123456789"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = MalformedTokenizer<MalformedToken>.Instance;
            IToken token = tokenizer.Take<IToken>("¤§")!;
            // [0:1] MalformedToken:  "¤"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = NewLineTokenizer<NewLineToken>.Instance;
            IToken token = tokenizer.Take<IToken>("\n\n\n")!;
            // [0:1] NewLineToken:  "\n"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = RealTokenizer<DecimalToken>.Instance;
            IToken token = tokenizer.Take<IToken>("-123.45600e-12 asdf")!;
            // [0:14] DecimalToken:  "-123.45600e-12"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new RegexTokenizer<TextToken>("^[a-zA-Z0-9]+");
            IToken token = tokenizer.Take<IToken>("ab12 cd34")!;
            // [0:4] TextToken:  "ab12"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new SequenceTokenizer<CompositeToken>(
                    IntegerTokenizer.Instance,
                    new ConstantTokenizer<SeparatorToken>("="),
                    IntegerTokenizer.Instance
                );
            IToken token = tokenizer.Take<IToken>("10=20")!;
            //[0:5] CompositeToken:  "10=20"
            //├── [0:2] DecimalToken: "10"
            //├── [2:3] SeparatorToken: "="
            //└── [3:5] DecimalToken: "20"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new SequenceTokenizer<CompositeToken>(
                    (WhitespaceTokenizer.Any, false),
                    (IntegerTokenizer.Instance, true),
                    (WhitespaceTokenizer.Any, false),
                    (new ConstantTokenizer<SeparatorToken>("="), true),
                    (WhitespaceTokenizer.Any, false),
                    (IntegerTokenizer.Instance, true),
                    (WhitespaceTokenizer.Any, false)
                );
            IToken token = tokenizer.Take<IToken>("10=20")!;
            //[0:5] CompositeToken:  "10=20"
            //├── [0:2] DecimalToken: "10"
            //├── [2:3] SeparatorToken: "="
            //└── [3:5] DecimalToken: "20"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var commaTokenizer = new ConstantTokenizer<OperandToken>(",");
            var anyTokenizer = new AnyTokenizer(commaTokenizer, new UntilTokenizer(commaTokenizer));
            var tokenizer = 
            new SequenceTokenizer<CompositeToken>(
                    (WhitespaceTokenizer.Any, false, false),
                    (anyTokenizer, true, true),
                    (WhitespaceTokenizer.Any, false, false),
                    (anyTokenizer, true, true),
                    (WhitespaceTokenizer.Any, false, false)
                );
            IToken token = tokenizer.Take<IToken>("A,B")!;
            //[0:2] CompositeToken: "A,"
            //├── [0:1] TextToken: "A"
            //└── [1:2] OperandToken: ","
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new TrueTokenizer<TextToken>(new RegexTokenizer<TextToken>("^[a-zA-Z0-9]?"));
            TextToken token = tokenizer.Take<TextToken>("");
            // [0:0] TextToken:  ""
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new UntilTokenizer<CompositeToken>(NewLineTokenizer.Instance);
            IToken token = tokenizer.Take<IToken>("First line\nSecond line\nThird line")!;
            // [0:10] CompositeToken:  "First line"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new UntilTokenizer(new ConstantTokenizer(" "), false, '\\');
            IToken token = tokenizer.Take<IToken>(@"a\ b\ c d e f")!;
            // [0:7] TextToken:  "a\\ b\\ c"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = new WhileTokenizer<CompositeToken>(
                    new AnyTokenizer(
                        new RegexTokenizer<TextToken>("^[a-zA-Z0-9]+"),
                        WhitespaceTokenizer.AllButNewLine,
                        NewLineTokenizer.Instance
                    ),
                    yieldChildren: true
                );
            CompositeToken token = tokenizer.TakeAll<CompositeToken>("Hello world\nabc\n123");
            // [0:19] CompositeToken: "Hello world\nabc\n123"
            // ├── [0:5] TextToken: "Hello"
            // ├── [5:6] WhitespaceToken: " "
            // ├── [6:11] TextToken: "world"
            // ├── [11:12] NewLineToken: "\n"
            // ├── [12:15] TextToken: "abc"
            // ├── [15:16] NewLineToken: "\n"
            // └── [16:19] TextToken: "123"
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = WhitespaceTokenizer.Any;
            IToken token = tokenizer.Take<WhitespaceToken>("  \t\t  \n  ABC");
            // [0:9] WhitespaceToken:  "                 \n  "
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
        {
            var tokenizer = WhitespaceTokenizer.AllButNewLine;
            IToken token = tokenizer.Take<WhitespaceToken>("  \t\t  \n  ABC");
            // [0:6] WhitespaceToken:  "                 "
            Console.WriteLine(token.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }
    }
}

