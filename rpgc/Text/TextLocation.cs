using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Text
{
    public sealed class TextLocation
    {
        public SourceText TEXT { get; }
        public TextSpan SPAN { get; }

        public TextLocation(SourceText text, TextSpan span)
        {
            TEXT = text;
            SPAN = span;
        }

        // ////////////////////////////////////////////////////////////////////
        public int startLine()
        {
            return SPAN.LineNo;
        }

        // ////////////////////////////////////////////////////////////////////
        public int endLine()
        {
            return TEXT.getLineIndex(SPAN.END);
        }

        // ////////////////////////////////////////////////////////////////////
        public int startCharacter()
        {
            return SPAN.START - TEXT.Lines[SPAN.LineNo].Start;
        }

        // ////////////////////////////////////////////////////////////////////
        public int endCharacter()
        {
            return SPAN.END - TEXT.Lines[SPAN.LineNo].End;
        }
    }
}
