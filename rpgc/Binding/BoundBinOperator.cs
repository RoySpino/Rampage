using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundBinOperator
    {
        public TokenKind SyntaxKind;
        public BoundBinOpToken tok;
        public Type RightType, LeftType, ResultType;
        private static BoundBinOperator[] OPERATORS = {
            new BoundBinOperator(TokenKind.TK_ADD, BoundBinOpToken.BBO_ADD, typeof(int)),
            new BoundBinOperator(TokenKind.TK_SUB, BoundBinOpToken.BBO_SUB, typeof(int)),
            new BoundBinOperator(TokenKind.TK_MULT, BoundBinOpToken.BBO_MULT, typeof(int)),
            new BoundBinOperator(TokenKind.TK_DIV, BoundBinOpToken.BBO_DIV, typeof(int)),

            new BoundBinOperator(TokenKind.TK_AND, BoundBinOpToken.BBO_AND, typeof(bool)),
            new BoundBinOperator(TokenKind.TK_OR, BoundBinOpToken.BBO_OR, typeof(bool)),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, typeof(bool)),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, typeof(bool)),
            new BoundBinOperator(TokenKind.TK_GE, BoundBinOpToken.BBO_GE, typeof(int), typeof(bool)),
            new BoundBinOperator(TokenKind.TK_GT, BoundBinOpToken.BBO_GT, typeof(int), typeof(bool)),
            new BoundBinOperator(TokenKind.TK_LE, BoundBinOpToken.BBO_LE, typeof(int), typeof(bool)),
            new BoundBinOperator(TokenKind.TK_LT, BoundBinOpToken.BBO_LT, typeof(int), typeof(bool)),
            new BoundBinOperator(TokenKind.TK_EQ, BoundBinOpToken.BBO_EQ, typeof(int), typeof(bool)),
            new BoundBinOperator(TokenKind.TK_NE, BoundBinOpToken.BBO_NE, typeof(int), typeof(bool))
        };

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundBinOperator(TokenKind syntaxKind, BoundBinOpToken kind, Type type)
        {
            SyntaxKind = syntaxKind;
            tok = kind;
            LeftType = type;
            RightType = type;
            ResultType = type;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundBinOperator(TokenKind syntaxKind, BoundBinOpToken kind, Type type, Type resultType)
        {
            SyntaxKind = syntaxKind;
            tok = kind;
            LeftType = type;
            RightType = type;
            ResultType = resultType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundBinOperator(TokenKind syntaxKind, BoundBinOpToken kind, Type leftType, Type rightType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            tok = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public static BoundBinOperator bind(TokenKind kind, Type Ltype, Type Rtype)
        {
            foreach (BoundBinOperator op in OPERATORS)
            {
                if (op.SyntaxKind == kind && op.LeftType == Ltype && op.RightType == Rtype)
                    return op;
            }

            return null;
        }
    }
}
