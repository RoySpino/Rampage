using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public struct TextSpan
    {
        public int START, LENGTH, END;
        public int LineNo, LinePos;

        public TextSpan(int start, int length, int ln = 0, int lp = 0)
        {
            START = start;
            LENGTH = length;
            END = START + LENGTH;
            LineNo = ln;
            LinePos = lp;
        }

        // //////////////////////////////////////////////////////////////////
        public static TextSpan fromBounds(int start, int end, int ln = 0, int lp = 0)
        {
            int len;

            len = Math.Abs(end - start);

            return new TextSpan(start, len, ln, lp);
        }
    }
}
