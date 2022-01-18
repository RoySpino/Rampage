using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class ErrorMemberSyntax : MemberSyntax
    {
        public override TokenKind kind => TokenKind.TK_BADTOKEN;

        public ErrorMemberSyntax(SyntaxTree st) : base(st)
        {
            //
        }
    }
}
