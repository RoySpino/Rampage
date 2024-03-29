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
        SyntaxToken lastToken, current;
        TokenKind EndToken;
        int pos, tCount;
        string curSubroutineScope;
        DiagnosticBag diagnostics = new DiagnosticBag();
        public readonly ImmutableArray<SyntaxToken> tokens;
        readonly SyntaxTree _sTree;
        SourceText text;
        TextLocation location;
        Dictionary<string, string> functionReturns = new Dictionary<string, string>();

        public Parser(SyntaxTree syntaxTree)
        {
            SyntaxToken ttkn;
            List<SyntaxToken> _tokens = new List<SyntaxToken>();
            lex = new Lexer(syntaxTree);
            text = syntaxTree.TEXT;
            pos = 0;
            _sTree = syntaxTree;
            lastToken = null;


            // get tokens
            _tokens = lex.getLexTokenList();

            // setup local token array and merge lexer diagnostics
            tokens = _tokens.ToImmutableArray();
            diagnostics.AddRange(lex.getDiagnostics());
            tCount = _tokens.Count;

            // setup current token and line turminator token
            if (tokens.Count() > 0)
            {
                current = tokens[0];

                // new line terminator is used for fixed format RPG
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
                case TokenKind.TK_FLOAT:
                    return parseFloatLiteral ();
                case TokenKind.TK_STRING:
                    return parseStringLiteral();
                case TokenKind.TK_EXSR:
                     return parseSubroutineCall();
                case TokenKind.TK_BADTOKEN:
                    nextToken();
                    return new ErrorExpressionSyntax(_sTree);
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
                case TokenKind.TK_SELECT:
                    return parseSelectStatement();
                case TokenKind.TK_CAS:
                    return parseCasStaement();
                case TokenKind.TK_CAB:
                    return parseCabStaement();
                case TokenKind.TK_BADTOKEN:
                    nextToken();
                    return new ErrorStatementSyntax(_sTree);
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
            diagnostics.reportUnexpectedToken(current.Location(), current.kind, tok);
            return new SyntaxToken(_sTree, TokenKind.TK_BADTOKEN, current.line, current.pos, current.sym);
        }

        // ///////////////////////////////////////////////////////////////////////
        private SyntaxToken match(TokenKind tok, TokenKind OptinalTok)
        {
            if (current.kind == tok || current.kind == OptinalTok)
                return nextToken();

            //addError(string.Format("Unreconized token [{2}] at ({0},{1})", current.line, current.pos, current.sym));
            diagnostics.reportUnexpectedToken(current.Location(), current.kind, tok);
            return new SyntaxToken(_sTree, TokenKind.TK_BADTOKEN, current.line, current.pos, current.sym);
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
                left = new UinaryExpressionSyntax(_sTree, operatorToken, operand);
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
                left = new BinaryExpressionSyntax(_sTree, left, operatorToken, right);
            }

            return left;
        }


        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parceAssignmentExpression()
        {
            ExpresionSyntax right;
            ExpresionSyntax ret;
            SyntaxToken identifierToken, operatorToken;
            SyntaxToken peek_1, peek_0;

            peek_0 = peek(0);
            peek_1 = peek(1);

            // parce assignment 
            if (peek_1.kind == TokenKind.TK_ASSIGN)
            {
                if (peek_0.kind == TokenKind.TK_IDENTIFIER && peek_1.kind == TokenKind.TK_ASSIGN)
                {
                    identifierToken = nextToken();
                    operatorToken = nextToken();
                    right = parceAssignmentExpression();
                    return new AssignmentExpressionSyntax(_sTree, identifierToken, operatorToken, right);
                }
                else
                {
                    location = new TextLocation(text, peek_1.span);
                    diagnostics.reportAssignmentWithotResult(location);
                }
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

            return new NamedExpressionSyntax(_sTree,identifier);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseSubroutineCall()
        {
            // new SeperatedSyntaxList<ExpresionSyntax>(ImmutableArray.CreateBuilder<SyntaxNode>().ToImmutable());
            SyntaxToken identifier;

            // get execution symbol and parce function
            match(TokenKind.TK_EXSR);
            identifier = match(TokenKind.TK_IDENTIFIER);

            if (identifier.sym.ToString() == curSubroutineScope)
            {
                location = new TextLocation(text, new TextSpan(0, identifier.sym.ToString().Length, identifier.linePosition, identifier.pos));
                diagnostics.reportSubroutineRecursion(location);
                return new ErrorExpressionSyntax(_sTree);
            }

            return new CallExpressionSyntax(_sTree, identifier,
                                            new SyntaxToken(_sTree, TokenKind.TK_PARENOPEN, 0, 0, "("),
                                            new SeperatedSyntaxList<ExpresionSyntax>(ImmutableArray.CreateBuilder<SyntaxNode>().ToImmutable()),
                                            new SyntaxToken(_sTree, TokenKind.TK_PARENCLOSE, 0, 0, ")"),
                                            true);
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

            if (identifier.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            // get value untill semicolon
            exp = parceExplicitCallArguments();

            return new CallExpressionSyntax(_sTree, identifier,
                                            new SyntaxToken(_sTree, TokenKind.TK_PARENOPEN, 0, 0, "("),
                                            exp,
                                            new SyntaxToken(_sTree, TokenKind.TK_PARENCLOSE, 0, 0, ")"));
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseCallExpression()
        {
            SyntaxToken identifier;
            SyntaxToken OpenPar, ClosePar;
            SeperatedSyntaxList<ExpresionSyntax> exp;

            // function name
            identifier = match(TokenKind.TK_IDENTIFIER);
            if (identifier.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            // parethesies
            OpenPar = match(TokenKind.TK_PARENOPEN);
            if (OpenPar.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            // arguments
            exp = parceArguments();

            // parethesies
            ClosePar = match(TokenKind.TK_PARENCLOSE);
            if (ClosePar.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            return new CallExpressionSyntax(_sTree, identifier, OpenPar, exp, ClosePar);
        }

        // ///////////////////////////////////////////////////////////////////////
        private SeperatedSyntaxList<ExpresionSyntax> parceExplicitCallArguments()
        {
            ExpresionSyntax exp;
            ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

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
            if (left.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);
            
            exp = parseBinaryExpresion();

            righ = match(TokenKind.TK_PARENCLOSE);
            if (righ.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            return new ParenthesizedExpression(_sTree, left, exp, righ);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseTokenBoolenLiteral()
        {
            SyntaxToken keyworkdTok;
            bool bolValue;

            bolValue = (current.kind == TokenKind.TK_INDON);

            keyworkdTok = (bolValue ? match(TokenKind.TK_INDON) : match(TokenKind.TK_INDOFF));
            if (keyworkdTok.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            return new LiteralExpressionSyntax(_sTree, keyworkdTok, bolValue);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseNumberLiteral()
        {
            SyntaxToken numberToken;

            numberToken = match(TokenKind.TK_INTEGER);
            if (numberToken.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            return new LiteralExpressionSyntax(_sTree,numberToken);
        }

        // ///////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseFloatLiteral()
        {
            SyntaxToken numberToken;

            numberToken = match(TokenKind.TK_FLOAT);
            if (numberToken.kind == TokenKind.TK_BADTOKEN)
                return new ErrorExpressionSyntax(_sTree);

            return new LiteralExpressionSyntax(_sTree, numberToken);
        }

        // /////////////////////////////////////////////////////////////////////////
        private ExpresionSyntax parseStringLiteral()
        {
            SyntaxToken stringToken;

            stringToken = match(TokenKind.TK_STRING);

            return new LiteralExpressionSyntax(_sTree,stringToken);
        }

        // /////////////////////////////////////////////////////////////////////////
        private ExpressionStatementSyntax parseExpressionStaement()
        {
            ExpresionSyntax expression;

            expression = parceExpression();
            catchEndOfLine();

            return new ExpressionStatementSyntax(_sTree, expression);
        }

        // /////////////////////////////////////////////////////////////////////////
        private TypeClauseSyntax parseTypeClause()
        {
            SyntaxToken identifr;

            identifr = match(TokenKind.TK_IDENTIFIER);

            return new TypeClauseSyntax(_sTree, identifr);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseTagStatement()
        {
            SyntaxToken syntaxToken, identifier;
            string tagName;

            syntaxToken = match(TokenKind.TK_TAG);

            identifier = match(TokenKind.TK_IDENTIFIER);
            if (identifier.kind == TokenKind.TK_BADTOKEN)
                return new ErrorStatementSyntax(_sTree);

            catchEndOfLine();
            tagName = identifier.sym.ToString();

            return new TagStatementSyntax(_sTree, syntaxToken, tagName);
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
            if (identifier.kind == TokenKind.TK_BADTOKEN)
                return new ErrorStatementSyntax(_sTree);

            catchEndOfLine();
            tagName = identifier.sym.ToString();

            // get all tags and find the tag that macthes the one expected
            var x = (tokens
                 .Where(tag => tag.kind == TokenKind.TK_TAG)
                 .Where(lbl => lbl.special == tagName)).ToArray();

            // tag was not found in the program
            if (x.Length == 0)
            {
                location = new TextLocation(text, ttoken.span);
                diagnostics.reportMissingTag(location, tagName);
                return new ErrorStatementSyntax(_sTree);
                //return new GoToStatementSyntax(new SyntaxToken(_sTree, TokenKind.TK_BADTOKEN, ttoken.line, ttoken.pos, ""), tagName);
            }

            return new GoToStatementSyntax(_sTree, syntaxToken, tagName);
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

            return new UntilStatementSyntax(_sTree, keyword, condition, body);
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
            if (identifier.kind == TokenKind.TK_BADTOKEN)
                return new ErrorStatementSyntax(_sTree);

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

            return new ForStatementSyntax(_sTree, keyword, identifier, equalsTok, lBound, keywordTo, uBound, body, keywordBy, step);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseBreakStatement()
        {
            SyntaxToken keywrd;
            keywrd = match(TokenKind.TK_LEAVE);
            catchEndOfLine();
            return new BreakStamentSyntax(_sTree, keywrd);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseContinueStatement()
        {
            SyntaxToken keywrd;
            keywrd = match(TokenKind.TK_ITER);
            catchEndOfLine();
            return new ContinueStamentSyntax(_sTree, keywrd);
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

            return new WhileStatementSyntax(_sTree, keyword, condition, body);
        }

        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseSelectStatement()
        {
            SyntaxToken keyword, whenKw;
            ExpresionSyntax condition;
            StatementSyntax statement;
            StatementSyntax defaultStmnt;
            List<StatementSyntax> whenBdy;
            List<ExpresionSyntax> whenCond;

            whenBdy = new List<StatementSyntax>();
            whenCond = new List<ExpresionSyntax>();
            defaultStmnt = null;

            keyword = match(TokenKind.TK_SELECT);
            catchEndOfLine();

            while (current.kind != TokenKind.TK_ENDSL &&
                   current.kind != TokenKind.TK_EOI &&
                   current.kind != TokenKind.TK_BLOCKEND)
            {
                whenKw = match(TokenKind.TK_WHEN, TokenKind.TK_OTHER);

                // check if user entered a when/other keyword
                if (whenKw.kind == TokenKind.TK_BADTOKEN)
                {
                    diagnostics.reportUnexpectedSymbol(whenKw.Location(), "When or Other");
                    return new ErrorStatementSyntax(_sTree);
                }

                // get the condition on the when keyword then
                // add condtion to list
                if (whenKw.kind == TokenKind.TK_WHEN)
                {
                    condition = parceExpression();
                    catchEndOfLine();
                    whenCond.Add(condition);

                    // colect the statements in the body of the when keyword
                    statement = parseStaement();
                    whenBdy.Add(statement);
                }
                else
                {
                    catchEndOfLine();

                    // parse the default statment
                    defaultStmnt = parseStaement();
                }
            }

            // catch end keyword and end of line
            match(TokenKind.TK_ENDSL);
            catchEndOfLine();

            return new Syntax.SelectStatementSyntax(_sTree, keyword, whenCond, whenBdy, defaultStmnt);
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

            return new IfStatementSyntax(_sTree, keyword, condition, statement, elseClause);
        }

        // /////////////////////////////////////////////////////////////////////////
        private CaseStatementSyntax parseCasStaement()
        {
            SyntaxToken keyword;
            List<ExpresionSyntax> conditions = new List<ExpresionSyntax>();
            List<StatementSyntax> statements = new List<StatementSyntax>();

            // basic setup
            keyword = null;

            // this has atleast ONE good cycle befor an error occurs
            while (current.kind != TokenKind.TK_ENDCS &&
                current.kind != TokenKind.TK_EOI &&
                current.kind != TokenKind.TK_END)
            {
                // current token at this point must be a CAS 
                // otherwis it is an error exit loop
                if (current.kind != TokenKind.TK_CAS)
                {
                    diagnostics.reportMissingEndCS(_sTree, keyword.Location());
                    break;
                }

                // collect CAS token
                keyword = match(TokenKind.TK_CAS);

                // get the condition
                conditions.Add(parceExpression());
                catchEndOfLine();

                // get subrutine call
                statements.Add(parseStaement(TokenKind.TK_ENDCS));

                if (current.kind == TokenKind.TK_NEWLINE)
                    catchEndOfLine();
            }

            // consume end of line statement
            match(TokenKind.TK_ENDCS);
            catchEndOfLine();

            return new CaseStatementSyntax(_sTree, keyword, conditions, statements);
        }

        // /////////////////////////////////////////////////////////////////////////
        private CabStatementSyntax parseCabStaement()
        {
            SyntaxToken keyword;
            List<ExpresionSyntax> conditions = new List<ExpresionSyntax>();
            List<StatementSyntax> statements = new List<StatementSyntax>();
            StatementSyntax gotoStm;

            // basic setup
            keyword = null;

            // loop only if the current token is a CAB
            while (current.kind != TokenKind.TK_EOI &&
                current.kind == TokenKind.TK_CAB)
            {
                // collect CAS token
                keyword = match(TokenKind.TK_CAB);

                // get the condition
                conditions.Add(parceExpression());
                catchEndOfLine();

                // get GOTO and indicator statments
                gotoStm = parseStaement();
                statements.Add(gotoStm);

                if (current.kind == TokenKind.TK_NEWLINE)
                    catchEndOfLine();
            }

            return new CabStatementSyntax(_sTree, keyword, conditions, statements);
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

            return new ElseStatementSyntax(_sTree, keyword, statement);
        }

        // ///////////////////////////////////////////////////////////////////////
        private StatementSyntax parseReturnStatement()
        {
            SyntaxToken keyword;
            ExpresionSyntax retExp;
            SyntaxToken tok;
            string funcReturns;

            // match keyword and set default value for return
            keyword = match(TokenKind.TK_RETURN);
            retExp = null;
            tok = peek(0);

            // report EOI error
            if (tok.kind == TokenKind.TK_EOI)
            {
                location = new TextLocation(text, current.span);
                diagnostics.reportUnexpectedToken(location, current.kind, EndToken);
                return new ErrorStatementSyntax(_sTree);
            }

            // check if the next token is a end of line token
            // if not treat it as the returning expression
            if (tok.kind != EndToken)
                retExp = parseBinaryExpresion();

            // consume the end of line token
            catchEndOfLine();

            if (functionReturns.ContainsKey(curSubroutineScope))
            {
                funcReturns = functionReturns[curSubroutineScope];

                // check for lone retrun statements
                // RETURN that do not return anything should only be in void funcitons
                if (retExp == null) 
                {
                    if (funcReturns != "")
                    {
                        diagnostics.reportProcedureSouldNotReturnVoid(keyword.Location(), funcReturns);
                        return new ErrorStatementSyntax(_sTree);
                    }
                }
            }

            return new ReturnStatementSyntax(_sTree, keyword, retExp);
        }

        // ///////////////////////////////////////////////////////////////////////
        private StatementSyntax parseConstVariableDeclaration()
        {
            SyntaxToken keyworkd, identifier;
            ExpresionSyntax initilize;

            // get declaration symbol and variable name
            // dcl-s name
            keyworkd = match(TokenKind.TK_VARDCONST);
            identifier = match(TokenKind.TK_IDENTIFIER);

            // identifier check
            // insert error when an error token was found
            if (identifier.kind != TokenKind.TK_IDENTIFIER)
                return new VariableDeclarationSyntax(_sTree, keyworkd, identifier, TypeSymbol.ERROR, null, null);

            initilize = parseBinaryExpresion();
            catchEndOfLine();

            // return new constant variable
            return new VariableDeclarationSyntax(_sTree, keyworkd, identifier, initilize);
        }
        // /////////////////////////////////////////////////////////////////////////
        private StatementSyntax parseVariableDeclaration()
        {
            SyntaxToken keyworkd, identifier, initKeyWord;
            ExpresionSyntax initilize;
            TypeClauseSyntax typClas;

            // get declaration symbol and variable name
            // dcl-s name
            keyworkd = match(TokenKind.TK_VARDECLR);
            identifier = match(TokenKind.TK_IDENTIFIER);

            // identifier check
            // insert error when an error token was found
            if (identifier.kind != TokenKind.TK_IDENTIFIER)
                return new VariableDeclarationSyntax(_sTree, keyworkd, identifier, TypeSymbol.ERROR, null, null);

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
            return new VariableDeclarationSyntax(_sTree, keyworkd, identifier, typClas, initKeyWord, initilize);
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

            return new BlockStatementSyntax(_sTree, openStatementToken, statements.ToImmutable(), closeStatementToken);
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

            return new GlobalStatmentSyntax(_sTree, statment);
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
            TokenKind procToken, tmpTok;
            bool isSub;
            string functionName;

            isSub = (current.sym.ToString() == "BEGSR");
            funcDclar = match(TokenKind.TK_PROCDCL);
            identifier = match(TokenKind.TK_IDENTIFIER);

            if (identifier.kind == TokenKind.TK_BADTOKEN)
                return new ErrorMemberSyntax(_sTree);
            catchEndOfLine();

            procToken = peek(0).kind;
            curSubroutineScope = identifier.sym.ToString();

            // check if ther are any paramiters given
            if (procToken == TokenKind.TK_PROCINFC)
            {
                // check if the paramters are in a procedure or subroutine
                if (isSub == false)
                {
                    intface = match(TokenKind.TK_PROCINFC);

                    // check interface name
                    pocInterfaceName = match(TokenKind.TK_IDENTIFIER);
                    if (pocInterfaceName.kind == TokenKind.TK_BADTOKEN)
                        return new ErrorMemberSyntax(_sTree);

                    functionName = pocInterfaceName.sym.ToString().ToUpper();

                    // check if procedure name matches interface name
                    if (functionName != curSubroutineScope && functionName != "*N")
                    {
                        diagnostics.reportProcedureNameMismatch(pocInterfaceName.Location(), curSubroutineScope);
                        return new ErrorMemberSyntax(_sTree);
                    }

                    // check for return type
                    tmpTok = peek(0).kind;
                    if (tmpTok != TokenKind.TK_SEMI || tmpTok != TokenKind.TK_NEWLINE)
                    {
                        returnType = parceOptinalTypeClause();
                    }

                    // start getting procedure paramiters
                    catchEndOfLine();
                    parms = parseParamiterList();

                    // complete interface block
                    endInterface = match(TokenKind.TK_ENDPI);
                    if (endInterface.kind == TokenKind.TK_BADTOKEN)
                        return new ErrorMemberSyntax(_sTree);

                    // save function return type
                    // used to check return operator
                    if (returnType != null)
                        functionReturns.Add(curSubroutineScope, returnType.Identifier.sym.ToString());
                    else
                        functionReturns.Add(curSubroutineScope, "void");

                    catchEndOfLine();
                }
                else
                {
                    location = new TextLocation(text, current.span);
                    diagnostics.reportSubroutineParamiters(location);
                    return new ErrorMemberSyntax(_sTree);
                }
            }

            // special case for procedure block
            // a dedicated block parcer was made because procs dont end with [END] code
            body = parseProcedureBlockStaement();

            return new ProcedureDeclarationSyntax(_sTree, funcDclar, identifier, intface, pocInterfaceName, returnType, parms, endInterface, body, isSub);
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

            return new TypeClauseSyntax(_sTree, identifier);
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

            return new ParamiterSyntax(_sTree, identifier, type);
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

            return new BlockStatementSyntax(_sTree, openStatementToken, statements.ToImmutable(), closeStatementToken);
        }

        // ///////////////////////////////////////////////////////////////////////
        public CompilationUnit parseCompilationUnit()
        {
            ImmutableArray<MemberSyntax> ret;
            SyntaxToken tmp;

            ret = parseMembers();

            tmp = match(TokenKind.TK_EOI);

            return new CompilationUnit(_sTree, ret, tmp);
        }
    }
}
