using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib
{
    public interface IGRPExplorerLibLogInterface
    {
        void Debug(string msg);
        void Info(string msg);
        void Error(string msg);
    }
}
