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
        public TextSpan(int start, int length)
        {
            START = start;
            LENGTH = length;
            END = START + LENGTH;
        }
    }
}
