using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc.Syntax
{
    public sealed class VariableDeclarationSyntax : StatementSyntax
    {
        // D<varNmae>        S             10  0 inz(0)
        // Dcl-s <varNmae> zoned(10: 0) inz(0);
        public override TokenKind kind => TokenKind.TK_VARDECLR;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeSymbol Typ { get; }
        public SyntaxToken InitKeyWord { get; }
        public ExpresionSyntax Initilizer { get; }
        public TypeClauseSyntax TypClas { get; }

        // temporary vars
        private SyntaxToken inz;
        private ExpresionSyntax inzVal;

        public VariableDeclarationSyntax(SyntaxTree stree, SyntaxToken keyword, SyntaxToken identifier, TypeSymbol typ, SyntaxToken initKeyWord = null, ExpresionSyntax initilize = null)
            : base(stree)
        {
            Keyword = keyword;
            Identifier = identifier;
            Typ = typ;

            // setup initial values
            setupInit(Typ, initKeyWord, initilize);
            InitKeyWord = inz;
            Initilizer = inzVal;

        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constant deffinition
        public VariableDeclarationSyntax(SyntaxTree stree,SyntaxToken keyword, SyntaxToken identifier, ExpresionSyntax initilize)
            : base(stree)
        {
            Keyword = keyword;
            Identifier = identifier;
            Typ = null;

            // setup initial values
            InitKeyWord = null;
            Initilizer = initilize;

        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Dcl-s _NAME_ zoned inz( _VAL_ )
        public VariableDeclarationSyntax(SyntaxTree stree, SyntaxToken keyworkd, SyntaxToken identifier, TypeClauseSyntax typClas, SyntaxToken initKeyWord = null, ExpresionSyntax initilize = null)
            : base(stree)
        {
            Keyword = keyworkd;
            Identifier = identifier;
            TypClas = typClas;
            Typ = SyntaxFacts.lookupType(typClas.Identifier.sym.ToString());

            // setup initial values
            setupInit(Typ, initKeyWord, initilize);
            InitKeyWord = inz;
            Initilizer = inzVal;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void setupInit(TypeSymbol typ, SyntaxToken initKeyWord, ExpresionSyntax initilizer)
        {

            if (typ != TypeSymbol.ERROR)
            {
                if (initKeyWord != null)
                {
                    inz = initKeyWord;
                    inzVal = initilizer;
                }
                else
                {
                    inz = new SyntaxToken(STREE, TokenKind.TK_INZ, 0, 0, "INZ");

                    switch (typ.Name)
                    {
                        case "IND":
                            inzVal = new LiteralExpressionSyntax(STREE, new SyntaxToken(STREE, TokenKind.TK_INDON, 0, 0, true));
                            break;
                        case "INT(10)":
                        case "ZONED":
                        case "PACKED":
                        case "FLOAT":
                            inzVal = new LiteralExpressionSyntax(STREE, new SyntaxToken(STREE, TokenKind.TK_INTEGER, 0, 0, 0));
                            break;
                        case "DATE":
                        case "TIMESTAMP":
                        case "TIME":
                            inzVal = new LiteralExpressionSyntax(STREE, new SyntaxToken(STREE, TokenKind.TK_DATETIME, 0, 0, DateTime.Now));
                            break;
                        case "STRING":
                        case "CHAR":
                        case "VARCHAR":
                        default:
                            inzVal = new LiteralExpressionSyntax(STREE, new SyntaxToken(STREE, TokenKind.TK_DATETIME, 0, 0, ""));
                            break;
                    }
                }
            }
            else
            {
                inzVal = new LiteralExpressionSyntax(STREE, new SyntaxToken(STREE, TokenKind.TK_INDON, 0, 0, true));
            }
        }
    }
}
