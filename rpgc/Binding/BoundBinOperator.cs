using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rpgc.Syntax;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal sealed class BoundBinOperator
    {
        public TokenKind SyntaxKind { get; }
        public BoundBinOpToken tok { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol ResultType { get; }

        private static BoundBinOperator[] OPERATORS = {
            // Mathmatic Operators Integers
            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, TypeSymbol.Integer),
            new BoundBinOperator(TokenKind.TK_SUB, BoundBinOpToken.BBO_SUB, TypeSymbol.Integer),
            new BoundBinOperator(TokenKind.TK_MULT, BoundBinOpToken.BBO_MULT, TypeSymbol.Integer),
            new BoundBinOperator(TokenKind.TK_DIV, BoundBinOpToken.BBO_DIV, TypeSymbol.Integer),

            // Mathmatic Operators FLOATS
            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_SUB, BoundBinOpToken.BBO_SUB, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_MULT, BoundBinOpToken.BBO_MULT, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_DIV, BoundBinOpToken.BBO_DIV, TypeSymbol.Float),

            // Mathmatic Operator Mix
            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_SUB, BoundBinOpToken.BBO_SUB, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_MULT, BoundBinOpToken.BBO_MULT, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_DIV, BoundBinOpToken.BBO_DIV, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Float),

            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_SUB, BoundBinOpToken.BBO_SUB, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_MULT, BoundBinOpToken.BBO_MULT, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Float),
            new BoundBinOperator(TokenKind.TK_DIV, BoundBinOpToken.BBO_DIV, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Float),

            // Logical Operators
            new BoundBinOperator(TokenKind.TK_AND, BoundBinOpToken.BBO_AND, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_OR, BoundBinOpToken.BBO_OR, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, TypeSymbol.Indicator),

            // logical comparison numerics FLOAT, INTEGERS
            new BoundBinOperator(TokenKind.TK_GE, BoundBinOpToken.BBO_GE, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_GT, BoundBinOpToken.BBO_GT, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LE, BoundBinOpToken.BBO_LE, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LT, BoundBinOpToken.BBO_LT, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, TypeSymbol.Integer, TypeSymbol.Indicator),

            new BoundBinOperator(TokenKind.TK_GE, BoundBinOpToken.BBO_GE, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_GT, BoundBinOpToken.BBO_GT, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LE, BoundBinOpToken.BBO_LE, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LT, BoundBinOpToken.BBO_LT, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, TypeSymbol.Integer, TypeSymbol.Float, TypeSymbol.Indicator),

            new BoundBinOperator(TokenKind.TK_GE, BoundBinOpToken.BBO_GE, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_GT, BoundBinOpToken.BBO_GT, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LE, BoundBinOpToken.BBO_LE, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LT, BoundBinOpToken.BBO_LT, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, TypeSymbol.Float, TypeSymbol.Integer, TypeSymbol.Indicator),

            new BoundBinOperator(TokenKind.TK_GE, BoundBinOpToken.BBO_GE, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_GT, BoundBinOpToken.BBO_GT, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LE, BoundBinOpToken.BBO_LE, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_LT, BoundBinOpToken.BBO_LT, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, TypeSymbol.Float, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, TypeSymbol.Float, TypeSymbol.Indicator),

            // string Eqals
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, TypeSymbol.Char, TypeSymbol.Indicator),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, TypeSymbol.varchar, TypeSymbol.Indicator),

            // string concat
            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, TypeSymbol.Char),
            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, TypeSymbol.varchar)
        };

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundBinOperator(TokenKind syntaxKind, BoundBinOpToken kind, TypeSymbol type)
        {
            SyntaxKind = syntaxKind;
            tok = kind;
            LeftType = type;
            RightType = type;
            ResultType = type;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundBinOperator(TokenKind syntaxKind, BoundBinOpToken kind, TypeSymbol type, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            tok = kind;
            LeftType = type;
            RightType = type;
            ResultType = resultType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundBinOperator(TokenKind syntaxKind, BoundBinOpToken kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            tok = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public static BoundBinOperator bind(TokenKind kind, TypeSymbol Ltype, TypeSymbol Rtype)
        {
            BoundBinOperator ret;

            ret = (from op in OPERATORS
                   where op.SyntaxKind == kind && op.LeftType == Ltype && op.RightType == Rtype
                   select op).FirstOrDefault();

            return ret;
        }
    }
}
