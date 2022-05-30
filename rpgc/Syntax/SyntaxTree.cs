using rpgc.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class SyntaxTree
    {
        private Dictionary<SyntaxNode, SyntaxNode> _parents;

        private delegate void ParseHandler(SyntaxTree syntaxTree,
                                           out CompilationUnit root,
                                           out ImmutableArray<Diagnostics> diagnostics);


        public SourceText TEXT { get; }
        public ImmutableArray<Diagnostics> Diagnostics { get; }
        public CompilationUnit ROOT { get; }
        public bool IsScript { get; }
        public SyntaxToken EOFT;
        static bool includeEndOfFile_;
        static List<SyntaxToken> tokens;

        private SyntaxTree(SourceText text, ParseHandler handler, bool _isscript)
        {
            TEXT = text;
            CompilationUnit rt;
            ImmutableArray<Diagnostics> diag;

            IsScript = _isscript;

            // calls Parcer
            handler(this, out rt, out diag);

            Diagnostics = diag;
            ROOT = rt;
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // first Parse call 
        public static SyntaxTree Parse(string text, string fName="")
        {
            SourceText sourceText_;

            sourceText_ = SourceText.FROM(text, fName);

            return Parse(sourceText_);
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // second Parse call 
        public static SyntaxTree Parse(SourceText text, bool _isScript = false)
        {
            return new SyntaxTree(text, Parse, _isScript);
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Third Parse call
        private static void Parse(SyntaxTree syntaxTree, out CompilationUnit root, out ImmutableArray<Diagnostics> diagnostics)
        {
            Parser par;

            par = new Parser(syntaxTree);
            root = par.parseCompilationUnit();
            diagnostics = par.getDiagnostics().ToImmutableArray();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /*
        public static SyntaxTree Load(string fileName)
        {
            string text;
            SourceText sourceText_;

            text = System.IO.File.ReadAllText(fileName);
            sourceText_ = SourceText.FROM(text, fileName);

            return Parse(sourceText_);
        }
        */

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /*
        public static ImmutableArray<SyntaxToken> ParseTokens(string text, bool includeEndOfFile = false)
        {
            SourceText sourceText_;

            sourceText_ = SourceText.FROM(text);
            
            return ParseTokens(sourceText_, includeEndOfFile);
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostics> diagnostics, bool includeEndOfFile = false)
        {
            SourceText sourceText_;
            
            sourceText_ = SourceText.FROM(text);
            
            return ParseTokens(sourceText_, out diagnostics, includeEndOfFile);
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, bool includeEndOfFile = false)
        {
            return ParseTokens(text, out _, includeEndOfFile);
        }
        */

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /**************************************
         * ************************************
        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Diagnostics> diagnostics, bool includeEndOfFile = false)
        {
            SyntaxTree syntaxTree;

            includeEndOfFile_ = includeEndOfFile;

            syntaxTree = new SyntaxTree(text, ParseTokens);
            diagnostics = syntaxTree.Diagnostics.ToImmutableArray();

            return tokens.ToImmutableArray();
        }
        */

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        static void ParseTokens(SyntaxTree st, out CompilationUnit root, out ImmutableArray<Diagnostics> d)
        {
            List<SyntaxToken> Tokens_;
            Lexer lex;
            SyntaxToken token;

            Tokens_ = new List<SyntaxToken>();
            lex = new Lexer(st);

            while (true)
            {
                token = lex.doLex();

                if (token.kind != TokenKind.TK_EOI || includeEndOfFile_)
                    Tokens_.Add(token);

                if (token.kind == TokenKind.TK_EOI)
                {
                    root = new CompilationUnit(st, ImmutableArray<MemberSyntax>.Empty, token);
                    break;
                }
            }

            d = lex.getDiagnostics().ToImmutableArray();
            tokens = Tokens_;
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal SyntaxNode GetParent(SyntaxNode syntaxNode)
        {
            Dictionary<SyntaxNode, SyntaxNode> parents;

            if (_parents == null)
            {
                parents = CreateParentsDictionary(ROOT);
                Interlocked.CompareExchange(ref _parents, parents, null);
            }

            return _parents[syntaxNode];
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Dictionary<SyntaxNode, SyntaxNode> CreateParentsDictionary(CompilationUnit root)
        {
            Dictionary<SyntaxNode, SyntaxNode> result;

            result = new Dictionary<SyntaxNode, SyntaxNode>();
            result.Add(root, null);
            CreateParentsDictionary(result, root);

            return result;
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void CreateParentsDictionary(Dictionary<SyntaxNode, SyntaxNode> result, SyntaxNode node)
        { 
            foreach (SyntaxNode child in node.getCildren())
            {
                result.Add(child, node);
                CreateParentsDictionary(result, child);
            }
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ImmutableArray<Diagnostics> getDiagnostics()
        {
            return Diagnostics;
        }
    }
}
