using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Text
{
    internal class TextSpanCompare : IComparable<TextSpan>
    {
        public int Compare(TextSpan A, TextSpan B)
        {
            int comp;
            comp = A.START - B.START;

            if (comp == 0)
                comp = A.LENGTH - B.LENGTH;

            return comp;
        }

        public int CompareTo(TextSpan other)
        {
            throw new NotImplementedException();
        }
    }
}
