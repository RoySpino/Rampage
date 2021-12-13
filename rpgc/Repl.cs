using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using rpgc.Syntax;

namespace rpgc
{
    internal abstract class Repl
    {
        public string submitionText;
        protected bool doShowTree;
        protected bool doShowProgramTree;
        protected bool LEAVE;
        protected Dictionary<VariableSymbol, object> variables;
        protected SyntaxTree stree;
        protected readonly StringBuilder sbuilder = new StringBuilder();
        string outLn;
        private readonly List<string> _submissionHistory = new List<string>();
        private int _submissionHistoryIndex;
        // public ObservableCollection<string> _document = new ObservableCollection<string>()

        // //////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////
        // ////////////////////////////////////////////////////////////////////////////////////

        private delegate object LineRenderHandler(IReadOnlyList<string> lines, int lineIndex, object state);

        private class SubmitionView
        {
            private readonly LineRenderHandler _lineRenderer;
            private readonly ObservableCollection<string> _document;
            private int _top;
            private int _maxRenderLine = 0;
            private string blankLine;
            private int _currentLineIndex;
            private int _currentChar;

            public int currentLineIndex
            {
                get { return _currentLineIndex; }
                set
                {
                    if (currentLineIndex != value)
                    {
                        _currentLineIndex = value;
                        updateCursorPosition();
                    }
                }
            }
            public int currentChar
            {
                get { return _currentChar; }
                set
                {
                    if (_currentChar != value)
                    {
                        _currentChar = value;
                        updateCursorPosition();
                    }
                }
            }

            // /////////////////////////////////////////////////////////////////////////////////////
            public SubmitionView(LineRenderHandler lineRenderer, ObservableCollection<string> doc)
            {
                _lineRenderer = lineRenderer;
                _document = doc;
                _document.CollectionChanged += submitionDocumentChanged;
                _top = Console.CursorTop;
                render();
            }

            // /////////////////////////////////////////////////////////////////////////////////////
            private void submitionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                render();
            }

            // /////////////////////////////////////////////////////////////////////////////////////
            private void render()
            {
                int blankLineCount;
                int lineCnt = 0;
                object state = null;

                Console.CursorVisible = false;
                Console.SetCursorPosition(0, _top);
                lineCnt = 0;

                foreach (string line in _document)
                {
                    if (_top + lineCnt >= Console.WindowHeight)
                    {
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.WriteLine();

                        if (_top > 0)
                            _top -= 1;
                    }

                    Console.SetCursorPosition(0, _top + lineCnt);
                    Console.ForegroundColor = ConsoleColor.Green;

                    if (lineCnt == 0)
                        Console.Write(">>> ");
                    else
                        Console.Write("  | ");

                    Console.ResetColor();
                    state = _lineRenderer(_document, lineCnt, state);
                    Console.WriteLine(new string(' ', Console.WindowWidth - line.Length - 4));
                    lineCnt += 1;
                }

                blankLineCount = _maxRenderLine - lineCnt;
                if (blankLineCount > 0)
                {
                    blankLine = new string(' ', Console.WindowWidth);
                    for (int i = 0; i < blankLineCount; i++)
                    {
                        Console.SetCursorPosition(0, _top + lineCnt + i);
                        Console.WriteLine(blankLine);
                    }
                }

                _maxRenderLine = lineCnt;
                Console.CursorVisible = true;
                updateCursorPosition();
            }

            // /////////////////////////////////////////////////////////////////////////////////////
            private void updateCursorPosition()
            {
                Console.CursorTop = _top + currentLineIndex;
                Console.CursorLeft = 4 + currentChar;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////
        // ////////////////////////////////////////////////////////////////////////////////////

        public void run()
        {
            outLn = "";
            stree = null;
            LEAVE = false;
            variables = new Dictionary<VariableSymbol, object>();

            while (true)
            {
                outLn = editSubmition();

                if (String.IsNullOrEmpty(outLn))
                    continue;

                if (outLn.Contains(Environment.NewLine) == false && outLn.StartsWith("!"))
                {
                    processMetaCommand(outLn);
                    continue;
                }
                else
                    evaluatePgm(outLn);

                _submissionHistory.Add(outLn);
                _submissionHistoryIndex = 0;
            }
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private string editSubmition()
        {
            ObservableCollection<string> doc = new ObservableCollection<string>() { "" };
            SubmitionView submitView = new SubmitionView(RenderLine, doc);
            ConsoleKeyInfo key;

            submitionText = null;
            LEAVE = false;

            while (LEAVE == false)
            {
                key = Console.ReadKey(true);
                handleKey(key, doc, submitView);
            }

            submitView.currentLineIndex = doc.Count - 1;
            submitView.currentChar = doc[submitView.currentLineIndex].Length;
            Console.WriteLine();

            return string.Join(Environment.NewLine, doc);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleKey(ConsoleKeyInfo key, ObservableCollection<string> doc, SubmitionView submitView)
        {
            if (key.Modifiers == default(ConsoleModifiers))
            {
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        handleEscape(doc, submitView);
                        break;
                    case ConsoleKey.Enter:
                        handleEnter(doc, submitView);
                        break;
                    case ConsoleKey.LeftArrow:
                        handleDocumentLeft(doc, submitView);
                        break;
                    case ConsoleKey.RightArrow:
                        handleDocumentRight(doc, submitView);
                        break;
                    case ConsoleKey.UpArrow:
                        handleDocumentUp(doc, submitView);
                        break;
                    case ConsoleKey.DownArrow:
                        handleDocumentDown(doc, submitView);
                        break;
                    case ConsoleKey.Backspace:
                        handleBackspace(doc, submitView);
                        break;
                    case ConsoleKey.Delete:
                        handleDelete(doc, submitView);
                        break;
                    case ConsoleKey.Home:
                        handleHome(doc, submitView);
                        break;
                    case ConsoleKey.End:
                        handleEnd(doc, submitView);
                        break;
                    case ConsoleKey.Tab:
                        handleTab(doc, submitView);
                        break;
                    case ConsoleKey.PageUp:
                        handlePageUp(doc, submitView);
                        break;
                    case ConsoleKey.PageDown:
                        handlePageDown(doc, submitView);
                        break;
                }
            }
            else
            {
                if (key.Modifiers == ConsoleModifiers.Control)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.Enter:
                            handleControlEnter(doc, submitView);
                            break;
                    }
                }
            }

            if (key.Key != ConsoleKey.Backspace && key.KeyChar >= ' ')
                handlTyping(doc, submitView, key.KeyChar.ToString());
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleControlEnter(ObservableCollection<string> doc, SubmitionView submitView)
        {
            insertLine(doc, submitView);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handlePageDown(ObservableCollection<string> doc, SubmitionView submitView)
        {
            _submissionHistoryIndex++;
            
            if (_submissionHistoryIndex > _submissionHistory.Count - 1)
                _submissionHistoryIndex = 0;

            UpdateDocumentFromHistory(doc, submitView);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handlePageUp(ObservableCollection<string> doc, SubmitionView submitView)
        {
            _submissionHistoryIndex--;

            if (_submissionHistoryIndex < 0)
                _submissionHistoryIndex = _submissionHistory.Count - 1;
            
            UpdateDocumentFromHistory(doc, submitView);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleTab(ObservableCollection<string> doc, SubmitionView submitView)
        {
            const int TabWidth = 4;
            int start, remainingSpaces;
            string line;

            start = submitView.currentChar;
            remainingSpaces = TabWidth - start % TabWidth;
            line = doc[submitView.currentLineIndex];
            doc[submitView.currentLineIndex] = line.Insert(start, new string(' ', remainingSpaces));
            submitView.currentChar += remainingSpaces;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleEnd(ObservableCollection<string> doc, SubmitionView submitView)
        {
            submitView.currentChar = doc[submitView.currentLineIndex].Length;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleHome(ObservableCollection<string> doc, SubmitionView submitView)
        {
            submitView.currentChar = 0;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleDelete(ObservableCollection<string> doc, SubmitionView submitView)
        {
            int lineIndex, start;
            string line, before, after, nextLine;

            lineIndex = submitView.currentLineIndex;
            line = doc[lineIndex];
            start = submitView.currentChar;

            if (start >= line.Length)
            {
                if (submitView.currentLineIndex == doc.Count - 1)
                    return;

                nextLine = doc[submitView.currentLineIndex + 1];
                doc[submitView.currentLineIndex] += nextLine;
                doc.RemoveAt(submitView.currentLineIndex + 1);

                return;
            }

            before = line.Substring(0, start);
            after = line.Substring(start + 1);
            doc[lineIndex] = before + after;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleBackspace(ObservableCollection<string> doc, SubmitionView submitView)
        {
            int start, lineIndex;
            string currentLine, previousLine,
                line, before, after;

            start = submitView.currentChar;
            
            if (start == 0)
            {
                if (submitView.currentLineIndex == 0)
                    return;

                currentLine = doc[submitView.currentLineIndex];
                previousLine = doc[submitView.currentLineIndex - 1];
                doc.RemoveAt(submitView.currentLineIndex);
                submitView.currentLineIndex--;
                doc[submitView.currentLineIndex] = previousLine + currentLine;
                submitView.currentChar = previousLine.Length;
            }
            else
            {
                lineIndex = submitView.currentLineIndex;
                line = doc[lineIndex];
                before = line.Substring(0, start - 1);
                after = line.Substring(start);
                doc[lineIndex] = before + after;
                submitView.currentChar--;
            }
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleEscape(ObservableCollection<string> doc, SubmitionView submitView)
        {
            doc.Clear();
            doc.Add(string.Empty);
            submitView.currentLineIndex = 0;
            submitView.currentChar = 0;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleDocumentLeft(ObservableCollection<string> doc, SubmitionView submitView)
        {
            if (submitView.currentChar > 0)
                submitView.currentChar -= 1;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleDocumentRight(ObservableCollection<string> doc, SubmitionView submitView)
        {
            string line;
            
            line = doc[submitView.currentLineIndex];

            if (submitView.currentChar <= (line.Length-1))
                submitView.currentChar += 1;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleDocumentUp(ObservableCollection<string> doc, SubmitionView submitView)
        {
            if (submitView.currentLineIndex > 0)
                submitView.currentLineIndex -= 1;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleDocumentDown(ObservableCollection<string> doc, SubmitionView submitView)
        {

            if (submitView.currentLineIndex < (doc.Count - 1))
                submitView.currentLineIndex += 1;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handleEnter(ObservableCollection<string> doc, SubmitionView submitView)
        {
            string docText;
            bool isCompleate;

            docText = string.Join(Environment.NewLine, doc);

            if (docText.StartsWith("!"))
            {
                LEAVE = true;
                return;
            }


            isCompleate = isCompleateSubmition(docText);
            if (isCompleate == true)
            {
                LEAVE = true;
                return;
            }

            insertLine(doc, submitView);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void handlTyping(ObservableCollection<string> doc, SubmitionView submitView, string text)
        {
            int lineIndex, start;

            lineIndex = submitView.currentLineIndex;
            start = submitView.currentChar;
            doc[lineIndex] = doc[lineIndex].Insert(start, text);
            submitView.currentChar += text.Length;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void insertLine(ObservableCollection<string> doc, SubmitionView view)
        {
            string remainder;
            int lineIndex;

            remainder = doc[view.currentLineIndex];

            // prevent invalid substring operation
            if (remainder.Length == 0 || view.currentChar > remainder.Length)
            {
                remainder = remainder.PadRight(view.currentChar, ' ');
                doc[view.currentLineIndex] = remainder;
            }

            remainder = remainder.Substring(view.currentChar);
            doc[view.currentLineIndex] = doc[view.currentLineIndex].Substring(0, view.currentChar);

            lineIndex = view.currentLineIndex + 1;
            doc.Insert(lineIndex, remainder);
            view.currentChar = 0;
            view.currentLineIndex = lineIndex;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private void UpdateDocumentFromHistory(ObservableCollection<string> doc, SubmitionView view)
        {
            string[] lines;
            string historyItem;

            if (_submissionHistory.Count == 0)
                return;

            doc.Clear();

            historyItem = _submissionHistory[_submissionHistoryIndex];
            historyItem = historyItem.Replace(Environment.NewLine, "\n");
            lines = historyItem.Split('\n');
            
            foreach (var line in lines)
                doc.Add(line);

            view.currentLineIndex = doc.Count - 1;
            view.currentChar = doc[view.currentLineIndex].Length;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        protected virtual object RenderLine(IReadOnlyList<string> lines, int lineIndex, object state)
        {
            Console.Write(lines[lineIndex]);
            return state;
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        protected abstract void evaluatePgm(string text);

        // /////////////////////////////////////////////////////////////////////////////////////
        protected abstract bool isCompleateSubmition(string txt);

        // /////////////////////////////////////////////////////////////////////////////////////
        protected abstract void processMetaCommand(string ln);
    }
}

