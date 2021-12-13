using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    sealed class ForStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_FOR;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpresionSyntax LowerBound { get; }
        public SyntaxToken Keyword_To { get; }
        public ExpresionSyntax UpperBound { get; }
        public SyntaxToken Keyword_By { get; }
        public ExpresionSyntax Increment { get; }
        public StatementSyntax Body { get; }

        public ForStatementSyntax(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsTok, ExpresionSyntax lowerBound, SyntaxToken keyword_To, ExpresionSyntax upperBound, StatementSyntax body, SyntaxToken keyword_By = null, ExpresionSyntax inc = null)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsTok;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Keyword_To = keyword_To;
            Body = body;

            if (keyword_By == null)
                Keyword_By = new SyntaxToken(TokenKind.TK_TO, 0, 0, "TO");
            else
                Keyword_To = keyword_By;

            if (inc == null)
                Increment = new LiteralExpressionSyntax(new SyntaxToken(TokenKind.TK_INTEGER, 0, 0, "1"));
            else
                Increment = inc;
        }
    }
}
