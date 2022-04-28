// Copyright (c) Toni Kalajainen 2022
namespace Avalanche.Tokenizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalanche.Utilities;

/// <summary>Extension methods for <see cref="IToken"/>.</summary>
public static class TokenPrintTreeExtensions
{
    /// <summary>
    /// Vists token as tree structure
    /// 
    /// ├──IToken
    /// │  ├──IToken
    /// │  │  └──IToken
    /// </summary>
    /// <param name="token"></param>
    /// <param name="depth">(optional) depth</param>
    /// <param name="format">(optional) print format</param>
    /// <returns></returns>
    public static IEnumerable<Line> VisitTree(this IToken token, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        // Init queue
        List<Line> queue = new List<Line>();
        // Add to queue
        queue.Add(new Line(token, 0, 0UL));
        // Process queue
        while (queue.Count > 0)
        {
            // Next line
            int lastIx = queue.Count - 1;
            Line line = queue[lastIx];
            queue.RemoveAt(lastIx);

            //
            var children = line.Token.Children;
            // Got children
            if (children != null && children.Length > 0 && line.Level < depth)
            {
                int startIndex = queue.Count;
                try
                {
                    // Bitmask when this level continues
                    ulong levelContinuesBitMask = line.LevelContinuesBitMask | (line.Level < 64 ? 1UL << line.Level : 0UL);
                    // Add children in reverse order
                    for (int i = children.Length - 1; i >= 0; i--)
                    {
                        // Get child
                        IToken childToken = children[i];
                        // Create line
                        Line childLine = new Line(childToken, line.Level + 1, levelContinuesBitMask);
                        // Ad to queue
                        queue.Add(childLine);
                    }
                    // Last entry doesn't continue on its level.
                    if (children.Length >= 1) queue[startIndex] = queue[startIndex].NewLevelContinuesBitMask(line.LevelContinuesBitMask);
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
    public static void PrintTreeTo(this IToken token, TextWriter output, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        StringBuilder sb = new StringBuilder();
        List<int> columns = new List<int>();
        foreach (Line line in token.VisitTree(depth, format))
        {
            line.AppendTo(sb, format, columns);
            output.WriteLine(sb);
            sb.Clear();
        }
        output.Write(sb);
    }

    /// <summary>Print as tree structure.</summary>
    public static void PrintTreeTo(this IToken token, StringBuilder output, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        List<int> columns = new List<int>();
        foreach (Line line in token.VisitTree(depth, format))
        {
            line.AppendTo(output, format, columns);
            output.AppendLine();
        }
    }

    /// <summary>Print as tree structure.</summary>
    /// <returns>Tree as string</returns>
    public static String PrintTree(this IToken token, int depth = int.MaxValue, PrintFormat format = PrintFormat.Default)
    {
        StringBuilder sb = new StringBuilder();
        List<int> columns = new List<int>();
        foreach (Line line in token.VisitTree(depth, format))
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
        public IToken Token = null!;
        /// <summary>Depth level, starts at 0.</summary>
        public int Level;
        /// <summary>Bitmask for each level on whether the level has more lines.</summary>
        public ulong LevelContinuesBitMask;
        /// <summary>(optional) error is placed here.</summary>
        public Exception? Error;

        /// <summary>Create request line</summary>
        public Line(
            IToken token,
            int level,
            ulong levelContinuesBitMask,
            Exception? error = null)
        {
            Token = token;
            Level = level;
            LevelContinuesBitMask = levelContinuesBitMask;
            Error = error;
        }

        /// <summary>Create line with new value to <see cref="LevelContinuesBitMask"/>.</summary>
        /// <param name="newLevelContinuesBitMask"></param>
        /// <returns>line with new mask</returns>
        public Line NewLevelContinuesBitMask(ulong newLevelContinuesBitMask) => new Line(Token, Level, newLevelContinuesBitMask, Error);

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
            // Print [Index:Length]
            if ((format & (PrintFormat.Start|PrintFormat.End)) != 0)
            {
                if (column++ > 0) output.Append(' ');
                // Print '[' 
                output.Append('[');
                // Print 'index' 
                if (format.HasFlag(PrintFormat.Start)) output.Append(Token.Memory.Index());
                // Print ':' 
                if ((format & (PrintFormat.Start | PrintFormat.End)) == (PrintFormat.Start | PrintFormat.End)) output.Append(':');
                // Print '[' 
                if (format.HasFlag(PrintFormat.End)) output.Append(Token.Memory.Index()+Token.Memory.Length);
                // Print ']' 
                output.Append(']');
            }
            // Print name
            if (format.HasFlag(PrintFormat.Name))
            {
                if (column++ > 0) output.Append(' ');
                // Print name
                output.Append(Token.GetType().Name);
            }
            // Print Text
            if (format.HasFlag(PrintFormat.Text))
            {
                if (format.HasFlag(PrintFormat.Name)) output.Append(':');
                if (column++ > 0) output.Append(" ");
                output.Append('"');
                output.Append(Escaper.Backslash.Escape(Token.Memory));
                output.Append('"');
            }
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

    /// <summary>Print format flags</summary>
    [Flags]
    public enum PrintFormat : ulong
    {
        /// <summary>Print tree structure</summary>
        Tree = 1UL << 1,
        /// <summary>Name, e.g. "NameToken". </summary>
        Name = 1UL << 3,
        /// <summary>Print text</summary>
        Text = 1UL << 4,
        /// <summary>Print [start:end]</summary>
        Start = 1UL << 5,
        /// <summary>Print [index:length]</summary>
        End = 1UL << 6,
        /// <summary>NewLine</summary>
        NewLine = 1UL << 9,
        /// <summary>Default format</summary>
        DefaultLong = Tree | Name | Text | Start | End,
        /// <summary>Default format</summary>
        Default = Tree | Name | Text,
        /// <summary>Default format, short</summary>
        DefaultShort = Tree | Name,
        /// <summary>Default format, very short</summary>
        DefaultVeryShort = Name,
    }

}

