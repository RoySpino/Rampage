using rpgc.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class SyntaxTree
    {
        public CompilationUnit ROOT;
        //ImmutableArray<Diagnostics> diagnostics;
        DiagnosticBag diagnostics;
        public SyntaxToken EOFT;
        public SourceText text;

        private SyntaxTree(SourceText source)
        {
            Parser parcer;
            CompilationUnit root;
            DiagnosticBag diagnos = null;

            parcer = new Parser(source);
            root = parcer.parseCompilationUnit();
            diagnos = parcer.getDiagnostics();

            text = source;
            ROOT = root;
            EOFT = root.EndOfFileToken;
            diagnostics = diagnos;
        }

        // ///////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }

        // ///////////////////////////////////////////////////////////////////////
        // this calles the function below this one
        public static SyntaxTree parce(string text)
        {
            SourceText sourceText = SourceText.from(text);

            return parce(sourceText);
            //return new SyntaxTree(sourceText);
        }

        // ///////////////////////////////////////////////////////////////////////
        public static SyntaxTree parce(SourceText text)
        {
            return new SyntaxTree(text);
        }

        // ///////////////////////////////////////////////////////////////////////
        public static IEnumerable<SyntaxToken> parceToken(string text, out ImmutableArray<Diagnostics> _diagnostics)
        {
            SourceText txt = SourceText.from(text);
            return parceToken(txt, out _diagnostics);
        }

        // ///////////////////////////////////////////////////////////////////////
        public static IEnumerable<SyntaxToken> parceToken(SourceText Text)
        {
            Lexer lex = new Lexer(Text);
            SyntaxToken token;

            while (true)
            {
                token = lex.doLex();

                if (token.kind == TokenKind.TK_EOI)
                    break;

                yield return token;
            }
        }

        // ///////////////////////////////////////////////////////////////////////
        public static ImmutableArray<SyntaxToken> parceToken(SourceText Text, out ImmutableArray<Diagnostics> diagnostics)
        {
            Lexer lex = new Lexer(Text);
            ImmutableArray<SyntaxToken> EvaluationResult;

            EvaluationResult = lexToken(Text).ToImmutableArray();
            diagnostics = lex.getDiagnostics().ToImmutableArray();

            return EvaluationResult;
        }

        // ///////////////////////////////////////////////////////////////////////
        private static IEnumerable<SyntaxToken> lexToken(SourceText Text)
        {
            Lexer lex = new Lexer(Text);
            SyntaxToken token;

            while (true)
            {
                token = lex.doLex();

                if (token.kind == TokenKind.TK_EOI)
                    break;

                yield return token;
            }
        }
    }
}
