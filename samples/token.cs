using Avalanche.Tokenizer;
using Avalanche.Utilities;
using static System.Console;

public class token
{
    public static void Run()
    {
        // The tokenized text
        ReadOnlyMemory<char> text = "1 + 2 = 3".AsMemory();
        // Create composite token manually, for purpose of example
        CompositeToken compositeToken = new CompositeToken
        {
            Memory = text,
            Children = new IToken[]
            {
                    new DecimalToken { Memory = text.Slice(0,1) },
                    new WhitespaceToken { Memory = text.Slice(1,1) },
                    new OperandToken { Memory = text.Slice(2, 1) },
                    new WhitespaceToken { Memory = text.Slice(3,1) },
                    new DecimalToken { Memory = text.Slice(4,1) },
                    new WhitespaceToken { Memory = text.Slice(5,1) },
                    new OperandToken { Memory = text.Slice(6, 1) },
                    new WhitespaceToken { Memory = text.Slice(7,1) },
                    new DecimalToken { Memory = text.Slice(8,1) },
            }
        };
        WriteLine(compositeToken.PrintTree());
        WriteLine(compositeToken.PrintTree(format: TokenPrintTreeExtensions.PrintFormat.DefaultLong));
        foreach (var line in compositeToken.VisitTree())
            WriteLine(line);
        string text_ = compositeToken.Text();
        int index = compositeToken.Memory.Index(), length = compositeToken.Memory.Length;
        compositeToken.Accept(new TokenVisitor());
    }

    public class TokenVisitor : ITokenVisitor<CompositeToken>, ITokenVisitor<DecimalToken>, ITokenVisitor<OperandToken>
    {
        public void Visit(CompositeToken token)
        {
            foreach (IToken child in token.Children)
                child.Accept(this);
        }
        public void Visit(DecimalToken token) => Console.WriteLine(token);
        public void Visit(OperandToken token) => Console.WriteLine(token);
    }
}

