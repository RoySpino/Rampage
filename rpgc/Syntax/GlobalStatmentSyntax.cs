﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed partial class GlobalStatmentSyntax : MemberSyntax
    {
        public override TokenKind kind => TokenKind.TK_GLBSTMNT;
        public StatementSyntax Statement { get; }
        public GlobalStatmentSyntax(SyntaxTree stree, StatementSyntax _statement) :base(stree)
        {
            Statement = _statement;
        }
    }
}
