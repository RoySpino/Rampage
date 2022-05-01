using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace rpgc.Syntax
{
    public sealed class SeperatedParamiterList<T> : SeperatedParamiterList, IEnumerable<T> where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> SeperatorsAndNodes;

        public SeperatedParamiterList(ImmutableArray<SyntaxNode> lst)
        {
            SeperatorsAndNodes = lst;
        }

        // ///////////////////////////////////////////////////////////////////
        public int Count()
        {
            return SeperatorsAndNodes.Length;
        }
        // ///////////////////////////////////////////////////////////////////
        public T getParamiterAt(int idx)
        {
            return (T)SeperatorsAndNodes[idx];
        }

        // ///////////////////////////////////////////////////////////////////
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // ///////////////////////////////////////////////////////////////////
        public IEnumerator<T> GetEnumerator()
        {
            int cnt = Count();
            for (int i = 0; i < cnt; i++)
            {
                yield return getParamiterAt(i);
            }
        }
        // ///////////////////////////////////////////////////////////////////
        public override ImmutableArray<SyntaxNode> getAllParamiters()
        {
            throw new NotImplementedException();
        }
    }


    // //////////////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////     /////
    // ////////////////////////////////////////////////////////////////////////////////////
    public abstract class SeperatedParamiterList
    {
        public SeperatedParamiterList()
        {
            //
        }

        // ///////////////////////////////////////////////////////////////////
        public abstract ImmutableArray<SyntaxNode> getAllParamiters();
    }
}
