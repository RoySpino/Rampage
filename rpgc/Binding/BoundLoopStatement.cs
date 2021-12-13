using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        public BoundLabel BreakLbl { get; }
        public BoundLabel ContinueLbl { get; }

        public BoundLoopStatement(BoundLabel breakLbl, BoundLabel continueLbl)
        {
            BreakLbl = breakLbl;
            ContinueLbl = continueLbl;
        }
    }
}
