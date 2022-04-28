using Avalanche.Tokenizer;
using Avalanche.Utilities;
using static System.Console;

public class tokenizer
{
    public static void Run()
    {
        {
            // Text to tokenizer
            String @string = "1, x, 3, 4, 5, x, 7";
            ReadOnlyMemory<char> text = @string.AsMemory();
            // Get tokenizer from singleton
            var valueTokenizer = IntegerTokenizer.Instance;
            // Try take token
            if (valueTokenizer.TryTake(text, out DecimalToken token0))
            {
                // Print token
                WriteLine(token0); // "[0:1] DecimalToken "1""
                // Slice
                text = text.SliceAfter(token0.Memory);
            }
            // Create comma tokenizer 
            var commaTokenizer = new ConstantTokenizer<SeparatorToken>(",");
            // Try take ','
            if (commaTokenizer.TryTake(text, out SeparatorToken token2))
            {
                // Print token
                WriteLine(token2);
                // Slice
                text = text.SliceAfter(token2.Memory); // "[1:2] SeparatorToken ",""
            }
            // Try take white-space 
            if (WhitespaceTokenizer.Any.TryTake(text, out WhitespaceToken token1))
            {
                // Print token
                WriteLine(token1); // "[2:3] WhitespaceToken " ""
                // Slice
                text = text.SliceAfter(token1.Memory);
            }
            // Create malformed tokenizer
            var malformedTokenizer = new UntilTokenizer<MalformedToken>(new AnyTokenizer(new ConstantTokenizer(","), WhitespaceTokenizer.Any));
            // Try take 'x'
            if (malformedTokenizer.TryTake(text, out MalformedToken token3))
            {
                // Print token
                WriteLine(token3); // "[3:4] MalformedToken "x""
                // Slice
                text = text.SliceAfter(token3.Memory);
            }
            // Take ','
            IToken token4 = commaTokenizer.Take<IToken>(text)!;
            text = text.SliceAfter(token4.Memory);
            // Take ' '
            IToken token5 = WhitespaceTokenizer.Any.Take<IToken>(text)!;
            text = text.SliceAfter(token5.Memory);
            // Create " , x " slot tokenizer
            var slotTokenizer = new SequenceTokenizer<ValueToken>(
                (WhitespaceTokenizer.Any, false, false),
                (commaTokenizer, false, false),
                (WhitespaceTokenizer.Any, false, false),
                (new AnyTokenizer(valueTokenizer, malformedTokenizer), true, true), 
                (WhitespaceTokenizer.Any, false, false)
            );
            // Try take '3 '
            if (slotTokenizer.TryTake(text, out IToken token6))
            {
                // Print token
                WriteLine(token6); // "[6:7] ValueToken "3""
                // Slice
                text = text.SliceAfter(token6.Memory);
            }
            // Put together a while tokenizer that takes all integer/malformed parts and repeats while content lasts
            var whileTokenizer = new WhileTokenizer(slotTokenizer, false);
            // Try take all
            if (whileTokenizer.TryTake("1, x, 3, 4".AsMemory(), out IToken tokenAll)) WriteLine(tokenAll.PrintTree());
            // Print tokenizer as tree
            WriteLine(whileTokenizer.PrintTree());
            foreach (var line in whileTokenizer.VisitTree())
                WriteLine(line);
            IToken compositeToken = whileTokenizer.TakeAll<IToken>("1, x, 3, 4, 5, x, 7");
            WriteLine(compositeToken.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        }

        {
            // Text to tokenize
            ReadOnlyMemory<char> text = "ID001, ID002, ID003".AsMemory();
            // Get token
            if (IdentifierTokenizer.Instance.TryTake(text, out IdentifierToken identifierToken))
            {
                // Slice text
                text = text.SliceAfter(identifierToken.Memory);
                //
                WriteLine(identifierToken); // '[0:5] IdentifierToken "ID001"'
            }
        }
    }

    /// <summary>Tokenizes letters and digits as <see cref="IdentifierToken"/>.</summary>
    public class IdentifierTokenizer : TokenizerBase<IdentifierToken>
    {
        /// <summary>Singleton</summary>
        static IdentifierTokenizer instance = new IdentifierTokenizer();
        /// <summary>Singleton</summary>
        public static IdentifierTokenizer Instance => instance;

        public override bool TryTake(ReadOnlyMemory<char> text, out IdentifierToken token)
        {
            // Get span
            ReadOnlySpan<char> span = text.Span;
            // Accepted chars
            int ix = 0;
            // 
            for (int i=0; i<span.Length; i++)
            {
                // Not letter
                if (!char.IsLetterOrDigit(span[i])) break;
                // Accept char
                ix++;
            }
            // No chars were accepted
            if (ix == 0) { token = default!; return false; }
            // Return result
            token = new IdentifierToken { Memory = text.Slice(0, ix) };
            return true;
        }
    }
}

