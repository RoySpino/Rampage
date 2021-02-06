using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public sealed class SyntaxTree
    {
        public ExpresionSyntax ROOT;
        DiagnosticBag diagnostics = new DiagnosticBag();
        public SyntaxToken EOFT;

        public SyntaxTree(DiagnosticBag _diagnostics, ExpresionSyntax root, SyntaxToken EndOfFileToken)
        {
            ROOT = root;
            EOFT = EndOfFileToken;
            diagnostics.AddRange(_diagnostics);
        }

        // ///////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }

        // ///////////////////////////////////////////////////////////////////////
        public static SyntaxTree parce(string text)
        {
            Parser paar = new Parser(text);

            return paar.parse();
        }

        // ///////////////////////////////////////////////////////////////////////
        public static SyntaxToken[] parceToken(TokenKind kind, string Text)
        {
            return null;
        }
    }
}
