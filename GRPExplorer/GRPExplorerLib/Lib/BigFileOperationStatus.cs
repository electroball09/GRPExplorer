using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GRPExplorerLib
{
    public abstract class BigFileOperationStatus
    {
        public virtual string OperationName { get { return "BigFileOperation"; } }

        internal Stopwatch stopwatch = new Stopwatch();
        public float TimeTaken { get { return stopwatch.ElapsedMilliseconds; } }

        public event Action<BigFileOperationStatus> OnProgressUpdated;
        private float progress = 0f;
        public virtual float Progress { get { return progress; } }

        internal void UpdateProgress(float _progress)
        {
            progress = _progress;
            if (OnProgressUpdated != null)
            {
                OnProgressUpdated.Invoke(this);
            }
        }

    }
}
