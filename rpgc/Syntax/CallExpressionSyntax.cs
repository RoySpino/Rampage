using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class CallExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_CALL;
        public SyntaxToken FunctionName { get; }
        public SyntaxToken OpenParen { get; }
        public SeperatedSyntaxList<ExpresionSyntax> Arguments{get; }
        public SyntaxToken CloseParen { get; }
        public bool isExsrCall { get; }

        public CallExpressionSyntax(SyntaxTree stree, SyntaxToken functionName, SyntaxToken openParen, SeperatedSyntaxList<ExpresionSyntax> args, SyntaxToken closeParen, bool onExsrCall = false)
            : base(stree)
        {
            FunctionName = functionName;
            Arguments = args;
            OpenParen = openParen;
            CloseParen = closeParen;
            isExsrCall = onExsrCall;
        }
    }






}
