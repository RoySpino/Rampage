using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace rpgc.Text
{
    public sealed class SourceText
    {
        public ImmutableArray<TextLine> Lines { get; }
        private string _Text;
        public override string ToString() => _Text;
        public string ToString(int Start, int length) => _Text.Substring(Start, length);
        public char this[int index] => ((index < _Text.Length) ?_Text[index]: '\0');
        public int Length => _Text.Length;
        public string FileName { get; }

        public SourceText(string text, string fileName_)
        {
            _Text = text;
            Lines = parseLines(this, text);
            FileName = fileName_;
        }

        // ///////////////////////////////////////////////////////////////////////////
        public static ImmutableArray<TextLine> parseLines(SourceText sourceText, string text)
        {
            ImmutableArray<TextLine>.Builder ret;
            TextLine line;
            int lineStart, lineBreakWdith;
            int pos;

            ret = ImmutableArray.CreateBuilder<TextLine>();
            lineStart = 0;
            pos = 0;

            while (pos < text.Length)
            {
                lineBreakWdith = GetLineWithLineBreak(text, pos);

                if (lineBreakWdith == 0)
                    pos += 1;
                else
                {
                    addLine(ret, sourceText, pos, lineStart, lineBreakWdith);
                    pos += lineBreakWdith;
                    lineStart = pos;
                }
            }

            if (pos > lineStart)
                addLine(ret, sourceText, pos, lineStart, 0);

            return ret.ToImmutable();
        }

        // ///////////////////////////////////////////////////////////////////////////
        public static SourceText FROM(string text, string filename="")
        {
            return new SourceText(text, filename);
        }

        // ///////////////////////////////////////////////////////////////////////////
        private static void addLine(ImmutableArray<TextLine>.Builder output, SourceText sourceText, int pos, int lineStart, int lineBreakWdith)
        {
            TextLine ret;
            ret = new TextLine(sourceText,
                                lineStart,
                                (pos - lineStart),
                                (pos + lineBreakWdith) - lineStart);
            output.Add(ret);
        }

        // ///////////////////////////////////////////////////////////////////////////
        private static int GetLineWithLineBreak(string Text, int pos)
        {
            char curChar;
            char peek;

            curChar = Text[pos];
            peek = (((pos + 1) >= Text.Length) ? '\0' : Text[pos + 1]);

            if (curChar == '\n' && peek == '\r')
                return 2;
            if (curChar == '\r' && peek == '\n')
                return 2;
            if (curChar == '\n' || curChar == '\r')
                return 1;

            return 0;
        }

        // ///////////////////////////////////////////////////////////////////////////
        public static SourceText from(string text)
        {
            return new SourceText(text, "");
        }

        // ///////////////////////////////////////////////////////////////////////////
        public string ToString(TextSpan span)
        {
            int start, len, tot;

            start = span.START;
            len = Math.Abs(span.LENGTH);
            tot = len + start;

            if (tot > _Text.Length)
                return "";

            return _Text.Substring(start, len);
        }

        // ///////////////////////////////////////////////////////////////////////////
        public string ToString(int start)
        {
            // return empty string when outside the range of the string
            if (start < 0 || start > _Text.Length)
                return "";

            return _Text.Substring(start);
        }

        // ///////////////////////////////////////////////////////////////////////////
        public int getLineIndex(int pos)
        {
            int max, mid, min, diff, failsafe;

            max = Lines.Length;
            min = 0;
            mid = max >> 1;
            failsafe = 0;

            while (true)
            {
                // check symbol
                if (Lines[mid].Start == pos)
                    return mid;

                // set new range
                if (Lines[mid].Start < pos)
                    min = mid;
                else
                    max = mid;

                // compute next symbol
                diff = (max - min);
                mid = diff >> 1;
                mid += min;

                // exit symbol not found
                if (failsafe > Lines.Length)
                    break;
                failsafe += 1;
            }

            return 0;
        }
    }
}
