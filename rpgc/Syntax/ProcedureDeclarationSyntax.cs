using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class ProcedureDeclarationSyntax : MemberSyntax
    {
        public override TokenKind kind => TokenKind.TK_PROCDCL;
        public SyntaxToken DeclareKeyWord { get; }
        public SyntaxToken ProcedureName { get; }
        public SyntaxToken ProcInterface { get; }
        public SyntaxToken IdentfirePN { get; }
        public TypeClauseSyntax ReturnType { get; }
        public SeperatedParamiterList<ParamiterSyntax> Paramiters { get; }
        public SyntaxToken EndInterfaceToken { get; }
        public StatementSyntax Body { get; }
        public bool isSubroutine { get; }

        public ProcedureDeclarationSyntax(SyntaxToken keywrd,
                                          SyntaxToken identfire,
                                          SyntaxToken procInterface,
                                          SyntaxToken identfirePN,
                                          TypeClauseSyntax retType,
                                          SeperatedParamiterList<ParamiterSyntax> parm,
                                          SyntaxToken endInterfacceToken,
                                          StatementSyntax procBody,
                                          bool isSub)
        {
            DeclareKeyWord = keywrd;
            ProcedureName = identfire;
            ProcInterface = procInterface;
            IdentfirePN = identfirePN;
            Paramiters = parm;
            EndInterfaceToken = endInterfacceToken;
            Body = procBody;
            isSubroutine = isSub;

            if (retType == null || isSub == true)
                ReturnType = new TypeClauseSyntax(new SyntaxToken(TokenKind.TK_IDENTIFIER, 0, 0, "void"));
            else
                ReturnType = retType;
        }
    }
}
