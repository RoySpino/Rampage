using System;
using System.Collections;

namespace rpgc.Syntax
{
    public sealed class ParamiterSyntax : SyntaxNode
    {
        public override TokenKind kind => TokenKind.TK_PARM;
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }

        public ParamiterSyntax(SyntaxTree stree,SyntaxToken ident, TypeClauseSyntax typ)
            :base(stree)
        {
            Identifier = ident;
            Type = typ;
        }
    }
}