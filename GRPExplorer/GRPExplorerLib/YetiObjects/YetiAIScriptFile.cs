using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiAIScriptFile : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.zc_;

        public string Script { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            Script = Encoding.ASCII.GetString(buffer, 0, size);
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETI AI SCRIPT {0}", Object.Name);
            log.Info("\n" + Script);
            log.Info("");
        }
    }
}
