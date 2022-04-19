using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace GRPExplorerLib.Util
{
    public class DiagTools
    {
        private Stopwatch stopwatch = new Stopwatch();
        public long StopwatchTime { get { return stopwatch.ElapsedMilliseconds; } }

        public void StartStopwatch()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        public void StopStopwatch()
        {
            stopwatch.Stop();
        }
    }
}
