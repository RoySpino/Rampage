using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Binding;

namespace rpgc
{
    public class Complation
    {
        public SyntaxTree Syntax;
        DiagnosticBag diognost = new DiagnosticBag();

        public Complation(SyntaxTree tree)
        {
            Syntax = tree;
            diognost.AddRange(tree.getDiagnostics());
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////
        public EvaluationResult evalate(Dictionary<VariableSymbol, object> _variables)
        {
            Binder bindr = new Binder(_variables);
            BoundExpression boundExpr = bindr.BindExpression(Syntax.ROOT);

            diognost.AddRange(bindr.diagnostics);

            if (diognost.Any())
                return new EvaluationResult(diognost, null);

            Evaluator eval = new Evaluator(boundExpr, _variables);
            var value = eval.Evaluate();

            return new EvaluationResult(null, value);
        }
    }

    // ////////////////////////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////////////////////////

    public sealed class Diagnostics
    {
        public TextSpan SPAN;
        string MESSAGE;

        public Diagnostics(TextSpan span, string message)
        {
            SPAN = span;
            MESSAGE = message;
        }

        public override string ToString()
        {
            return MESSAGE;
        }
    }

    // ////////////////////////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////////////////////////

    public sealed class DiagnosticBag : IEnumerable<Diagnostics>
    {
        private List<Diagnostics> _diagnostic = new List<Diagnostics>();

        public void report(TextSpan span, string message)
        {
            Diagnostics diag = new Diagnostics(span, message);
            _diagnostic.Add(diag);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void report(string message, int start, int len)
        {
            Diagnostics diag = new Diagnostics(new TextSpan(start, len), message);
            _diagnostic.Add(diag);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportInvalidNumber(string symbol, Type typVal, int start, int len)
        {
            string message;

            message = string.Format("rpgc: the symbol {0} is not a valid {1}", symbol, typVal);

            report(message, start, len);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadCharacter(char symbol, int position)
        {
            string message;

            message = string.Format("rpgc: bad character imput", symbol);

            report(message, (position - 1), 1);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUnexpectedToken(TextSpan span, TokenKind actual, TokenKind expected)
        {
            string message;

            message = string.Format("rpgc: unexpected token [{0}] expected [{1}]", actual, expected);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedUniaryOp(TextSpan span, string opSym, Type opType)
        {
            string message;

            message = string.Format("rpgc: uninary operator [{0}] is not defined for type {1}", opSym, opType);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedBynaryOp(TextSpan span, string opSym, Type LeftType, Type RightType)
        {
            string message;

            message = string.Format("rpgc: binnary operator [{0}] is not defined for types {1} and {2}", opSym, LeftType, RightType);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedName(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc: the variable [{0}] is not defined", name);

            report(span, message);
        }
        // //////////////////////////////////////////////////////////////////////////
        public void reportMissingFactor1(TextSpan span, int lp)
        {
            string message;

            message = string.Format("rpgc: factor 1 without Key word on line {0}", lp);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportNotLeftJustified(TextSpan span, int factor, int lp)
        {
            string message;

            message = string.Format("rpgc({1},{2}): factor {0} is not left justified", factor, lp, span.START);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportNotRightJustified(TextSpan span, int factor, int lp)
        {
            string message;

            message = string.Format("rpgc({1},{2}): factor {0} is not right justifide", factor, lp, span.START);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadFactor(TextSpan span, int factor, int lp)
        {
            string message;

            message = string.Format("rpgc({1},{2}): factor {0} is not empty", factor, lp, span.START);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadSpec(char symbol, int linePosition)
        {
            string message;

            message = string.Format("rpgc({1},1): unknown specification [{0}]", symbol, linePosition);

            report(new TextSpan(0, 1), message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadOpcode(string symbol, int linePosition)
        {
            string message;

            message = string.Format("rpgc({1},21): unknown Operation Code [{0}]", symbol, linePosition);

            report(new TextSpan(20, symbol.Length), message);
        }
        

        // //////////////////////////////////////////////////////////////////////////
        public IEnumerator<Diagnostics> GetEnumerator()
        {
            return _diagnostic.GetEnumerator();
        }

        // //////////////////////////////////////////////////////////////////////////
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // //////////////////////////////////////////////////////////////////////////
        public void AddRange(DiagnosticBag a)
        {
            if (a != null)
                _diagnostic.AddRange(a._diagnostic);
        }

        public void Clear()
        {
            _diagnostic.Clear();
        }
    }
}
