using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class SyntaxToken : SyntaxNode
    {
        public override TokenKind kind { get; }
        public int line { get; }
        public int pos { get; }
        public object sym { get; }
        public override TextSpan span => new TextSpan(pos, (sym?.ToString().Length ?? 0), line, linePosition);
        public string special;
        public int linePosition;


        public SyntaxToken(SyntaxTree sTree,TokenKind k, int l, int p, object s, int linPos = 0)
            :base(sTree)
        {
            kind = k;
            line = l;
            pos = p;
            sym = s;
            linePosition = linPos;
        }
        
        // ////////////////////////////////////////////////////////
        public SyntaxToken(SyntaxTree sTree, TokenKind k, int l, int p, object s, string specl, int linPos=0)
            : base(sTree)
        {
            kind = k;
            line = l;
            pos = p;
            sym = s;
            special = specl;
            linePosition = linPos;
        }

        // ////////////////////////////////////////////////////////
        internal bool isMissing()
        {
            return (sym == null);
        }
    }

    // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public enum TokenKind
    {
        TK_ACQ,
        TK_ADD,
        TK_ADDDUR,
        TK_ALLOC,
        TK_AND,
        TK_ASSIGN,
        TK_BADTOKEN,
        TK_BEGSR,
        TK_BITOFF,
        TK_BITON,
        TK_BLOCKEND,
        TK_BLOCKSTART,
        TK_BLOCKSYNTX,
        TK_BY,
        TK_BYNARYEXPR,
        TK_CAB,
        TK_CALL,
        TK_CALLB,
        TK_CALLP,
        TK_CAS,
        TK_CAT,
        TK_CHAIN,
        TK_CHECK,
        TK_CHECKR,
        TK_CLEAR,
        TK_CLOSE,
        TK_COLON,
        TK_COMMIT,
        TK_COMP,
        TK_COMPLATIONUNT,
        TK_DATE,
        TK_DATETIME,
        TK_DEALLOC,
        TK_DEFINE,
        TK_DELETE,
        TK_DIV,
        TK_DO,
        TK_DOEND,
        TK_DOU,
        TK_DOW,
        TK_DOWNTO,
        TK_DSPLY,
        TK_DUMP,
        TK_ELSE,
        TK_ENDCS,
        TK_ENDDO,
        TK_ENDIF,
        TK_ENDFOR,
        TK_ENDMON,
        TK_ENDPROC,
        TK_ENDSR,
        TK_ENDSL,
        TK_EOI,
        TK_EQ,
        TK_EVAL,
        TK_EVALR,
        TK_EXCEPT,
        TK_EXFMT,
        TK_EXPONENT,
        TK_EXPRNSTMNT,
        TK_EXSR,
        TK_EXTRCT,
        TK_FEOD,
        TK_FLOAT,
        TK_FOR,
        TK_FORCE,
        TK_GE,
        TK_GOTO,
        TK_GRAPHIC,
        TK_GT,
        TK_IDENTIFIER,
        TK_IF,
        TK_IFEND,
        TK_IN,
        TK_INDOFF,
        TK_INDON,
        TK_INTEGER,
        TK_INZ,
        TK_ITER,
        TK_KFLD,
        TK_KLIST,
        TK_LEAVE,
        TK_LEAVESR,
        TK_LOOKUP,
        TK_LE,
        TK_LITEXPR,
        TK_LT,
        TK_MHHZO,
        TK_MHLZO,
        TK_MLHZO,
        TK_MLLZO,
        TK_MONITOR,
        TK_MOVE,
        TK_MOVEA,
        TK_MOVEL,
        TK_MULT,
        TK_MVR,
        TK_NAMEDEXP,
        TK_NE,
        TK_NEXT,
        TK_NONE,
        TK_NOT,
        TK_OCCUR,
        TK_OPEN,
        TK_OR,
        TK_OTHER,
        TK_OUT,
        TK_PACKED,
        TK_PARENEXP,
        TK_PARENCLOSE,
        TK_PARENOPEN,
        TK_PARM,
        TK_PLIST,
        TK_POST,
        TK_PROCDCL,
        TK_PROCEND,
        TK_READ,
        TK_READC,
        TK_READE,
        TK_READP,
        TK_READPE,
        TK_REALLOC,
        TK_REL,
        TK_RESET,
        TK_RETURN,
        TK_ROLBK,
        TK_SCAN,
        TK_SELECT,
        TK_SEMI,
        TK_SETGT,
        TK_SETLL,
        TK_SETOFF,
        TK_SETON,
        TK_SHTDN,
        TK_SORTA,
        TK_SPACE,
        TK_SQRT,
        TK_STRING,
        TK_SUB,
        TK_SUBDUR,
        TK_SUBST,
        TK_SUBRTNDCL,
        TK_SUBRTNEND,
        TK_TAG,
        TK_TEST,
        TK_TESTB,
        TK_TESTN,
        TK_TESTZ,
        TK_TIME,
        TK_TO,
        TK_UNIOP,
        TK_UNIEXP,
        TK_UNLOCK,
        TK_UPDATE,
        TK_VARDCONST,
        TK_VARDDATAS,
        TK_VARDECLR,
        TK_WHEN,
        TK_WRITE,
        TK_XFOOT,
        TK_XLATE,
        TK_ZADD,
        TK_ZONED,
        TK_ZSUB,
        TK_INDICATOR,
        TK_TIMESTAMP,
        TK_TYPCLAUSE,
        TK_GLBSTMNT,
        TK_PRCKKEYWRD,
        TK_PROCINFC,
        TK_ENDPI,
        TK_NEWLINE,
        TK_ENDPR,
        TK_ENDDS
    }
}
