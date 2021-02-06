using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundUniOperator
    {
        public TokenKind SyntaxKind;
        public BoundUniOpToken tok;
        public Type OperatorType, ResultType;
        private static BoundUniOperator[] OPERATORS = {
            new BoundUniOperator(TokenKind.TK_NOT, BoundUniOpToken.BUO_NOT, typeof(bool)),
            new BoundUniOperator(TokenKind.TK_ADD, BoundUniOpToken.BUO_IDENTITY, typeof(int)),
            new BoundUniOperator(TokenKind.TK_SUB, BoundUniOpToken.BUO_NEGATION, typeof(int))
        };

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundUniOperator(TokenKind syntaxKind, BoundUniOpToken op, Type operatorType)
        {
            SyntaxKind = syntaxKind;
            tok = op;
            OperatorType = operatorType;
            ResultType = operatorType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundUniOperator(TokenKind syntaxKind, BoundUniOpToken op, Type operatorType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            tok = op;
            OperatorType = operatorType;
            ResultType = resultType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public static BoundUniOperator bind(TokenKind kind, Type operandType)
        {
            foreach (BoundUniOperator op in OPERATORS)
            {
                if (op.OperatorType == operandType && op.SyntaxKind == kind)
                    return op;
            }

            return null;
        }
    }
}
