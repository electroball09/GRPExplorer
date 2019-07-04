using System;
using System.Collections.Generic;
using System.Text;
using GRPExplorerLib;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;
using System.Threading;

namespace UnityIntegration
{
    public static class IntegrationUtil
    {
        /// <summary>
        /// Creates a thread and loads a bigfile on it, then calls the method provided.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="next"></param>
        public static void LoadBigFileInBackground(string file, Action<BigFile> next)
        {
            BigFile bigFile = BigFile.OpenBigFile(file);
            
            ThreadStart ts = new ThreadStart
                (() =>
                {
                    bigFile.LoadFromDisk();

                    next?.Invoke(bigFile);
                });

            Thread t = new Thread(ts);

            t.Start();
        }
    }
}
