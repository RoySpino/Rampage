using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public class EvaluationResult
    {
        //public DiagnosticBag _Diagnostics = new DiagnosticBag();
        public ImmutableArray<Diagnostics> _Diagnostics { get; }
        public object Value;

        public EvaluationResult(ImmutableArray<Diagnostics> diagnostics, object value)
        {
            _Diagnostics = diagnostics;
            Value = value;
        }
    }
}
