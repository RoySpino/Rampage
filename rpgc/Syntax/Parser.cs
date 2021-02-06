using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Syntax;

namespace rpgc
{
    internal sealed class Parser
    {
        Lexer lex = null;
        SyntaxToken tok, current;
        List<SyntaxToken> tokens = new List<SyntaxToken>();
        int pos, tCount;
        DiagnosticBag diagnostics = new DiagnosticBag();

        public Parser(string txt)
        {
            lex = new Lexer(txt);
            pos = 0;

            // get tokens
            do
            {
                // get token
                tok = lex.doLex();

                // exit on error
                if (tok.tok == TokenKind.TK_BADTOKEN)
                {
                    tokens.Add(tok);
                    break;
                }

                // save avalable tokens
                if (tok != null && tok.tok != TokenKind.TK_SPACE)
                    tokens.Add(tok);
            }
            while (tok.tok != TokenKind.TK_EOI);

            diagnostics.AddRange(lex.getDiagnostics());
            tCount = tokens.Count;

            if (tokens.Count > 0)
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
            if (current.tok == tok)
                return nextToken();

            //addError(string.Format("Unreconized token [{2}] at ({0},{1})", current.line, current.pos, current.sym));
            diagnostics.reportUnexpectedToken(current.span, current.tok, tok);
            return new SyntaxToken(current.tok, current.line, current.pos, current.sym);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseBinaryExpresion(int parentPrecedence = 0)
        {
            int precedence, uniPrecedence;
            ExpresionSyntax left, right, operand;
            SyntaxToken operatorToken;

            uniPrecedence = 0;
            uniPrecedence = current.tok.getUinaryOporatorPrecedence();

            // account for nagative(-) symbol before numbers
            if (uniPrecedence > parentPrecedence)
            {
                operatorToken = nextToken();
                operand = parsePrimaryExpresion();
                left = new UinaryExpressionSyntax(operatorToken, operand);
            }
            else
                left = parsePrimaryExpresion();

            // regulare numerical expressions
            while (true)
            {
                precedence = current.tok.getBinaryOporatorPrecedence();
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

            if (peek(0).tok == TokenKind.TK_BADTOKEN)
                return null;

            if (peek(0).tok == TokenKind.TK_IDENTIFIER && peek(1).tok == TokenKind.TK_ASSIGN)
            {
                identifierToken = nextToken();
                operatorToken = nextToken();
                right = parceAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }

            return parseBinaryExpresion();
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parsePrimaryExpresion()
        {
            SyntaxToken numberToken, left, righ, keyworkdTok, identifierToken;
            ExpresionSyntax exp;
            bool bolValue;

            // handle parethesies
            switch (current.tok)
            {
                case TokenKind.TK_PARENOPEN:
                    left = nextToken();
                    exp = parseBinaryExpresion();
                    righ = match(TokenKind.TK_PARENCLOSE);
                    return new ParenthesizedExpression(left, exp, righ);
                case TokenKind.TK_INDON:
                case TokenKind.TK_INDOFF:
                    bolValue = (current.tok == TokenKind.TK_INDON);
                    keyworkdTok = nextToken();
                    return new LiteralExpressionSyntax(keyworkdTok, bolValue);
                case TokenKind.TK_IDENTIFIER:
                    identifierToken = nextToken();
                    return new NamedExpressionSyntax(identifierToken);
                default:
                    // handel reglular math
                    numberToken = match(TokenKind.TK_INTEGER);
                    return new LiteralExpressionSyntax(numberToken);
            }

        }

        // ///////////////////////////////////////////////////////////////////////
        public SyntaxTree parse()
        {
            ExpresionSyntax ret;
            SyntaxToken tmp;

            ret = parceExpression();

            tmp = match(TokenKind.TK_EOI);

            return new SyntaxTree(diagnostics, ret, tmp);
        }

        // ///////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }
    }
}
