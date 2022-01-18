using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public abstract class StatementSyntax :SyntaxNode
    {
        public StatementSyntax(SyntaxTree stree)
            : base(stree)
        {
            //
        }
    }
}
