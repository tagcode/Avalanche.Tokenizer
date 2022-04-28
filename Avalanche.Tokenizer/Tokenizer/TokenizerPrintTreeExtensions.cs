// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Avalanche.Utilities;
using Avalanche.Utilities.Provider;
using Avalanche.Utilities.Record;

/// <summary>Extension methods for <see cref="ITokenizerBase"/>.</summary>
public static class TokenizerPrintTreeExtensions
{
    /// <summary>
    /// Vists tokenizer as tree structure
    /// 
    /// ├──ITokenizerBase
    /// │  ├──ITokenizerBase
    /// │  │  └──ITokenizerBase
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <param name="depth">(optional) depth</param>
    /// <param name="format">(optional) print format</param>
    /// <returns></returns>
    public static IEnumerable<Line> VisitTree(this ITokenizerBase tokenizer, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        // Init queue
        List<Line> queue = new List<Line>();
        // Visited set (for cycle detection)
        var visited = TokenizerInfo.InfoProvider.Cached(ReferenceEqualityComparer<ITokenizerBase>.Instance);
        // Read info
        TokenizerInfo rootInfo = visited[tokenizer];
        // Add to queue
        queue.Add(new Line(tokenizer, rootInfo.Name, null, 0, rootInfo.Fields, rootInfo.References, 0UL, rootInfo.Error));
        // Process queue
        while (queue.Count > 0)
        {
            // Next line
            int lastIx = queue.Count - 1;
            Line line = queue[lastIx];
            queue.RemoveAt(lastIx);

            // Got children
            if (line.References != null && line.Level < depth)
            {
                int startIndex = queue.Count;
                try
                {
                    // Bitmask when this level continues
                    ulong levelContinuesBitMask = line.LevelContinuesBitMask | (line.Level < 64 ? 1UL << line.Level : 0UL);
                    // Add children in reverse order
                    for (int i = line.References.Count - 1; i >= 0; i--)
                    {
                        string reference = line.References[i].Key;
                        ITokenizerBase childTokenizer = line.References[i].Value;
                        // Detected cycle
                        bool isCycle = visited.Map.ContainsKey(childTokenizer);
                        // Get info
                        TokenizerInfo childInfo = visited[childTokenizer];
                        // Create line
                        Line childLine = new Line(childTokenizer, childInfo.Name, reference, line.Level + 1, childInfo.Fields, isCycle ? null : childInfo.References, levelContinuesBitMask, childInfo.Error);
                        // Ad to queue
                        queue.Add(childLine);
                    }
                    // Last entry doesn't continue on its level.
                    if (line.References.Count >= 1) queue[startIndex] = queue[startIndex].NewLevelContinuesBitMask(line.LevelContinuesBitMask);
                }
                catch (Exception e)
                {
                    // Add error to be yielded along
                    line.Error = e;
                }
            }
            // yield line
            yield return line;
        }
    }

    /// <summary>Print as tree structure.</summary>
    public static void PrintTreeTo(this ITokenizerBase tokenizer, TextWriter output, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        StringBuilder sb = new StringBuilder();
        List<int> columns = new List<int>();
        foreach (Line line in tokenizer.VisitTree(depth, format))
        {
            line.AppendTo(sb, format, columns);
            output.WriteLine(sb);
            sb.Clear();
        }
        output.Write(sb);
    }

    /// <summary>Print as tree structure.</summary>
    public static void PrintTreeTo(this ITokenizerBase tokenizer, StringBuilder output, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        List<int> columns = new List<int>();
        foreach (Line line in tokenizer.VisitTree(depth, format))
        {
            line.AppendTo(output, format, columns);
            output.AppendLine();
        }
    }

    /// <summary>Print as tree structure.</summary>
    /// <returns>Tree as string</returns>
    public static String PrintTree(this ITokenizerBase tokenizer, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        StringBuilder sb = new StringBuilder();
        List<int> columns = new List<int>();
        foreach (Line line in tokenizer.VisitTree(depth, format))
        {
            line.AppendTo(sb, format, columns);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>Line.</summary>
    public struct Line
    {
        /// <summary></summary>
        public ITokenizerBase Tokenizer = null!;
        /// <summary></summary>
        public string? Name = null!;
        /// <summary>Relation to parent</summary>
        public string? Relation;
        /// <summary>Fields printed as strings</summary>
        public IList<KeyValuePair<string, object?>>? Fields;
        /// <summary></summary>
        public IList<KeyValuePair<string, ITokenizerBase>>? References;
        /// <summary>Depth level, starts at 0.</summary>
        public int Level;
        /// <summary>Bitmask for each level on whether the level has more lines.</summary>
        public ulong LevelContinuesBitMask;
        /// <summary>(optional) error is placed here.</summary>
        public Exception? Error;

        /// <summary>Create request line</summary>
        public Line(
            ITokenizerBase tokenizer,
            string? name,
            string? relation,
            int level,
            IList<KeyValuePair<string, object?>>? fields,
            IList<KeyValuePair<string, ITokenizerBase>>? references,
            ulong levelContinuesBitMask,
            Exception? error = null)
        {
            Tokenizer = tokenizer;
            Name = name;
            Relation = relation;
            Level = level;
            Fields = fields;
            References = references;
            LevelContinuesBitMask = levelContinuesBitMask;
            Error = error;
        }

        /// <summary>Create line with new value to <see cref="LevelContinuesBitMask"/>.</summary>
        /// <param name="newLevelContinuesBitMask"></param>
        /// <returns>line with new mask</returns>
        public Line NewLevelContinuesBitMask(ulong newLevelContinuesBitMask) => new Line(Tokenizer, Name, Relation, Level, Fields, References, newLevelContinuesBitMask, Error);

        /// <summary>Tests whether there will be more sections to specific <paramref name="level"/>.</summary>
        public bool LevelContinues(int level)
        {
            // Undefined
            if (level == 0) return false;
            // Not supported after 64 levels
            if (level > 64) return false;
            // Read the bit
            return (LevelContinuesBitMask & 1UL << (level - 1)) != 0UL;
        }

        /// <summary>Write to <see cref="StringBuilder"/> <paramref name="output"/>.</summary>
        public void AppendTo(StringBuilder output, PrintFormat format, IList<int> columns)
        {
            // Number of info fields printed
            int column = 0;

            // Print tree
            if (format.HasFlag(PrintFormat.Tree) && Level > 0)
            {
                // Print indents
                for (int l = 1; l < Level; l++)
                {
                    // Append " "
                    if (column++ > 0) output.Append(' ');
                    //
                    output.Append(LevelContinues(l) ? "│  " : "   ");
                }
                // Print last indent
                if (Level >= 1)
                {
                    // Append " "
                    if (column++ > 0) output.Append(' ');
                    //
                    output.Append(LevelContinues(Level) ? "├──" : "└──");
                }
            }
            // Is last element in string buffer relation to parent (Fields[x], Constraint, ...)
            bool relationPrinted = false;
            if (format.HasFlag(PrintFormat.Relation) && Relation != null)
            {
                if (column++ > 0) output.Append(' ');
                output.Append(Relation);
                relationPrinted = true;
            }
            // Print name
            if (format.HasFlag(PrintFormat.Type) && Name != null)
            {
                if (relationPrinted) { output.Append(" ="); relationPrinted = false; }
                if (column++ > 0) output.Append(' ');
                // Print name
                output.Append(Name);
            }
            //
            bool gotNullFields = format.HasFlag(PrintFormat.FieldsNull) && Fields!=null && !Fields.Where(f => f.Value == null).IsEmpty();
            bool gotNonNullFields = format.HasFlag(PrintFormat.FieldsNonNull) && Fields!=null && !Fields.Where(f => f.Value != null).IsEmpty();
            // Opener '{'
            if (gotNullFields || gotNonNullFields) 
            {
                if (relationPrinted) { output.Append(" ="); relationPrinted = false; }
                if (column++ > 0) output.Append(' '); 
                output.Append("{ "); 
            }
            // Field index
            int fieldIx = 0;
            // Print properties
            if (gotNullFields||gotNonNullFields)
            {
                // 
                for (int i = 0; i < Fields!.Count; i++)
                {
                    var field = Fields[i];
                    if (field.Value == null && (format & PrintFormat.FieldsNull) == 0) continue;
                    if (field.Value != null && (format & PrintFormat.FieldsNonNull) == 0) continue;
                    if (fieldIx++ > 0) output.Append(", ");
                    output.Append(field.Key);
                    output.Append(" = ");
                    AppendParameterValue(output, field.Value);
                }
            }
            // Closer '}'
            if (gotNullFields || gotNonNullFields) { if (column++ > 0) output.Append(' '); output.Append("} "); }
            // Next line
            if (format.HasFlag(PrintFormat.NewLine)) output.AppendLine();
        }

        /// <summary>Formulate <paramref name="value"/> to append into <paramref name="output"/>.</summary>
        void AppendParameterValue(StringBuilder output, object? value)
        {
            //
            if (value == null) output.Append("null");
            // Print string
            else if (value is String str)
            {
                output.Append('"');
                output.Append(Escaper.Quotes.Escape(str));
                output.Append('"');
            }
            // Print Regex
            else if (value is Regex regex)
            {
                output.Append('"');
                output.Append(Escaper.Quotes.Escape(regex.ToString()));
                output.Append('"');
            }
            // Print Type
            else if (value is Type type)
            {
                output.Append(CanonicalName.Print(type, CanonicalNameOptions.IncludeGenerics));
            }
            // Print enumr
            else if (value is IEnumerable enumr)
            {
                output.Append('[');
                int jx = 0;
                foreach (object o in enumr)
                {
                    if (jx++ > 0) output.Append(", ");
                    AppendParameterValue(output, o);
                }
                output.Append(']');
            }
            // Print value
            else output.Append(value);
        }

        /// <summary>Print as string</summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(PrintFormat format)
        {
            StringBuilder sb = new StringBuilder();
            List<int> columns = new List<int>();
            AppendTo(sb, format, columns);
            return sb.ToString();
        }

        /// <summary>Print as string</summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            List<int> columns = new List<int>();
            AppendTo(sb, PrintFormat.Default, columns);
            return sb.ToString();
        }
    }

    /// <summary>Extracted tokenizer info</summary>
    public record TokenizerInfo
    {
        /// <summary>Info provider</summary>
        static readonly IProvider<ITokenizerBase, TokenizerInfo> infoProvider = Providers.Func<ITokenizerBase, TokenizerInfo>(Read);
        /// <summary>Info provider</summary>
        static readonly IProvider<ITokenizerBase, IResult<TokenizerInfo>> infoResultProvider = infoProvider.ResultCaptured();
        /// <summary>Info provider</summary>
        public static IProvider<ITokenizerBase, TokenizerInfo> InfoProvider => infoProvider;
        /// <summary>Info provider</summary>
        public static IProvider<ITokenizerBase, IResult<TokenizerInfo>> InfoResultProvider => infoResultProvider;

        /// <summary></summary>
        public ITokenizerBase Tokenizer = null!;
        /// <summary>Reference to parent</summary>
        public string Name => CanonicalName.Print(Tokenizer.GetType(), CanonicalNameOptions.IncludeGenerics);
        /// <summary></summary>
        public Type? Type => Tokenizer.GetType();
        /// <summary>Fields printed as strings</summary>
        public List<KeyValuePair<string, object?>> Fields = new();
        /// <summary>References to other tokenizers</summary>
        public List<KeyValuePair<string, ITokenizerBase>> References = new();
        /// <summary>Captured errors</summary>
        public Exception? Error = null!;

        /// <summary>Extract values from <paramref name="tokenizer"/>.</summary>
        public static TokenizerInfo Read(ITokenizerBase tokenizer)
        {
            // Create result
            TokenizerInfo result = new TokenizerInfo { Tokenizer = tokenizer };
            // Get description
            IResult<IRecordDescription> recordDescriptionResult = RecordDescription.CachedResult[tokenizer.GetType()];
            // Got error
            if (recordDescriptionResult.Error != null) result.Error = recordDescriptionResult.Error;
            // Assign other fields
            else
            {
                try
                {
                    // Dive into fields
                    foreach (IFieldDescription field in recordDescriptionResult.AssertValue().Fields)
                    {
                        // Get field name
                        string? fieldName = field.Name?.ToString();
                        // No field name
                        if (string.IsNullOrEmpty(fieldName)) continue;
                        // Read value
                        object? value = FieldReadFuncOO.Cached[field](tokenizer);
                        // Got reference
                        if (value is ITokenizerBase tokenizerReference0)
                        {
                            // Add reference
                            result.References.Add(new KeyValuePair<string, ITokenizerBase>(fieldName, tokenizerReference0));
                        }
                        // Got reference array
                        else if (value is IEnumerable<ITokenizerBase> enumr)
                        {
                            //
                            int ix = 0;
                            // Add references
                            foreach (ITokenizerBase tokenizerReference1 in enumr)
                            {
                                result.References.Add(new KeyValuePair<string, ITokenizerBase>($"{fieldName}[{ix}]", tokenizerReference1));
                                ix++;
                            }
                        }
                        // Got value
                        else
                        {
                            // Got other field
                            result.Fields.Add(new KeyValuePair<string, object?>(fieldName, value));
                        }
                    }
                }
                catch (Exception e)
                {
                    result.Error = e;
                }
            }
            //
            return result;
        }
    }


    /// <summary>Print format flags</summary>
    [Flags]
    public enum PrintFormat : ulong
    {
        /// <summary>Print tree structure</summary>
        Tree = 1UL << 1,
        /// <summary>Print relation to parent</summary>
        Relation = 1UL << 2,
        /// <summary>Print type name, e.g. "SequenceTokenizer". </summary>
        Type = 1UL << 3,
        /// <summary>Print non-null fields, e.g. "Annotations = []"</summary>
        FieldsNonNull = 1UL << 7,
        /// <summary>Print null fields</summary>
        FieldsNull = 1UL << 8,
        /// <summary>NewLine</summary>
        NewLine = 1UL << 9,
        /// <summary>Default format, long</summary>
        DefaultLong = Tree | Relation | FieldsNonNull | FieldsNull,
        /// <summary>Default format</summary>
        Default = Tree | Relation | Type | FieldsNonNull,
        /// <summary>Default format, short</summary>
        DefaultShort = Tree | Relation | Type,
        /// <summary>Default format, very short</summary>
        DefaultVeryShort = Tree | Type,
    }

}

