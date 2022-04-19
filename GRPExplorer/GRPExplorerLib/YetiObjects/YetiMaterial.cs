using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiMaterial : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.mat;

        public List<YetiTextureMetadata> TextureRefs { get; private set; } = new List<YetiTextureMetadata>();
        public YetiObject ShaderObject { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            for (int i = 0; i < objectReferences.Length - 1; i++)
            {
                if (objectReferences[i] != null)
                    TextureRefs.Add(objectReferences[i].ArchetypeAs<YetiTextureMetadata>());
            }

            ShaderObject = objectReferences[objectReferences.Length - 1];
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETIMATERIAL textures: {0}  shader key: {1:X8}", TextureRefs.Count, ShaderObject.FileInfo.Key);
        }
    }
}
