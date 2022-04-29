using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using rpgc.Syntax;
using rpgc.Symbols;
using rpgc.Text;

namespace rpgc.Binding
{
    internal sealed class Binder
    {
        public DiagnosticBag diagnostics = new DiagnosticBag();
        public BoundScope scope;
        private FunctionSymbol function_;
        private int loopControlLableCnt = 0;
        public Stack<(BoundLabel BreakLable, BoundLabel ContinueLable)> _loopStack = new Stack<(BoundLabel BreakLable, BoundLabel ContinueLable)>();

        public Binder(BoundScope parant)
        {
            scope = new BoundScope(parant);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public Binder(BoundScope parant, FunctionSymbol funcn) : this(parant)
        {
            scope = new BoundScope(parant);
            function_ = funcn;

            if (function_ != null)
            {
                foreach(var p in function_.Paramiter)
                {
                    scope.declare(p);
                }
            }
        }

        // ///////////////////////////////////////////////////////////////////////////////
        //public static BoundGlobalScope bindGlobalScope(BoundGlobalScope prev, CompilationUnit syntax)
        public static BoundGlobalScope bindGlobalScope(BoundGlobalScope prev, ImmutableArray<SyntaxTree> sTrees)
        {
            Binder _binder;
            BoundScope parantScop;
            BoundBlockStatement stmt;
            BoundStatement statment;
            ImmutableArray<BoundStatement>.Builder statementBuilder;
            ImmutableArray<FunctionSymbol> functon;
            ImmutableArray<VariableSymbol> vars;
            ImmutableArray<Diagnostics> tDiagonostics;
            DiagnosticBag diag;
            BoundBlockStatement loweredBody;
            BoundStatement body;
            ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder functionBodies;
            IEnumerable<ProcedureDeclarationSyntax> functionDeclar;
            IEnumerable<GlobalStatmentSyntax> gblStatements;

            //BoundScope parantScope;

            parantScop = createParantScope(prev);
            _binder = new Binder(parantScop, funcn: null);
            diag = new DiagnosticBag();


            // get all procedures and subrutines from all source codes
            functionDeclar = sTrees.SelectMany(st => st.ROOT.Members).OfType<ProcedureDeclarationSyntax>();

            // load all built in functions first
            // doing this alows procedures to be called globaly like C# or java
            foreach (ProcedureDeclarationSyntax procedure in functionDeclar)
                _binder.bindFunctionDeclaration(procedure);



            // load user deffigned Functions
            functionBodies = ImmutableDictionary.CreateBuilder <FunctionSymbol, BoundBlockStatement>();

            if (prev != null)
            {
                foreach (FunctionSymbol _function in prev.Functons)
                {
                    _binder = new Binder(parantScop, _function);
                    body = _binder.BindStatements(_function.Declaration.Body);
                    loweredBody = Lowering.Lowerer.Lower(body);
                    functionBodies.Add(_function, loweredBody);
                    
                    diag.AddRange(_binder.diagnostics);
                }
            }

            // get all global statements from all source codes
            gblStatements = sTrees.SelectMany(st => st.ROOT.Members).OfType<GlobalStatmentSyntax>();


            // load main scope
            statementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (GlobalStatmentSyntax gblStmnt in gblStatements)
            {
                statment = _binder.BindStatements(gblStmnt.Statement);
                statementBuilder.Add(statment);
            }



            //stmt = bind.BindStatements(statment);
            stmt = new BoundBlockStatement(statementBuilder.ToImmutable());
            functon = _binder.scope.getDeclaredFunctions();
            vars = _binder.scope.getDeclaredVariables();
            diag.AddRange(_binder.diagnostics);// = bind.diagnostics.ToImmutableArray();
            tDiagonostics = diag.ToImmutableArray();

            if (prev != null)
                tDiagonostics = tDiagonostics.InsertRange(0, prev.Diagnostic);

            return new BoundGlobalScope(prev, tDiagonostics, functon, vars, stmt);
        }
        
        // ///////////////////////////////////////////////////////////////////////////////
        public static BoundProgram BindProgram(BoundGlobalScope gblScope)
        {
            BoundScope parantScope;
            Binder _binder;
            BoundProgram pgm;
            BoundStatement body;
            BoundGlobalScope scopes;
            BoundBlockStatement loweredBody;
            ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder functionBodies;
            ImmutableArray<Diagnostics> diagnostics;
            ImmutableArray<Diagnostics> tmp = ImmutableArray.Create<Diagnostics>();
            bool pathsError;


            diagnostics = gblScope.Diagnostic;
            parantScope = createParantScope(gblScope);
            functionBodies = ImmutableDictionary.CreateBuilder <FunctionSymbol, BoundBlockStatement>();
            scopes = gblScope;

            while (scopes != null)
            {
                foreach (FunctionSymbol _function in scopes.Functons)
                {
                    _binder = new Binder(parantScope, _function);
                    body = _binder.BindStatements(_function.Declaration.Body);
                    loweredBody = Lowering.Lowerer.Lower(body);

                    // check function paths
                    pathsError = ControlFlowGraph.AllPathsReturn(loweredBody);
                    if (_function.Type != TypeSymbol.Void && pathsError == false)
                        _binder.diagnostics.reportNotAllCodePathsReturn(_function.Declaration.Location(), _function.Name);

                    functionBodies.Add(_function, loweredBody);

                    tmp = tmp.AddRange(diagnostics);
                    tmp = tmp.AddRange(_binder.diagnostics);
                    diagnostics = tmp;
                }

                // go to next scope
                scopes = scopes.Preveous;
            }

            pgm = new BoundProgram(gblScope, diagnostics, functionBodies.ToImmutable());
            return pgm;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private static BoundScope createParantScope(BoundGlobalScope prev)
        {
            Stack<BoundGlobalScope> stk = new Stack<BoundGlobalScope>();
            BoundGlobalScope tmp;
            BoundScope parant = null;
            BoundScope _scope = null;

            // create a linklist of submitions
            while (prev != null)
            {
                stk.Push(prev);
                prev = prev.Preveous;
            }

            parant = createRootScope();

            while (stk.Count > 0)
            {
                prev = stk.Pop();
                _scope = new BoundScope(parant);

                foreach (FunctionSymbol f in prev.Functons)
                    _scope.declareFunction(f);

                foreach (VariableSymbol v in prev.Variables)
                    _scope.declare(v);

                parant = _scope;
            }

            return parant;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private static BoundScope createRootScope()
        {
            BoundScope res;

            res = new BoundScope(null);

            // declare built in functions
            foreach (var f in BuiltinFunctions.getAll())
                res.declareFunction(f);

            // declare builtin variables
            foreach (var v in SyntaxFacts.getAllIndicators())
                res.declare(new GlobalVariableSymbol($"*IN{v}", false, TypeSymbol.Indicator));

            res.declare(new GlobalVariableSymbol("^^LO", false, TypeSymbol.Integer));

            return res;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindStatements(StatementSyntax syntax)
        {
            switch (syntax.kind)
            {
                case TokenKind.TK_BLOCKSYNTX:
                    return BindBlockStatement((BlockStatementSyntax)syntax);
                case TokenKind.TK_EXPRNSTMNT:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                case TokenKind.TK_VARDECLR:
                    return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
                case TokenKind.TK_IF:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case TokenKind.TK_DOW:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case TokenKind.TK_DOU:
                    return BindUntilStatement((UntilStatementSyntax)syntax);
                case TokenKind.TK_FOR:
                    return BindForStatement((ForStatementSyntax)syntax);
                case TokenKind.TK_GOTO:
                    return BindGoToStatement((GoToStatementSyntax)syntax);
                case TokenKind.TK_TAG:
                    return BindTagStatement((TagStatementSyntax)syntax);
                case TokenKind.TK_ITER:
                    return BindContinueStatement((ContinueStamentSyntax)syntax);
                case TokenKind.TK_LEAVE:
                    return BindBreakStatement((BreakStamentSyntax)syntax);
                case TokenKind.TK_RETURN:
                    return BindReturnStatement((ReturnStatementSyntax)syntax);
                case TokenKind.TK_BADTOKEN:
                    return new BoundErrorStatement();
                default:
                    throw new Exception(string.Format("Unexpected Syntax {0}", syntax.kind));
            }
        }


        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindExpressionInternal(ExpresionSyntax syntax)
        {
            switch (syntax.kind)
            {
                case TokenKind.TK_PARENEXP:
                    return BindParenthesizedExpression((ParenthesizedExpression)syntax);
                case TokenKind.TK_LITEXPR:
                    return BindLiteralExp((LiteralExpressionSyntax)syntax);
                case TokenKind.TK_UNIEXP:
                    return BindUniExpression((UinaryExpressionSyntax)syntax);
                case TokenKind.TK_BYNARYEXPR:
                    return BindBinExpression((BinaryExpressionSyntax)syntax);
                case TokenKind.TK_NAMEDEXP:
                    return BindNamedExpression((NamedExpressionSyntax)syntax);
                case TokenKind.TK_ASSIGN:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case TokenKind.TK_CALL:
                case TokenKind.TK_CALLP:
                case TokenKind.TK_CALLB:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                case TokenKind.TK_BADTOKEN:
                    return new BoundErrorExpression();
                default:
                    throw new Exception(string.Format("Unexpected Syntax {0}", syntax.kind));
            }
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindErrorStatement()
        {
            return new BoundExpressionStatement(new BoundErrorExpression());
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private void bindFunctionDeclaration(ProcedureDeclarationSyntax syntax)
        {
            ImmutableArray<ParamiterSymbol>.Builder parms;
            string name;
            TypeClauseSyntax _type;
            TypeSymbol typ;
            TypeSymbol returnType;
            ParamiterSymbol par = null;
            HashSet<string> paramiterNames;
            FunctionSymbol procedur;

            parms = ImmutableArray.CreateBuilder<ParamiterSymbol>();
            paramiterNames = new HashSet<string>();
            if (syntax.Paramiters != null)
            {
                foreach (ParamiterSyntax pram in syntax.Paramiters)
                {
                    name = pram.Identifier.sym.ToString();
                    _type = checkVarType(pram.Type, new TextLocation(pram.STREE.TEXT, pram.span));
                    typ = bindTypeClause(_type);

                    if (paramiterNames.Add(name) == false)
                        diagnostics.reportDuplicateParamiterName(pram.Location(), name, typ.Name);
                    else
                    {
                        par = new ParamiterSymbol(name, typ);
                        parms.Add(par);
                    }
                }
            }

            name = syntax.ProcedureName.sym.ToString();  //IdentfirePN.sym.ToString();
            _type = checkVarType(syntax.ReturnType, new TextLocation(syntax.STREE.TEXT, syntax.span));
            returnType = bindTypeClause(syntax.ReturnType); //?? TypeSymbol.Void;
            procedur = new FunctionSymbol(name, parms.ToImmutable(), returnType, syntax, syntax.isSubroutine);

            if (scope.declareFunction(procedur) == false)
                diagnostics.reportFunctionAlreadyDeclared(syntax.Location(), name);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindExpression(ExpresionSyntax syntax, bool canBeVoid = false)
        {
            BoundExpression result = BindExpressionInternal(syntax);

            if(canBeVoid == false && result.Type == TypeSymbol.Void)
            {
                diagnostics.reportExpressionMustHaveValue(syntax.Location());
                return new BoundErrorExpression();
            }

            return result;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindExpression(ExpresionSyntax syntax, TypeSymbol expectedResult)
        {
            return BindConversion(syntax, expectedResult);
            /*
            BoundExpression result;

            result = BindExpression(syntax);

            // check if expression ican be void
            if (result.Type != expectedResult)
            {
                diagnostics.reportCannotConvert(syntax.span, result.Type, expectedResult);
                return new BoundErrorExpression();
            }

            return result;
            */
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            BoundExpression expression;

            expression = BindExpression(syntax.Expression, true);

            return new BoundExpressionStatement(expression);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            BoundExpression expr;
            bool isNull;

            isNull = (syntax.Expression == null);

            // return with no function 
            if (function_ == null)
            {
                diagnostics.reportInvalidReturn(syntax.Location());
                return new BoundErrorStatement();
            }
            else
            {
                // return within a funcion
                // check if the funciton returns a null
                if (function_.Type == TypeSymbol.Void)
                {
                    // funciton returns void but return, returns a value 
                    if (isNull == false)
                    {
                        diagnostics.reportVoidFunctionReturnsValue(syntax.Expression.Location());
                        return new BoundErrorStatement();
                    }

                    // set returning expression to null
                    expr = null;
                }
                else
                {
                    // when function returns a value but return is returning a void
                    if (isNull == true)
                    {
                        diagnostics.reportFunctionReturnsNULL(syntax.Location(), function_.Type);
                        return new BoundErrorStatement();
                    }

                    // expression binding and type checking is handled here
                    expr = BindExpression(syntax.Expression);

                    // check return type
                    if (function_.Type != expr.Type)
                    {
                        diagnostics.reportCannotConvertType(syntax.Expression.Location(), expr.Type, function_.Type);
                        return new BoundErrorStatement();
                    }
                }
            }

            return new BoundReturnStatement(expr);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        // handel procedures and Built in Functions (BIF)
        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            IEnumerable<FunctionSymbol> BIF;
            ImmutableArray<BoundExpression>.Builder boundArguments;
            FunctionSymbol function;
            BoundExpression ar, argument;
            ParamiterSymbol parametr;
            TypeSymbol a2;
            TextSpan span;
            SyntaxNode firstNode;
            ExpresionSyntax lastNode;
            bool isCountGreater, a1, hasErrors = false;
            int argsCnt, fnParamsCnt;
            string name;

            // setup function call properties
            a1 = (syntax.Arguments.Count() == 1);
            a2 = (lookupType(syntax.FunctionName.sym.ToString()));


            name = syntax.FunctionName.sym.ToString();
            boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            if (a1 == true && a2 != null)
                return BindConversion(syntax.Arguments[0], a2);


            // get argument array
            // and create a bound argument list
            foreach (var args in syntax.Arguments)
            {
                ar = BindExpression(args);
                boundArguments.Add(ar);
            }
            argsCnt = boundArguments.Count();

            // check if function is a built-in function call
            // then save symbol in local function variable
            if (name.StartsWith("%") == true)
            {
                BIF = BuiltinFunctions.getAll();
                function = BIF.FirstOrDefault(f => f.Name == name);
            }
            else
            {
                scope.lookupFuncion(name, out function);
            }

            // user passed an unknown function
            if (function == null)
            {
                diagnostics.reportBadFunctionOrProcedure(syntax.FunctionName.Location(), name);

                return new BoundErrorExpression();
            }

            // check if user calling Procedure with EXSR
            if (function.isSubroutine == false && syntax.isExsrCall == true)
            {
                diagnostics.reportProcedureCalledWithExsr(syntax.FunctionName.Location(), name);

                return new BoundErrorExpression();
            }

            // check if user calling Subroutine like a Procedure
            if (function.isSubroutine == true && syntax.isExsrCall == false)
            {
                diagnostics.reportSubroutineCalledAsProcedure(syntax.FunctionName.Location(), name);

                return new BoundErrorExpression();
            }

            // prep for argument count check
            argsCnt = function.Paramiter.Length;
            fnParamsCnt = syntax.Arguments.Count();

            // check for increct number of argument errors
            if (fnParamsCnt != argsCnt)
            {
                isCountGreater = (fnParamsCnt > function.Paramiter.Length);

                //diagnostics.reportWrongArgumentCount(syntax.FunctionName.span, name, isCountGreater, argsCnt);
                if (function_ != null)
                {
                    if (function_.Paramiter.Length > argsCnt)
                    {
                        if (fnParamsCnt > 0)
                            firstNode = syntax.Arguments.getSeperatorAt(function_.Paramiter.Length - 1);
                        else
                            firstNode = syntax.Arguments[0];

                        lastNode = syntax.Arguments[fnParamsCnt - 1];
                        span = TextSpan.fromBounds(firstNode.span.START, lastNode.span.END);
                    }
                    else
                    {
                        span = syntax.CloseParen.span;
                    }
                }
                diagnostics.reportWrongArgumentCount(syntax.FunctionName.Location(), name, isCountGreater, argsCnt);


                return new BoundErrorExpression();
            }

            // bind arguments to function and check the argument types 
            // against the expected function paramiter types
            for (int i = 0; i < argsCnt; i++)
            {
                argument = boundArguments[i];
                parametr = function.Paramiter[i];

                // check argumet and paramiter types
                if (argument.Type != parametr.type)
                {
                    //diagnostics.reportFunctionParamiterTypeMismatch(syntax.Arguments[i].span, argument.Type, parametr.type);
                    //return new BoundErrorExpression();
                    if (argument.Type != TypeSymbol.ERROR)
                    {
                        diagnostics.reportFunctionParamiterTypeMismatch(syntax.Arguments[i].Location(), argument.Type, parametr.type);
                    }
                    hasErrors = true;
                }
            }

            if (hasErrors == true)
                return new BoundErrorExpression();

            // bind function fall
            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindConversion(ExpresionSyntax syntax, TypeSymbol type)
        {
            BoundExpression expression;

            expression = BindExpression(syntax);

            return BindConversion(expression, type, syntax.Location());
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindConversion(BoundExpression expression, TypeSymbol type, TextLocation lTxtLoc, bool isAssignment = false)
        {
            Conversion convertn;

            // get appropreat conversion
            if (isAssignment == false)
                convertn = Conversion.Clasifiyer(expression.Type, type);
            else
                convertn = Conversion.assignmentConvertion(expression.Type, type);

            // check if the conversion can happen
            if (convertn.Exits == false)
            {
                if (expression.Type != TypeSymbol.ERROR && type != TypeSymbol.ERROR)
                    diagnostics.reportCannotConvertType(lTxtLoc, expression.Type, type);
                return new BoundErrorExpression();
            }

            // types match, no need to convert
            if (convertn.Identity == true)
                return expression;

            return new BoundConversionExpression(type, expression);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        // bindLabel bind label
        private BoundStatement BindTagStatement(TagStatementSyntax syntax)
        {
            BoundLabel lbl;

            lbl = new BoundLabel(syntax.LableName);

            return new BoundLabelStatement(lbl);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindGoToStatement(GoToStatementSyntax syntax)
        {
            BoundLabel lbl;

            lbl = new BoundLabel(syntax.LableName);

            return new BoundGoToStatement(lbl);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindUntilStatement(UntilStatementSyntax syntax)
        {
            BoundExpression condition;
            BoundStatement body;
            BoundLabel breakLbl, continueLbl;

            condition = BindExpression(syntax.Condition, TypeSymbol.Indicator);
            body = bindLoopBody(syntax.Body, out breakLbl, out continueLbl);

            // loop error no condition was given
            if (condition == null || condition.Type == TypeSymbol.ERROR)
            {
                diagnostics.reportLoopWithoutCondition(syntax.Location(), syntax.Keyword.sym.ToString());
                return new BoundErrorStatement();
            }

            return new BoundUntilStatement(condition, body, breakLbl, continueLbl);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            string name = syntax.Identifier.sym.ToString();
            BoundExpression lBound;
            BoundExpression ubound;
            BoundExpression countBy;
            BoundStatement body;
            VariableSymbol variable = null;
            BoundLabel breakLbl, continueLbl;
            bool isCountUp;

            lBound = BindExpression(syntax.LowerBound);
            ubound = BindExpression(syntax.UpperBound);
            countBy = BindExpression(syntax.Increment);

            isCountUp = (syntax.Keyword_By.kind == TokenKind.TK_TO);

            // variable must be declared before the for loop is used
            if (scope.checkLocalVariables(name) == false)
            {
                diagnostics.reportVariableDoesNotExist(syntax.Identifier.Location(), name);
                return new BoundErrorStatement();
            }
            else
                variable = scope.variables[name];

            body = bindLoopBody(syntax.Body, out breakLbl, out continueLbl);

            return new BoundForStatement(variable, lBound, ubound, body, isCountUp, countBy, breakLbl, continueLbl);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            BoundExpression condition;
            BoundStatement body;
            BoundLabel breakLbl, continueLbl;

            condition = BindExpression(syntax.Condition, TypeSymbol.Indicator);
            body = bindLoopBody(syntax.Body, out breakLbl, out continueLbl);


            // loop error no condition was given
            if (condition == null || condition.Type == TypeSymbol.ERROR)
            {
                diagnostics.reportLoopWithoutCondition(syntax.Location(), syntax.Keyword.sym.ToString());
                return new BoundErrorStatement();
            }

            return new BoundWhileStatement(condition, body, breakLbl, continueLbl);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            BoundExpression condition;
            BoundStatement thenStatement;
            BoundStatement elseStatement;

            // set condition and then statments
            condition = BindExpression(syntax.Condition, TypeSymbol.Indicator);
            thenStatement = BindStatements(syntax.ThenStatement);

            if (condition == null || condition.Type == TypeSymbol.ERROR)
            {
                diagnostics.reportIfWithoutCondition(syntax.Location());
                return new BoundErrorStatement();
            }

            // set else statement
            if (syntax.ElseBlock != null)
                elseStatement = BindStatements(syntax.ElseBlock.ElseStatement);
            else
                elseStatement = null;

            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindBreakStatement(BreakStamentSyntax syntax)
        {
            BoundLabel breakLable;

            if (_loopStack.Count == 0)
            {
                diagnostics.reportLeaveOrIterWithoutLoop(syntax.Location(), "break");
                return BindErrorStatement();
            }

            breakLable = _loopStack.Peek().BreakLable;
            return new BoundGoToStatement(breakLable);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindContinueStatement(ContinueStamentSyntax syntax)
        {
            BoundLabel continueLable;

            if (_loopStack.Count == 0)
            {
                diagnostics.reportLeaveOrIterWithoutLoop(syntax.Location(), "break");
                return BindErrorStatement();
            }

            continueLable = _loopStack.Peek().ContinueLable;
            return new BoundGoToStatement(continueLable);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement bindLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLablel)
        {
            BoundStatement boundBody;
            int lblCnt = loopControlLableCnt;

            // create breake and continue labes for loop
            breakLabel = new BoundLabel($"^leave{lblCnt}");
            continueLablel = new BoundLabel($"^iter{lblCnt}");
            loopControlLableCnt += 1;

            // apply loop to stack and create loop body
            _loopStack.Push((breakLabel, continueLablel));
            boundBody = BindStatements(body);
            _loopStack.Pop();

            // return loop body
            return boundBody;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            //string name;
            //bool isReadOnly;
            BoundExpression expression;
            VariableSymbol variable;
            VariableDeclarationSyntax tvar = null;

            //name = syntax.Identifier.sym.ToString();
            //isReadOnly = (syntax.kind == TokenKind.TK_VARDCONST);
            //expression = BindExpression(syntax.Initilizer);
            //variable = new VariableSymbol(name, isReadOnly, expression.Type);


            expression = BindExpression(syntax.Initilizer);

            if (syntax.Keyword.kind == TokenKind.TK_VARDCONST)
            {
                tvar = new VariableDeclarationSyntax(syntax.STREE, syntax.Keyword, syntax.Identifier, expression.Type, null, null);
                variable = bindVariable(tvar, true);
            }
            else
            {
                variable = bindVariable(syntax, false);
            }

            //if (scope.declare(variable) == false)
            //{
            //    diagnostics.reportVariableAleadyDeclared(syntax.span, name);
            //    return new BoundErrorStatement();
            //}

            return new boundVariableDeclaration(variable, expression);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            ImmutableArray<BoundStatement>.Builder statements;
            BoundStatement statement;

            scope = new BoundScope(scope);
            statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var syntaxStatement in syntax.Statements)
            {
                statement = BindStatements(syntaxStatement);
                statements.Add(statement);
            }

            scope = scope.Parant;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression syntax)
        {
            return BindExpression(syntax.Expression);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindNamedExpression(NamedExpressionSyntax syntax)
        {
            string name = syntax.IDENTIFIERTOKEN.sym.ToString();
            VariableSymbol _variable;

            if (string.IsNullOrEmpty(name))
            {
                // token was inserted by the parcer and
                // error has already been reported: return error expresion
                return new BoundErrorExpression();
            }

            if (scope.lookup(name, out _variable) == false)
            {
                diagnostics.reportUndefinedName(syntax.IDENTIFIERTOKEN.Location(), name);
                return new BoundErrorExpression();
            }

            return new BoundVariableExpression(_variable);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            string name;
            BoundExpression boundExp;
            VariableSymbol _variable;
            BoundExpression convExpr;

            name = syntax.IDENTIFIERTOKEN.sym.ToString();
            boundExp = BindExpression(syntax.EXPRESSION);

            // check if the variable has been declared
            if (scope.lookup(name, out _variable) == false)
            {
                diagnostics.reportUndefinedName(syntax.IDENTIFIERTOKEN.Location(), name);
                return new BoundErrorExpression();
            }

            // check if varialbe is read only
            // do not do for PI, DS or PR blocks
            if (_variable.IsReadOnly == true &&  _variable.kind != SymbolKind.SYM_PARAMITER)
            {
                diagnostics.reportAssignmentOfConstantVar(syntax.IDENTIFIERTOKEN.Location(), name);
                return new BoundErrorExpression();
            }

            // check if the type of the variable matches its assigned expression
            // then bind the expression
            convExpr = BindConversion(boundExp, _variable.type, syntax.Location(), true);

            return new BoundAssignmentExpression(_variable, convExpr);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindLiteralExp(LiteralExpressionSyntax syntax)
        {
            object Value;
            BoundExpression ret;

            Value = syntax.value ?? 0;
            ret = new BoundLiteralExp(Value);

            return ret;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindUniExpression(UinaryExpressionSyntax syntax)
        {
            BoundExpression expression;
            //BoundUniOpToken? boundOperatorKind;
            BoundUniOperator boundOperatorKind;

            expression = BindExpression(syntax.right);
            boundOperatorKind = BoundUniOperator.bind(syntax.Operand.kind, expression.Type);

            // account for errors
            if (boundOperatorKind == null)
            {
                diagnostics.reportUndefinedUniaryOp(syntax.Operand.Location(), syntax.Operand.sym.ToString(), expression.Type);
                return new BoundErrorExpression();
            }

            return new BoundUniExpression(boundOperatorKind, expression);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindBinExpression(BinaryExpressionSyntax syntax)
        {
            BoundExpression left, right;
            BoundBinOperator boundOperatorKind;

            left = BindExpression(syntax.left);
            right = BindExpression(syntax.right);
            boundOperatorKind = BoundBinOperator.bind(syntax.operatorToken.kind, left.Type, right.Type);

            // check for errors
            if (left.Type == TypeSymbol.ERROR || right.Type == TypeSymbol.ERROR)
                return new BoundErrorExpression();

            // account for errors
            if (boundOperatorKind == null)
            {
                diagnostics.reportUndefinedBynaryOp(syntax.operatorToken.Location(), syntax.operatorToken.sym.ToString(), left.Type, right.Type);
                return new BoundErrorExpression();
            }

            return new BoundBinExpression(left, boundOperatorKind, right);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private VariableSymbol bindVariable(VariableDeclarationSyntax syntax, bool isConstant)
        {
            string name;
            VariableSymbol variable;
            TypeSymbol type;
            TypeClauseSyntax _type;

            // bind type only on variables NOT CONSTANTS
            if (isConstant == false)
            {
                _type = checkVarType(syntax.TypClas, new TextLocation(syntax.STREE.TEXT, syntax.span));
                type = bindTypeClause(_type);

                // check if variable has a nmae
                if (syntax == null || syntax.Identifier.kind == TokenKind.TK_BADTOKEN)
                {
                    diagnostics.reportVariableWithNoName(syntax.Location());
                    return null;
                }
            }
            else
                type = syntax.Typ;

            // Get variable name
            name = syntax.Identifier.sym.ToString();

            // identify scope
            if (function_ == null)
                variable = new GlobalVariableSymbol(name, isConstant, type);
            else
                variable = new LocalVariableSymbol(name, isConstant, type);

            // try to declare the variable 
            // function returns true when varialbe is added
            if (scope.declare(variable) == false)
            {
                diagnostics.reportVariableAlreadyDeclared(syntax.Location(), name);
                return null;
            }

            return variable;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private TypeSymbol bindTypeClause(TypeClauseSyntax typClas)
        {
            TypeSymbol ret;
            string typename;

            typename = typClas.Identifier.sym.ToString();
            ret = SyntaxFacts.lookupType(typename);

            if (ret == null)
            {
                diagnostics.reportUndefinedType(typClas.Location(), typename);
                return null;
            }

            return ret;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private TypeClauseSyntax checkVarType(TypeClauseSyntax typClas, TextLocation tloc)
        {
            if (typClas == null)
            {
                diagnostics.reportTypeNotGiven(tloc);
                return null;
            }

            return typClas;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private TypeSymbol lookupType(string name)
        {
            switch (name.ToUpper())
            {
                case "%CHAR":
                    return TypeSymbol.Char;
                case "%DATE":
                    return TypeSymbol.Date;
                case "%TIMESTAMP":
                    return TypeSymbol.DateTime;
                case "%FLOAT":
                    return TypeSymbol.Float;
                case "%INT":
                    return TypeSymbol.Integer;
                case "%TIME":
                    return TypeSymbol.Time;
                default:
                    return null;
            }
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }
    }

    public enum BoundNodeToken
    {
        BNT_UINEX,
        BNT_LITEX,
        BNT_VAREX,
        BNT_ASNEX,
        BNT_BINEX,
        BNT_BLOCKSTMT,
        BNT_EXPRSTMT,
        BNT_VARDECLR,
        BNT_IFSTMT,
        BNT_WHILESTMT,
        BNT_FORSTMT,
        BNT_DOUNTIL,
        BNT_GOTO,
        BNT_GOTOCOND,
        BNT_LABEL,
        BNT_ERROREXP,
        BNT_CALLEXP,
        BNT_CONVEXP,
        BNT_RETSTMT,
        BNT_NOOP
    }

    public enum BoundUniOpToken
    {
        BUO_IDENTITY,
        BUO_NEGATION,
        BUO_NOT
    }

    public enum BoundBinOpToken
    {
        BBO_ADD,
        BBO_SUB,
        BBO_MULT,
        BBO_DIV,

        BBO_AND,
        BBO_NOT,
        BBO_OR,
        BBO_GE,
        BBO_GT,
        BBO_LE,
        BBO_LT,
        BBO_EQ,
        BBO_NE
    }
}
