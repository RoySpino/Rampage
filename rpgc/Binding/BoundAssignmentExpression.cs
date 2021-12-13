using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_ASNEX;
        public override TypeSymbol Type => Expression.Type;
        public BoundExpression Expression { get; }
        public VariableSymbol Variable { get; }

        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            Expression = expression;
            Variable = variable;
        }

        public BoundAssignmentExpression(BoundAssignmentExpression assin)
        {
            Expression = assin.Expression;
            Variable = assin.Variable;
        }
    }
}
