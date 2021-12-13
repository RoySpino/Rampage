using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    public abstract class BoundNode
    {
        public abstract BoundNodeToken tok { get; }

        // /////////////////////////////////////////////////////////////////////////
        public IEnumerable<BoundNode> getCildren()
        {
            PropertyInfo[] properties;
            IEnumerable<BoundNode> children = null;
            BoundNode chd;

            properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(prop.PropertyType))
                {
                    chd = (BoundNode)prop.GetValue(this);
                    if (chd != null)
                        yield return chd;
                }
                else
                {
                    if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType))
                    {
                        children = (IEnumerable<BoundNode>)prop.GetValue(this);
                        foreach (BoundNode child in children)
                        {
                            if (child != null)
                                yield return child;
                        }
                    }
                }
            }
        }


        // /////////////////////////////////////////////////////////////////////////
        public IEnumerable<(string name, object value)> getProperties()
        {
            PropertyInfo[] properties;
            object value;

            properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == nameof(tok) || 
                    prop.Name == nameof(BoundBinExpression.OP))
                    continue;

                if (typeof(BoundNode).IsAssignableFrom(prop.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType))
                {
                    continue;
                }

                value = prop.GetValue(this);
                if (value != null)
                    yield return (prop.Name, value);
            }
        }

        // /////////////////////////////////////////////////////////////////////////
        private static void printTree(System.IO.TextWriter textWriter, BoundNode node, string indent = "", bool isLast = true)
        {
            BoundNode lastChild;
            string marker;
            string text;
            bool isToConsole, isFirstProp;
            IEnumerable<BoundNode> xtmp;

            if (node == null)
                return;

            isToConsole = textWriter == Console.Out;
            marker = ((isLast == true) ? "└──" : "├──");

            if (isToConsole == true)
                Console.ForegroundColor = ConsoleColor.Gray;

            textWriter.Write(indent + marker);

            Console.ForegroundColor = getConsoleColor(node);
            text = getText(node);
            textWriter.Write(text);
            Console.ResetColor();

            isFirstProp = true;

            foreach ((string name, object value) v in node.getProperties())
            {
                if (isFirstProp == true)
                    isFirstProp = false;
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    textWriter.Write(",");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                textWriter.Write(" " + v.name);
                Console.ForegroundColor = ConsoleColor.Gray;
                textWriter.Write(" = ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                textWriter.Write(v.value);
            }
            textWriter.WriteLine("");

            indent += ((isLast == true) ? "   " : "│  ");

            lastChild = node.getCildren().LastOrDefault();
            xtmp = node.getCildren();

            foreach (BoundNode child in xtmp)
                printTree(textWriter, child, indent, child == lastChild);
        }

        // /////////////////////////////////////////////////////////////////////////
        private static string getText(BoundNode node)
        {
            BoundBinExpression t1;
            BoundUniExpression u1;

            if (node is BoundBinExpression)
            {
                t1 = (BoundBinExpression)node;
                return t1.OP.tok.ToString() + "_Expression";
            }
            if (node is BoundUniExpression)
            {
                u1 = (BoundUniExpression)node;
                return u1.OP.tok.ToString() + "_Expression";
            }

            return node.tok.ToString();
        }

        // /////////////////////////////////////////////////////////////////////////
        private static ConsoleColor getConsoleColor(BoundNode node)
        {
            if (node is BoundExpression)
                return ConsoleColor.DarkCyan;
            if (node is BoundStatement)
                return ConsoleColor.Magenta;

            return ConsoleColor.Gray;
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
