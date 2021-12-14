﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;
using rpgc.Syntax;
using rpgc.Text;

namespace rpgc.Syntax
{
    internal sealed class Parser
    {
        Lexer lex = null;
        SyntaxToken tok, current;
        TokenKind EndToken;
        int pos, tCount;
        DiagnosticBag diagnostics = new DiagnosticBag();
        public readonly ImmutableArray<SyntaxToken> tokens;
        public readonly SourceText source;

        public Parser(SourceText txt)
        {
            SyntaxToken ttkn;
            List<SyntaxToken> _tokens = new List<SyntaxToken>();
            lex = new Lexer(txt);
            pos = 0;
            source = txt;

            // get tokens
            do
            {
                // get token
                tok = lex.doLex();

                // special case for else add block end befor else
                if (tok.kind == TokenKind.TK_ELSE)
                    _tokens.Add(new SyntaxToken(TokenKind.TK_ENDIF, tok.line, tok.pos, ""));

                /* exit on error
                if (tok.kind == TokenKind.TK_BADTOKEN)
                {
                    _tokens.Add(tok);
                    break;
                }
                */

                // save avalable tokens skipping nulls and space
                if (tok != null && tok.kind != TokenKind.TK_SPACE)
                    _tokens.Add(tok);
            }
            while (tok.kind != TokenKind.TK_EOI);

            tokens = _tokens.ToImmutableArray();
            diagnostics.AddRange(lex.getDiagnostics());
            tCount = _tokens.Count;

            if (tokens.Count() > 0)
            {
                current = tokens[0];

                ttkn = tokens.Where(tk => tk.kind == TokenKind.TK_NEWLINE).FirstOrDefault();
                if (ttkn == null)
                    EndToken = TokenKind.TK_SEMI;
                else
                    EndToken = TokenKind.TK_NEWLINE;
            }
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
                case TokenKind.TK_STRING:
                    return parseStringLiteral();
                case TokenKind.TK_BADTOKEN:
                    return new ErrorExpressionSyntax();
                case TokenKind.TK_IDENTIFIER:
                default:
                    return parseTokenNamedOrCallExpression();
            }
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseStaement(TokenKind blockEndToken = TokenKind.TK_BLOCKEND)
        {
            switch (current.kind)
            {
                case TokenKind.TK_BLOCKSTART:
                    return parseBlockStaement(blockEndToken);
                case TokenKind.TK_IF:
                    return parseIfStaement();
                case TokenKind.TK_DOW:
                    return parsewhileStaement();
                case TokenKind.TK_DOU:
                    return parseUntilStaement();
                case TokenKind.TK_FOR:
                    return parseForStatement();
                case TokenKind.TK_TAG:
                    return parseTagStatement();
                case TokenKind.TK_GOTO:
                    return parseGoToStatement();
                case TokenKind.TK_VARDCONST:
                    return parseConstVariableDeclaration();
                case TokenKind.TK_VARDECLR:
                    return parseVariableDeclaration();
                case TokenKind.TK_ITER:
                    return parseContinueStatement();
                case TokenKind.TK_LEAVE:
                    return parseBreakStatement();
                case TokenKind.TK_RETURN:
                    return parseReturnStatement();
                case TokenKind.TK_BADTOKEN:
                    return new ErrorStatementSyntax();
                default:
                    return parseExpressionStaement();
            }
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
            return new SyntaxToken(TokenKind.TK_BADTOKEN, current.line, current.pos, current.sym);
        }

        // ///////////////////////////////////////////////////////////////////////
        private SyntaxToken match(TokenKind tok, TokenKind OptinalTok)
        {
            if (current.kind == tok || current.kind == OptinalTok)
                return nextToken();

            //addError(string.Format("Unreconized token [{2}] at ({0},{1})", current.line, current.pos, current.sym));
            diagnostics.reportUnexpectedToken(current.span, current.kind, tok);
            return new SyntaxToken(TokenKind.TK_BADTOKEN, current.line, current.pos, current.sym);
        }

        // ///////////////////////////////////////////////////////////////////////
        private void catchEndOfLine()
        {
            // find semicolon or new line to signify an end of instruction
            // semicolon is for free format
            // new line is for fixed format
            // -- EndToken is set in constructor
            match(EndToken);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseBinaryExpresion(int parentPrecedence = 0)
        {
            int precedence, uniPrecedence;
            ExpresionSyntax left, right, operand;
            SyntaxToken operatorToken;

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

            // regular literal expressions
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
        private ExpresionSyntax parceAssignmentExpression()
        {
            ExpresionSyntax right;
            ExpresionSyntax ret;
            SyntaxToken identifierToken, operatorToken;

            // parce assignment 
            if (peek(1).kind == TokenKind.TK_ASSIGN)
            {
                if (peek(0).kind == TokenKind.TK_IDENTIFIER && peek(1).kind == TokenKind.TK_ASSIGN)
                {
                    identifierToken = nextToken();
                    operatorToken = nextToken();
                    right = parceAssignmentExpression();
                    return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
                }
                else
                    diagnostics.reportAssignmentWithotResult(peek(1).span);
            }

            // next token is not an assignment
            // parce as binary expression
            ret = parseBinaryExpresion();

            return ret;
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parceExpression()
        {
            return parceAssignmentExpression();
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseTokenNamedExpression()
        {
            SyntaxToken identifier;
            //identifierToken = nextToken();
            identifier = match(TokenKind.TK_IDENTIFIER);

            //if (identifier.kind == TokenKind.TK_BADTOKEN)
            //    nextToken();

            return new NamedExpressionSyntax(identifier);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseTokenNamedOrCallExpression()
        {
            SyntaxNode a, b;
            
            a = peek(0);
            b = peek(1);

            if (SyntaxFacts.isNoParenthesisFunction(current.sym.ToString()))
                return parseExplicitCallExpression();

            if (a.kind == TokenKind.TK_IDENTIFIER && b.kind == TokenKind.TK_PARENOPEN)
                return parseCallExpression();

            return parseTokenNamedExpression();
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseExplicitCallExpression()
        {
            SyntaxToken identifier;
            SeperatedSyntaxList<ExpresionSyntax> exp;

            // function name
            identifier = match(TokenKind.TK_IDENTIFIER);

            // get value untill semicolon
            exp = parceExplicitCallArguments();

            return new CallExpressionSyntax(identifier, new SyntaxToken(TokenKind.TK_PARENOPEN,0,0,"("), exp, new SyntaxToken(TokenKind.TK_PARENCLOSE, 0, 0,")"));
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseCallExpression()
        {
            SyntaxToken identifier;
            SyntaxToken OpenPar, ClosePar;
            SeperatedSyntaxList<ExpresionSyntax> exp;

            // function name
            identifier = match(TokenKind.TK_IDENTIFIER);

            // parethesies
            OpenPar = match(TokenKind.TK_PARENOPEN);
            exp = parceArguments();
            ClosePar = match(TokenKind.TK_PARENCLOSE);

            return new CallExpressionSyntax(identifier, OpenPar, exp, ClosePar);
        }

        // ///////////////////////////////////////////////////////////////////////
        private SeperatedSyntaxList<ExpresionSyntax> parceExplicitCallArguments()
        {
            ExpresionSyntax exp;
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators2 = ImmutableArray.CreateBuilder<SyntaxNode>();

            if (peek(0).kind == TokenKind.TK_PARENOPEN)
            {
                match(TokenKind.TK_PARENOPEN);
                //parceArguments();
                match(TokenKind.TK_PARENCLOSE);
            }

            if (peek(0).kind != EndToken)
            {
                exp = parceExpression();
                nodesAndSeparators.Add(exp);
            }

            return new SeperatedSyntaxList<ExpresionSyntax>(nodesAndSeparators.ToImmutable());
        }

        // ///////////////////////////////////////////////////////////////////////
        private SeperatedSyntaxList<ExpresionSyntax> parceArguments()
        {
            ExpresionSyntax exp;
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            SyntaxNode colon;

            while (current.kind != TokenKind.TK_PARENCLOSE && current.kind != TokenKind.TK_EOI)
            {
                exp = parceExpression();
                nodesAndSeparators.Add(exp);

                if (current.kind != TokenKind.TK_PARENCLOSE)
                {
                    colon = match(TokenKind.TK_COLON);
                    nodesAndSeparators.Add(colon);
                }

            }

            return new SeperatedSyntaxList<ExpresionSyntax>(nodesAndSeparators.ToImmutable());
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
        private ExpresionSyntax parseTokenBoolenLiteral()
        {
            SyntaxToken keyworkdTok;
            bool bolValue;

            bolValue = (current.kind == TokenKind.TK_INDON);
            keyworkdTok = (bolValue ? match(TokenKind.TK_INDON) : match(TokenKind.TK_INDOFF));

            return new LiteralExpressionSyntax(keyworkdTok, bolValue);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseNumberLiteral()
        {
            SyntaxToken numberToken;
            numberToken = match(TokenKind.TK_INTEGER);
            return new LiteralExpressionSyntax(numberToken);
        }

        // /////////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseStringLiteral()
        {
            SyntaxToken stringToken;
            stringToken = match(TokenKind.TK_STRING);
            return new LiteralExpressionSyntax(stringToken);
        }

        // /////////////////////////////////////////////////////////////////////////
        private ExpressionStatementSyntax parseExpressionStaement()
        {
            ExpresionSyntax expression;

            expression = parceExpression();
            catchEndOfLine();

            return new ExpressionStatementSyntax(expression);
        }

        // /////////////////////////////////////////////////////////////////////////
        private TypeClauseSyntax parseTypeClause()
        {
            SyntaxToken identifr;

            identifr = match(TokenKind.TK_IDENTIFIER);

            return new TypeClauseSyntax(identifr);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseTagStatement()
        {
            SyntaxToken syntaxToken, identifier;
            string tagName;

            syntaxToken = match(TokenKind.TK_TAG);
            identifier = match(TokenKind.TK_IDENTIFIER);
            catchEndOfLine();
            tagName = identifier.sym.ToString();

            return new TagStatementSyntax(syntaxToken, tagName);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseGoToStatement()
        {
            SyntaxToken syntaxToken, identifier;
            string tagName;
            SyntaxToken ttoken;

            ttoken = current;
            syntaxToken = match(TokenKind.TK_GOTO);
            identifier = match(TokenKind.TK_IDENTIFIER);
            catchEndOfLine();
            tagName = identifier.sym.ToString();

            // get all tags and find the tag that macthes the one expected
            var x = (tokens
                 .Where(tag => tag.kind == TokenKind.TK_TAG)
                 .Where(lbl => lbl.special == tagName)).ToArray();

            // tag was not found in the program
            if (x.Length == 0)
            {
                diagnostics.reportMissingTag(ttoken.span, tagName);
                return new GoToStatementSyntax(new SyntaxToken(TokenKind.TK_BADTOKEN, ttoken.line, ttoken.pos, ""), tagName);
            }

            return new GoToStatementSyntax(syntaxToken, tagName);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseUntilStaement()
        {
            SyntaxToken keyword;
            ExpresionSyntax condition;
            StatementSyntax body;

            keyword = match(TokenKind.TK_DOU);
            condition = parceExpression();
            catchEndOfLine();
            body = parseStaement(TokenKind.TK_ENDDO);

            return new UntilStatementSyntax(keyword, condition, body);
        }

        // /////////////////////////////////////////////////////////////////////////
        // for [identifier] = [lBound] to [uBound] by [step]
        private StatementSyntax parseForStatement()
        {
            SyntaxToken keyword;
            SyntaxToken identifier;
            SyntaxToken equalsTok;
            SyntaxToken keywordTo;
            ExpresionSyntax lBound;
            ExpresionSyntax uBound;
            SyntaxToken keywordBy;
            ExpresionSyntax step;
            StatementSyntax body = null;

            keyword = match(TokenKind.TK_FOR);
            identifier = match(TokenKind.TK_IDENTIFIER);
            equalsTok = match(TokenKind.TK_ASSIGN);
            lBound = parceExpression();


            // set to/downto keyword
            keywordTo = peek(0);
            switch(keyword.kind)
            {
                case TokenKind.TK_DOWNTO:
                    match(TokenKind.TK_DOWNTO);
                    break;
                default:
                    match(TokenKind.TK_TO);
                    break;
            }

            // set uperbound limit
            uBound = parceExpression();

            // setup for increment
            if (peek(0).kind == TokenKind.TK_BY)
            {
                keywordBy = match(TokenKind.TK_BY);
                step = parceExpression();
            }
            else
            {
                keywordBy = null;
                step = null;
            }

            catchEndOfLine();
            body = parseStaement(TokenKind.TK_ENDFOR);

            return new ForStatementSyntax(keyword, identifier, equalsTok, lBound, keywordTo, uBound, body, keywordBy, step);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseBreakStatement()
        {
            SyntaxToken keywrd;
            keywrd = match(TokenKind.TK_LEAVE);
            catchEndOfLine();
            return new BreakStamentSyntax(keywrd);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseContinueStatement()
        {
            SyntaxToken keywrd;
            keywrd = match(TokenKind.TK_ITER);
            catchEndOfLine();
            return new ContinueStamentSyntax(keywrd);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parsewhileStaement()
        {
            SyntaxToken keyword;
            ExpresionSyntax condition;
            StatementSyntax body;

            keyword = match(TokenKind.TK_DOW);
            condition = parceExpression();
            catchEndOfLine();
            body = parseStaement(TokenKind.TK_ENDDO);

            return new WhileStatementSyntax(keyword, condition, body);
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
            catchEndOfLine();
            statement = parseStaement(TokenKind.TK_ENDIF);
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

            keyword = match(TokenKind.TK_ELSE);
            catchEndOfLine();
            statement = parseStaement(TokenKind.TK_ENDIF);

            return new ElseStatementSyntax(keyword, statement);
        }

        // ///////////////////////////////////////////////////////////////////////
        private StatementSyntax parseReturnStatement()
        {
            SyntaxToken keyword;
            ExpresionSyntax retExp;
            SyntaxToken tok;

            // match keyword and set default value for return
            keyword = match(TokenKind.TK_RETURN);
            retExp = null;
            tok = peek(0);

            // report EOI error
            if (tok.kind == TokenKind.TK_EOI)
            {
                diagnostics.reportUnexpectedToken(current.span, current.kind, EndToken);
                return new ErrorStatementSyntax();
            }

            // check if the next token is a end of line token
            // if not treat it as the returning expression
            if (tok.kind != EndToken)
                retExp = parseBinaryExpresion();

            // consume the end of line token
            catchEndOfLine();

            return new ReturnStatementSyntax(keyword, retExp);
        }

        private StatementSyntax parseConstVariableDeclaration()
        {
            SyntaxToken keyworkd, identifier, initKeyWord, varType;
            ExpresionSyntax initilize;

            // get declaration symbol and variable name
            // dcl-s name
            keyworkd = match(TokenKind.TK_VARDCONST);
            identifier = match(TokenKind.TK_IDENTIFIER);

            // identifier check
            // insert error when an error token was found
            if (identifier.kind != TokenKind.TK_IDENTIFIER)
                return new VariableDeclarationSyntax(keyworkd, identifier, TypeSymbol.ERROR, null, null);

            initilize = parseBinaryExpresion();
            catchEndOfLine();

            // return new constant variable
            return new VariableDeclarationSyntax(keyworkd, identifier, initilize);
        }
        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseVariableDeclaration()
        {
            TokenKind expected;
            SyntaxToken keyworkd, identifier, initKeyWord, varType;
            ExpresionSyntax initilize;
            TypeClauseSyntax typClas;
            TypeSymbol ts = null;

            expected = TokenKind.TK_VARDECLR;

            // get declaration symbol and variable name
            // dcl-s name
            keyworkd = match(TokenKind.TK_VARDECLR);
            identifier = match(TokenKind.TK_IDENTIFIER);

            // identifier check
            // insert error when an error token was found
            if (identifier.kind != TokenKind.TK_IDENTIFIER)
                return new VariableDeclarationSyntax(keyworkd, identifier, TypeSymbol.ERROR, null, null);

            // get expected type
            /*
            switch (current.kind)
            {
                case TokenKind.TK_INTEGER:
                    ts = TypeSymbol.Integer;
                    expected = current.kind;
                    break;
                case TokenKind.TK_INDICATOR:
                    ts = TypeSymbol.Indicator;
                    expected = current.kind;
                    break;
                case TokenKind.TK_DATE:
                    ts = TypeSymbol.Date;
                    expected = current.kind;
                    break;
                case TokenKind.TK_DATETIME:
                    ts = TypeSymbol.DateTime;
                    expected = current.kind;
                    break;
                case TokenKind.TK_ZONED:
                    ts = TypeSymbol.Integer;
                    expected = current.kind;
                    break;
                case TokenKind.TK_PACKED:
                    ts = TypeSymbol.Integer;
                    expected = current.kind;
                    break;
                case TokenKind.TK_STRING:
                    ts = TypeSymbol.Char;
                    expected = current.kind;
                    break;
                default:
                    ts = TypeSymbol.ERROR;
                    break;
            }
            //varType = match(expected);
            */
            typClas = parseTypeClause();

            // add varialbe initilizer if any
            if (peek(0).kind == TokenKind.TK_INZ)
            {
                //nextToken();
                initKeyWord = match(TokenKind.TK_INZ);
                initilize = parceExpression();
            }
            else
            {
                initKeyWord = null;
                initilize = null;
            }

            catchEndOfLine();

            //return new VariableDeclarationSyntax(keyworkd, identifier, ts, initKeyWord, initilize);
            return new VariableDeclarationSyntax(keyworkd, identifier, typClas, initKeyWord, initilize);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseBlockStaement(TokenKind blockEndToken = TokenKind.TK_BLOCKEND)
        {
            ImmutableArray<StatementSyntax>.Builder statements;
            SyntaxToken openStatementToken;
            SyntaxToken closeStatementToken;
            SyntaxToken startToken;
            StatementSyntax statmt;

            // account for: do, ifeq, doweq, etc symbols
            // do has two end types (enddo/ end)
            if (current.sym.ToString() == "DO")
                blockEndToken = TokenKind.TK_ENDDO;

            statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            openStatementToken = match(TokenKind.TK_BLOCKSTART);

            // build block body
            while (current.kind != TokenKind.TK_EOI &&
                   current.kind != blockEndToken &&
                   current.kind != TokenKind.TK_BLOCKEND)
            {
                startToken = current;

                statmt = parseStaement();
                statements.Add(statmt);

                // parseStaement did not consume any tokens
                // no need to report an error 
                if (current == startToken) {
                    nextToken();
                }
            }

            // match end statement token
            closeStatementToken = match(blockEndToken, TokenKind.TK_BLOCKEND);
            if (peek(0).kind == EndToken)
                catchEndOfLine();

            return new BlockStatementSyntax(openStatementToken, statements.ToImmutable(), closeStatementToken);
        }

        // ///////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }

        // ///////////////////////////////////////////////////////////////////////
        private ImmutableArray<MemberSyntax> parseMembers()
        {
            ImmutableArray<MemberSyntax>.Builder members;
            SyntaxToken startToken;
            MemberSyntax member;

            members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (current.kind != TokenKind.TK_EOI)
            {
                startToken = current;

                member = parseMember();
                members.Add(member);

                // parseStaement did not consume any tokens
                // go to next token 
                // if this is removed then an infinite loop will occur
                if (current == startToken)
                {
                    nextToken();
                }
            }

            return members.ToImmutable();
        }

        // ///////////////////////////////////////////////////////////////////////
        private MemberSyntax parseMember()
        {
            if (current.kind == TokenKind.TK_PRCKKEYWRD || current.kind == TokenKind.TK_PROCDCL || current.kind == TokenKind.TK_BEGSR)
                return parseProcedureDeclaration();

            return parseGlobalStatment();
        }

        // ///////////////////////////////////////////////////////////////////////
        private MemberSyntax parseGlobalStatment()
        {
            StatementSyntax statment;

            statment = parseStaement();

            return new GlobalStatmentSyntax(statment);
        }

        // ///////////////////////////////////////////////////////////////////////
        private MemberSyntax parseProcedureDeclaration()
        {
            SyntaxToken funcDclar;
            SyntaxToken identifier;
            SyntaxToken intface = null;
            SyntaxToken pocInterfaceName = null;
            SyntaxToken endInterface = null;
            SeperatedParamiterList<ParamiterSyntax> parms = null;
            TypeClauseSyntax returnType = null;
            StatementSyntax body;
            TokenKind procToken;
            bool isSub;

            funcDclar = match(TokenKind.TK_PROCDCL, TokenKind.TK_BEGSR);
            identifier = match(TokenKind.TK_IDENTIFIER);
            isSub = (funcDclar.kind == TokenKind.TK_BEGSR);
            catchEndOfLine();
            procToken = peek(0).kind;

            if (procToken == TokenKind.TK_PROCINFC)
            {
                if (isSub == false)
                {
                    intface = match(TokenKind.TK_PROCINFC);
                    pocInterfaceName = match(TokenKind.TK_IDENTIFIER);
                    returnType = parceOptinalTypeClause();
                    catchEndOfLine();
                    parms = parseParamiterList();
                    endInterface = match(TokenKind.TK_ENDPI);
                    catchEndOfLine();
                }
                else
                {
                    diagnostics.reportSubroutineParamiters(current.span);
                    return new ErrorMemberSyntax();
                }
            }

            // special case for procedure block
            // a dedicated block parcer was made because procs dont end with [END] code
            body = parseProcedureBlockStaement();

            return new ProcedureDeclarationSyntax(funcDclar, identifier, intface, pocInterfaceName, returnType, parms, endInterface, body, isSub);
        }

        // ///////////////////////////////////////////////////////////////////////
        private TypeClauseSyntax parceOptinalTypeClause()
        {
            SyntaxToken identifier;

            // no type given exit with null
            if (peek(0).kind == EndToken)
                return null;

            // get the return type
            identifier = match(TokenKind.TK_IDENTIFIER);

            return new TypeClauseSyntax(identifier);
        }

        // ///////////////////////////////////////////////////////////////////////
        private SeperatedParamiterList<ParamiterSyntax> parseParamiterList()
        {
            ParamiterSyntax prm;
            ImmutableArray<SyntaxNode>.Builder nodes = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (current.kind != TokenKind.TK_ENDPI && current.kind != TokenKind.TK_EOI)
            {
                prm = ParamiterSyntax();
                nodes.Add(prm);
            }

            return new SeperatedParamiterList<ParamiterSyntax>(nodes.ToImmutable());
        }

        // ///////////////////////////////////////////////////////////////////////
        private ParamiterSyntax ParamiterSyntax()
        {
            SyntaxToken identifier;
            TypeClauseSyntax type;

            identifier = match(TokenKind.TK_IDENTIFIER);
            type = parseTypeClause();
            catchEndOfLine();

            return new ParamiterSyntax(identifier, type);
        }

        // /////////////////////////////////////////////////////////////////////////
        // function and procedure
        private StatementSyntax parseProcedureBlockStaement()
        {
            ImmutableArray<StatementSyntax>.Builder statements;
            SyntaxToken openStatementToken;
            SyntaxToken closeStatementToken;
            SyntaxToken startToken;
            StatementSyntax statmt;


            statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            //openStatementToken = match(TokenKind.TK_BLOCKSTART);
            openStatementToken = null;

            // build block body
            while (current.kind != TokenKind.TK_EOI &&
                   current.kind != TokenKind.TK_ENDPROC)
            {
                startToken = current;

                statmt = parseStaement();
                statements.Add(statmt);

                // parseStaement did not consume any tokens
                // no need to report an error 
                if (current == startToken)
                {
                    nextToken();
                }
            }

            // match end statement token
            closeStatementToken = match(TokenKind.TK_ENDPROC);
            catchEndOfLine();

            return new BlockStatementSyntax(openStatementToken, statements.ToImmutable(), closeStatementToken);
        }

        // ///////////////////////////////////////////////////////////////////////
        public CompilationUnit parseCompilationUnit()
        {
            ImmutableArray<MemberSyntax> ret;
            SyntaxToken tmp;

            ret = parseMembers();

            tmp = match(TokenKind.TK_EOI);

            return new CompilationUnit(ret, tmp);
        }
    }
}