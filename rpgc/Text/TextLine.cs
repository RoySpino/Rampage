using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Text
{
    public sealed class TextLine
    {
        public SourceText Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public int LengthWithLineBreak { get; }
        public TextSpan Span => new TextSpan(Start, Length);

        public TextSpan SpanWithLineBreak => new TextSpan(Start, LengthWithLineBreak);
        public override string ToString() => Text.ToString(Span);

        public TextLine(SourceText text, int start, int length, int lengthWithLineBreak)
        {
            Text = text;
            Start = start;
            Length = length;
            LengthWithLineBreak = lengthWithLineBreak;
        }
    }
}
