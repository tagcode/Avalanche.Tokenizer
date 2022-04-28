<b>Avalanche.Tokenizer</b> contains string tokenizing utilities,
[[git]](https://github.com/tagcode/Avalanche.Tokenizer/Avalanche.Tokenizer), 
[[www]](https://avalanche.fi/Avalanche.Core/Avalanche.Tokenizer/docs/), 
[[licensing]](https://avalanche.fi/Avalanche.Core/license/index.html).

Add package reference to .csproj.
```xml
<PropertyGroup>
    <RestoreAdditionalProjectSources>https://avalanche.fi/Avalanche.Core/nupkg/index.json</RestoreAdditionalProjectSources>
</PropertyGroup>
<ItemGroup>
    <PackageReference Include="Avalanche.Tokenizer"/>
</ItemGroup>
```

This class library tokenizes strings.

```csharp
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
```

<pre>
[0:7] CompositeToken:  "1, x, 3"
├── [0:1] ValueToken:  "1"
│   └── [0:1] DecimalToken:  "1"
├── [1:4] ValueToken:  ", x"
│   ├── [1:2] SeparatorToken:  ","
│   ├── [2:3] WhitespaceToken:  " "
│   └── [3:4] MalformedToken:  "x"
└── [4:7] ValueToken:  ", 3"
    ├── [4:5] SeparatorToken:  ","
    ├── [5:6] WhitespaceToken:  " "
    └── [6:7] DecimalToken:  "3"
</pre>
