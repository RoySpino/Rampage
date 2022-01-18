using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Text;

namespace rpgc
{
    public sealed class Diagnostics
    {
        public TextSpan SPAN { get; }
        public string MESSAGE { get; }
        public bool IsWarning { get; set; }
        public TextLocation Location { get; }

        public Diagnostics(TextLocation txtLoc, string message)
        {
            SPAN = txtLoc.SPAN;
            MESSAGE = message;
            IsWarning = false;
            Location = txtLoc;
        }
        public Diagnostics(TextLocation txtLoc, TextSpan span_, string message)
        {
            SPAN = span_;
            MESSAGE = message;
            IsWarning = false;
            Location = txtLoc;
        }

        public override string ToString()
        {
            return MESSAGE;
        }
    }
}
