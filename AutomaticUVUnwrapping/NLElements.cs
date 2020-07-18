using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticUVUnwrapping
{

    class NLContext
    {
        public NLVariable[] Variables { get; set; }
        public uint Rows { get; set; }
        public bool LeastSquares { get; set; }
        public uint rhs;

        public NLContext()
        {
            rhs = 1;
        }
    }

    class NLVariable
    {
        public float[] Value { get; } = new float[4];
        public bool Locked { get; set; }
        public uint Index { get; set; }
        public NLRowColumn[] RowColumn { get; set; }
    }

    class NLRowColumn
    {

    }
}
