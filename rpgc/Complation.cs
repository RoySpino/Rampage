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

namespace rpgc
{
    public class Complation
    {
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        private DiagnosticBag diognost = new DiagnosticBag();
        private BoundGlobalScope _globalScope;
        private Complation previous;

        public Complation(params SyntaxTree[] tree) : this(null, tree)
        {
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        private Complation(Complation prev, params SyntaxTree[] trees)
        {
            SyntaxTrees = trees.ToImmutableArray();
            previous = prev;
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

                    gs = Binder.bindGlobalScope(previous?.globalScope_, SyntaxTrees);
                    //gs = Binder.bindGlobalScope(gs, Syntax.ROOT);
                    Interlocked.CompareExchange(ref _globalScope, gs, null);
                }
                return _globalScope;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        internal void emitTree(TextWriter writer)
        {
            BoundStatement stmnt = getStatement();
            _globalScope.Statement.writeTo(writer);
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        private BoundBlockStatement getStatement()
        {
            //BoundStatement stmnt;
            BoundBlockStatement flattend;

            flattend = Lowerer.Lower(_globalScope.Statement);
            _globalScope.Statement = flattend;

            return flattend;
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        public Complation continueWith(SyntaxTree tree)
        {
            return new Complation(this, tree);
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        public EvaluationResult evalate(Dictionary<VariableSymbol, object> _variables)
        {
            ImmutableArray<Diagnostics> diognos;
            BoundBlockStatement st;
            BoundProgram program;
            Evaluator eval;
            object value;
            IEnumerable<Diagnostics> parseDiagno;

            // collect all diagnostics from all syntax trees
            parseDiagno = SyntaxTrees.SelectMany(stre => stre.Diagnostics);

            // call property {globalScope_} to bind syntax tree
            diognos = parseDiagno.Concat(globalScope_.Diagnostic).ToImmutableArray();

            if (diognos.Any())
                return new EvaluationResult(diognos, null);

            program = Binder.BindProgram(globalScope_);

            diognos = program.Diagnostics;
            if (diognos.Any())
                return new EvaluationResult(diognos, null);

            st = getStatement();
            eval = new Evaluator(program.FunctionBodies, st, _variables);
            //eval = new Evaluator(st, _variables);
            value = eval.Evaluate();

            return new EvaluationResult(ImmutableArray<Diagnostics>.Empty, value);
        }
    }
}
