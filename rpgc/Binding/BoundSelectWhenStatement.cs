using rpgc.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace rpgc.Binding
{
    internal class BoundSelectWhenStatement : BoundStatement
    {
        public ImmutableArray<BoundExpression> BoundExpressions { get; }
        public ImmutableArray<BoundStatement> BoundStatements { get; }
        public BoundStatement DefualtStatements { get; }
        public override BoundNodeToken tok => BoundNodeToken.BNT_SELECTSTMT;


        public BoundSelectWhenStatement(ImmutableArray<BoundExpression> boundExpressions, ImmutableArray<BoundStatement> boundStatements, BoundStatement defaultStatement = null)
        {
            BoundExpressions = boundExpressions;
            BoundStatements = boundStatements;
            DefualtStatements = defaultStatement;
        }
    }
}