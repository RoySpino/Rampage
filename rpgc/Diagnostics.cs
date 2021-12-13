using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public sealed class Diagnostics
    {
        public TextSpan SPAN { get; }
        public string MESSAGE { get; }
        public bool IsWarning { get; set; }

        public Diagnostics(TextSpan span, string message)
        {
            SPAN = span;
            MESSAGE = message;
            IsWarning = false;
        }

        public override string ToString()
        {
            return MESSAGE;
        }
    }
}
