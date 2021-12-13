using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rpgc.Syntax;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal sealed class BoundUniOperator
    {
        public TokenKind SyntaxKind { get; }
        public BoundUniOpToken tok { get; }
        public TypeSymbol OperatorType, ResultType;
        private static BoundUniOperator[] OPERATORS = {
            new BoundUniOperator(TokenKind.TK_NOT, BoundUniOpToken.BUO_NOT, TypeSymbol.Indicator),
            new BoundUniOperator(TokenKind.TK_ADD, BoundUniOpToken.BUO_IDENTITY, TypeSymbol.Integer),
            new BoundUniOperator(TokenKind.TK_SUB, BoundUniOpToken.BUO_NEGATION, TypeSymbol.Integer)
        };

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundUniOperator(TokenKind syntaxKind, BoundUniOpToken op, TypeSymbol operatorType)
        {
            SyntaxKind = syntaxKind;
            tok = op;
            OperatorType = operatorType;
            ResultType = operatorType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundUniOperator(TokenKind syntaxKind, BoundUniOpToken op, TypeSymbol operatorType, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            tok = op;
            OperatorType = operatorType;
            ResultType = resultType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public static BoundUniOperator bind(TokenKind kind, TypeSymbol operandType)
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