using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public class EvaluationResult
    {
        public DiagnosticBag Diagnostics = new DiagnosticBag();
        public object Value;

        public EvaluationResult(DiagnosticBag diagnostics, object value)
        {
            Diagnostics = diagnostics;
            Value = value;
        }
    }
}
