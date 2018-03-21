using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GRPExplorerLib
{
    public abstract class BigFileOperationStatus
    {
        internal Stopwatch stopwatch = new Stopwatch();
        public float TimeTaken { get { return stopwatch.ElapsedMilliseconds; } }

        public abstract float Progress { get; }
    }
}
