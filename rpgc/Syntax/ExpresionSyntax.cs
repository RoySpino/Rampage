﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public abstract class ExpresionSyntax : SyntaxNode
    {
        public ExpresionSyntax(SyntaxTree stree)
            :base(stree)
        { }
    }
}
