using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    class boundVariableDeclaration : BoundStatement
    {
        public VariableSymbol Variable { get; }
        public BoundExpression Initalizer { get; }

        public override BoundNodeToken tok => BoundNodeToken.BNT_VARDECLR;

        public boundVariableDeclaration(VariableSymbol variable, BoundExpression initalizer)
        {
            Variable = variable;
            Initalizer = initalizer;
        }
    }
}
