using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace rpgc.Syntax
{
    public sealed class SeperatedSyntaxList<T> : SeperatedSyntaxList, IEnumerable<T> where T : SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> SeperatorsAndNodes;

        public SeperatedSyntaxList(ImmutableArray<SyntaxNode> lst)
        {
            SeperatorsAndNodes = lst;
        }

        // ///////////////////////////////////////////////////////////////////
        public int Count()
        {
            return (SeperatorsAndNodes.Length + 1) >> 1;
        }

        // ///////////////////////////////////////////////////////////////////
        public T this[int index] => (T)SeperatorsAndNodes[index * 2];

        // ///////////////////////////////////////////////////////////////////
        public T getParamiterAt(int idx)
        {
            return (T)SeperatorsAndNodes[idx * 2];
        }

        // ///////////////////////////////////////////////////////////////////
        public SyntaxToken getSeperatorAt(int idx)
        {
            if (idx == (Count() - 1))
                return null;

            return (SyntaxToken)SeperatorsAndNodes[idx * 2 + 1];
        }

        // ///////////////////////////////////////////////////////////////////
        public override ImmutableArray<SyntaxNode> getWithSeperators()
        {
            return SeperatorsAndNodes;
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
                yield return this[i];
            }
        }
    }

    // //////////////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////     /////
    // ////////////////////////////////////////////////////////////////////////////////////

    public abstract class SeperatedSyntaxList
    {
        public SeperatedSyntaxList()
        {
            //
        }

        // ///////////////////////////////////////////////////////////////////
        public abstract ImmutableArray<SyntaxNode> getWithSeperators();
    }

}
