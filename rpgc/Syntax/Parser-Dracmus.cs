using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Syntax;
using rpgc.Text;

namespace rpgc.Syntax
{
    internal sealed class Parser
    {
        Lexer lex = null;
        SyntaxToken tok, current;
        int pos, tCount;
        DiagnosticBag diagnostics = new DiagnosticBag();
        public readonly ImmutableArray<SyntaxToken> tokens;
        public readonly SourceText source;

        public Parser(SourceText txt)
        {
            List<SyntaxToken> _tokens = new List<SyntaxToken>();
            lex = new Lexer(txt);
            pos = 0;
            source = txt;

            // get tokens
            do
            {
                // get token
                tok = lex.doLex();

                // special case for else add block end before else
                if (tok.kind == TokenKind.TK_ELSE)
                    _tokens.Add(new SyntaxToken(TokenKind.TK_BLOCKEND, tok.line, tok.pos, ""));

                // exit on error
                if (tok.kind == TokenKind.TK_BADTOKEN)
                {
                    _tokens.Add(tok);
                    break;
                }

                // save avalable tokens
                if (tok != null && tok.kind != TokenKind.TK_SPACE)
                    _tokens.Add(tok);
            }
            while (tok.kind != TokenKind.TK_EOI);

            tokens = _tokens.ToImmutableArray();
            diagnostics.AddRange(lex.getDiagnostics());
            tCount = _tokens.Count;

            if (tokens.Count() > 0)
                current = tokens[0];
        }

        // ///////////////////////////////////////////////////////////////////////
        SyntaxToken nextToken()
        {
            SyntaxToken ret;

            if (pos >= tCount)
                return null;
            else
            {
                ret = tokens[pos];
                pos += 1;
                current = tokens[((pos >= tCount) ? tCount - 1 : pos)];
                return ret;
            }
        }

        // ///////////////////////////////////////////////////////////////////////
        SyntaxToken peek(int offset)
        {
            int index;

            index = pos + offset;

            if (index >= tCount)
                return tokens[tCount - 1];

            return tokens[index];
        }

        // ///////////////////////////////////////////////////////////////////////
        private SyntaxToken match(TokenKind tok)
        {
            if (current.kind == tok)
                return nextToken();

            //addError(string.Format("Unreconized token [{2}] at ({0},{1})", current.line, current.pos, current.sym));
            diagnostics.reportUnexpectedToken(current.span, current.kind, tok);
            return new SyntaxToken(current.kind, current.line, current.pos, current.sym);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseBinaryExpresion(int parentPrecedence = 0)
        {
            int precedence, uniPrecedence;
            ExpresionSyntax left, right, operand;
            SyntaxToken operatorToken;

            uniPrecedence = 0;
            uniPrecedence = current.kind.getUinaryOporatorPrecedence();

            // account for nagative(-) symbol before numbers
            if (uniPrecedence > parentPrecedence)
            {
                operatorToken = nextToken();
                operand = parsePrimaryExpression();
                left = new UinaryExpressionSyntax(operatorToken, operand);
            }
            else
                left = parsePrimaryExpression();

            // regulare numerical expressions
            while (true)
            {
                precedence = current.kind.getBinaryOporatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                operatorToken = nextToken();
                right = parseBinaryExpresion(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parceExpression()
        {
            return parceAssignmentExpression();
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parceAssignmentExpression()
        {
            ExpresionSyntax right;
            SyntaxToken identifierToken, operatorToken;

            if (peek(0).kind == TokenKind.TK_IDENTIFIER && peek(1).kind == TokenKind.TK_ASSIGN)
            {
                identifierToken = nextToken();
                operatorToken = nextToken();
                right = parceAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }

            return parseBinaryExpresion();
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parsePrimaryExpression()
        {
            // handle parethesies
            switch (current.kind)
            {
                case TokenKind.TK_PARENOPEN:
                    return parseTokenParethesizedExpression();
                case TokenKind.TK_INDON:
                case TokenKind.TK_INDOFF:
                    return parseTokenBoolenLiteral();
                case TokenKind.TK_INTEGER:
                case TokenKind.TK_ZONED:
                case TokenKind.TK_PACKED:
                    return parseNumberLiteral();
                case TokenKind.TK_IDENTIFIER:
                default:
                    return parseTokenNamedExpression();
            }

        }
        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseTokenNamedExpression()
        {
            SyntaxToken identifier;
            //identifierToken = nextToken();
            identifier = match(TokenKind.TK_IDENTIFIER);
            return new NamedExpressionSyntax(identifier);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseTokenBoolenLiteral()
        {
            SyntaxToken keyworkdTok;
            bool bolValue;

            bolValue = (current.kind == TokenKind.TK_INDON);
            keyworkdTok = (bolValue ? match(TokenKind.TK_INDON) : match(TokenKind.TK_INDOFF));
            return new LiteralExpressionSyntax(keyworkdTok, bolValue);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseTokenParethesizedExpression()
        {
            SyntaxToken left, righ;
            ExpresionSyntax exp;

            left = match(TokenKind.TK_PARENOPEN);
            exp = parseBinaryExpresion();
            righ = match(TokenKind.TK_PARENCLOSE);
            return new ParenthesizedExpression(left, exp, righ);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseNumberLiteral()
        {
            SyntaxToken numberToken;
            numberToken = match(TokenKind.TK_INTEGER);
            return new LiteralExpressionSyntax(numberToken);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseStaement()
        {
            switch (current.kind)
            {
                case TokenKind.TK_BLOCKSTART:
                    return parseBlockStaement();
                case TokenKind.TK_IF:
                    return parseIfStaement();
                case TokenKind.TK_VARDCONST:
                case TokenKind.TK_VARDECLR:
                    return parseVariableDeclaration();
                default:
                    return parseExpressionStaement();
            }
        }

        // /////////////////////////////////////////////////////////////////////////
        private IfStatementSyntax parseIfStaement()
        {
            SyntaxToken keyword;
            ExpresionSyntax condition;
            StatementSyntax statement;
            ElseStatementSyntax elseClause;

            keyword = match(TokenKind.TK_IF);
            condition = parceExpression();
            statement = parseStaement();
            elseClause = parseElseStaement();

            return new IfStatementSyntax(keyword, condition, statement, elseClause);
        }

        // /////////////////////////////////////////////////////////////////////////
        private ElseStatementSyntax parseElseStaement()
        {
            SyntaxToken keyword;
            StatementSyntax statement;

            if (current.kind != TokenKind.TK_ELSE)
                return null;

            keyword = nextToken();
            statement = parseStaement();
            
            return new ElseStatementSyntax(keyword, statement);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseVariableDeclaration()
        {
            TokenKind expected;
            SyntaxToken keyworkd, identifier, initKeyWord;
            ExpresionSyntax initilize;

            switch (current.kind)
            {
                case TokenKind.TK_VARDDATAS:
                case TokenKind.TK_VARDCONST:
                    expected = current.kind;
                    break;
                default:
                    expected = TokenKind.TK_VARDECLR;
                    break;
            }

            keyworkd = match(expected);
            identifier = match(TokenKind.TK_IDENTIFIER);

            // add varialbe initilizer
            if (peek(0).kind == TokenKind.TK_INZ)
            {
                nextToken();
                initKeyWord = match(TokenKind.TK_INZ);
                initilize = parceExpression();
            }
            else
            {
                initKeyWord = null;
                initilize = null;
            }

            return new VariableDeclarationSyntax(keyworkd, identifier, typeof(Int32), initKeyWord, initilize);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseBlockStaement()
        {
            ImmutableArray<StatementSyntax>.Builder statements;
            SyntaxToken openStatementToken;
            SyntaxToken closeStatementToken;
            StatementSyntax statmt;

            statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            openStatementToken = match(TokenKind.TK_BLOCKSTART);

            while (current.kind != TokenKind.TK_EOI && 
                   current.kind != TokenKind.TK_BLOCKEND)
            {
                statmt = parseStaement();
                statements.Add(statmt);
            }

            closeStatementToken = match(TokenKind.TK_BLOCKEND);

            return new BlockStatementSyntax(openStatementToken, statements.ToImmutable(), closeStatementToken);
        }

        // /////////////////////////////////////////////////////////////////////////
        private ExpressionStatementSyntax parseExpressionStaement()
        {
            ExpresionSyntax expression;
            expression = parceExpression();
            return new ExpressionStatementSyntax(expression);
        }

        // ///////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }

        // ///////////////////////////////////////////////////////////////////////
        public CompilationUnit parseCompilationUnit()
        {
            StatementSyntax ret;
            SyntaxToken tmp;

            ret = parseStaement();

            tmp = match(TokenKind.TK_EOI);

            return new CompilationUnit(ret, tmp);
        }
    }
}
