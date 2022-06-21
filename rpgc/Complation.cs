using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using rpgc.Binding;
using rpgc.Syntax;
using System.Collections.Immutable;
using System.IO;
using rpgc.Lowering;
using rpgc.Symbols;

namespace rpgc
{
    public class Complation
    {
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public Complation Previous { get; }
        public bool IsScript { get; } 
        private BoundGlobalScope _globalScope;

        /*
        public Complation(params SyntaxTree[] tree) : this(null, tree)
        {
        }
        */

        // //////////////////////////////////////////////////////////////////////////////////////////////
        private Complation(bool isScript, Complation prev, params SyntaxTree[] trees)
        {
            SyntaxTrees = trees.ToImmutableArray();
            Previous = prev;
            IsScript = isScript;
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        public static Complation Create(params SyntaxTree[] trees)
        {
            return new Complation(false, null, trees);
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        public static Complation CreateScript(Complation prev, params SyntaxTree[] trees)
        {
            return new Complation(true, prev, trees);
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        private BoundProgram getProgram()
        {
            BoundProgram perv;

            if (Previous == null)
                perv = null;
            else
                perv = Previous.getProgram();

            return Binder.BindProgram(IsScript, perv, globalScope_);
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        internal BoundGlobalScope globalScope_
        {
            get
            {
                BoundGlobalScope gs;

                if (_globalScope == null)
                {
                    /*
                    if (previous != null)
                        gs = previous.globalScope_;
                    else
                        gs = null;
                    */

                    gs = Binder.bindGlobalScope(IsScript, Previous?.globalScope_, SyntaxTrees);
                    //gs = Binder.bindGlobalScope(gs, Syntax.ROOT);
                    Interlocked.CompareExchange(ref _globalScope, gs, null);
                }
                return _globalScope;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        internal void emitTree(TextWriter writer)
        {
            bool isMain, isScript;

            isMain = _globalScope.MainFunction != null;
            isScript = _globalScope.ScriptFunciton != null;

            if (isMain == true && isScript == false)
                emitTree(_globalScope.MainFunction, writer);
            else
                emitTree(_globalScope.ScriptFunciton, writer);
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        internal void emitTree(FunctionSymbol symbol,TextWriter writer)
        {
            BoundProgram program = getProgram();
            BoundBlockStatement body;

            symbol.writeTo(writer);
            writer.WriteLine();

            if (program.Functions.TryGetValue(symbol, out body) == false)
                return;

            body.writeTo(writer);
        }


        // //////////////////////////////////////////////////////////////////////////////////////////////
        /*
        private BoundBlockStatement getStatement()
        {
            //BoundStatement stmnt;
            BoundBlockStatement flattend;

            flattend = Lowerer.Lower(_globalScope.Statement);
            _globalScope.Statement = flattend;

            return flattend;
        }
        */

        // //////////////////////////////////////////////////////////////////////////////////////////////
        /*
        public Complation continueWith(SyntaxTree tree)
        {
            return new Complation(this, tree);
        }
        */

        FunctionSymbol MainFunction()
        {
            return globalScope_.MainFunction;
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        public EvaluationResult evalate(Dictionary<VariableSymbol, object> _variables)
        {
            ImmutableArray<Diagnostics> diognos;
            BoundProgram program;
            Evaluator eval;
            object value;
            IEnumerable<Diagnostics> parseDiagno;

            // collect all diagnostics from all syntax trees
            parseDiagno = SyntaxTrees.SelectMany(stre => stre.Diagnostics);

            // call property {globalScope_} to bind syntax tree
            if (globalScope_.Diagnostic != null)
                diognos = parseDiagno.Concat(globalScope_.Diagnostic).ToImmutableArray();
            else
                diognos = parseDiagno.ToImmutableArray();

            if (diognos.Any())
                return new EvaluationResult(diognos, null);

            program = getProgram();


            diognos = program.Diagnostics;
            if (diognos.Any())
                return new EvaluationResult(diognos, null);

            BoundStatement[] xtn = (from vars in program.GblScope.Statements
                       where vars.tok == BoundNodeToken.BNT_VARDECLR
                       select vars).ToArray();


            foreach (boundVariableDeclaration vrs in xtn)
            {
                _variables.Add(vrs.Variable, vrs.Initalizer);
            }

            //st = getStatement();
            eval = new Evaluator(program, _variables);
            //eval = new Evaluator(program.FunctionBodies, st, _variables);
            ////eval = new Evaluator(st, _variables);
            value = eval.Evaluate();

            return new EvaluationResult(ImmutableArray<Diagnostics>.Empty, value);
        }
    }
}
