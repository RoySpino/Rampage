﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Immutable;

namespace rpgc.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract TokenKind kind { get; }

        // /////////////////////////////////////////////////////////////////////////
        public IEnumerable<SyntaxNode> getCildren()
        {
            PropertyInfo[] properties;
            IEnumerable<SyntaxNode> children = null;
            SyntaxNode chd;
            SeperatedSyntaxList lst;

            properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(prop.PropertyType))
                {
                    chd = (SyntaxNode)prop.GetValue(this);
                    if (chd != null)
                        yield return chd;
                }
                else
                {
                    if (typeof(SeperatedSyntaxList).IsAssignableFrom(prop.PropertyType))
                    {
                        lst = (SeperatedSyntaxList)prop.GetValue(this);

                        foreach (SyntaxNode arg in lst.getWithSeperators())
                            yield return arg;
                    }
                    else
                    {
                        if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(prop.PropertyType))
                        {
                            children = (IEnumerable<SyntaxNode>)prop.GetValue(this);
                            foreach (SyntaxNode child in children)
                            {
                                if (child != null)
                                    yield return child;
                            }
                        }
                    }
                }
            }
        }

        // /////////////////////////////////////////////////////////////////////////
        public virtual TextSpan span
        {
            get
            {
                TextSpan first = getCildren().First().span;
                TextSpan last = getCildren().First().span;
                return TextSpan.fromBounds(first.START, last.END, first.LineNo, first.LinePos);
            }
        }

        // /////////////////////////////////////////////////////////////////////////
        public SyntaxToken GetLastToken()
        {
            if (this is SyntaxToken token)
                return token;

            // A syntax node should always contain at least 1 token.
            return getCildren().Last().GetLastToken();
        }

        // /////////////////////////////////////////////////////////////////////////
        private static void printTree(TextWriter textWriter, SyntaxNode node, string indent = "", bool isLast = true)
        {
            SyntaxNode lastChild;

            if (node == null)
                return;

            string marker = ((isLast == true) ? "└──" : "├──");
            textWriter.Write(indent + marker + node.kind);

            if (node is SyntaxToken)
            {
                SyntaxToken t = (SyntaxToken)node;
                textWriter.Write(" " + t.sym);
            }

            textWriter.WriteLine("");

            indent += ((isLast == true) ? "   " : "│  ");

            lastChild = node.getCildren().LastOrDefault();

            foreach (SyntaxNode child in node.getCildren())
                printTree(textWriter, child, indent, child == lastChild);
        }

        // /////////////////////////////////////////////////////////////////////////
        public void writeTo(TextWriter writer)
        {
            printTree(writer, this);
        }

        // /////////////////////////////////////////////////////////////////////////
        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                writeTo(writer);
                return writer.ToString();
            }
        }

    }
}