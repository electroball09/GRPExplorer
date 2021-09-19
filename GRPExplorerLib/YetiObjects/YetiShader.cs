#pragma warning disable IDE1006
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using GRPExplorerLib.Util;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiShaderNodeGraph
    {
        public bool LoadGraphFromStream(Stream s)
        {
            BinaryReader br = new BinaryReader(s);

            s.Seek(16, SeekOrigin.Current);

            int numNodes = br.ReadInt32();

            LogManager.Debug($"Loading {numNodes} nodes");

            s.Seek(4, SeekOrigin.Current);

            for (int i = 0; i < numNodes; i++)
            {
                var node = YetiShader.ShaderNode.LoadNodeFromStream(s);
                if (node == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class YetiShader : YetiObjectArchetype
    {

        public override YetiObjectType Identifier => YetiObjectType.shd;

        public int Flags { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            using (MemoryStream ms = new MemoryStream(buffer, 0, size))
            using (BinaryReader br = new BinaryReader(ms))
            {
                LogManager.Debug($"Loading shader key {Object.FileInfo.Key:X8}");

                Flags = br.ReadInt32();

                ms.Seek(64, SeekOrigin.Current);

                YetiShaderNodeGraph graph = new YetiShaderNodeGraph();
                if (!graph.LoadGraphFromStream(ms))
                {
                    log.Error($"Shader graph loading failed on object {Object.NameWithExtension}  key: {Object.FileInfo.Key}");
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            
        }


        public abstract class ShaderNode
        {
            static Dictionary<string, Type> nodeTypeMap;
            static IOBuffers buffers = new IOBuffers();

            static ShaderNode()
            {
                nodeTypeMap = new Dictionary<string, Type>();
                var types = typeof(ShaderNode).GetNestedTypes();
                foreach (var type in types)
                    nodeTypeMap.Add(type.Name, type);
            }

            public static ShaderNode LoadNodeFromStream(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                int idLength = br.ReadInt32();
                if (idLength <= 0)
                {
                    LogManager.Error($"Came across an invalid shader node id length!");
                    return null;
                }
                if (idLength > 50)
                {
                    LogManager.Error($"Very large id length! [{idLength}]");
                    return null;
                }
                s.Read(buffers[idLength], 0, idLength);
                string nodeId = Encoding.ASCII.GetString(buffers[idLength], 0, idLength);
                if (nodeId.Substring(0, 4) != "eSID")
                {
                    LogManager.Error($"Invalid shader node ID! [{nodeId}]");
                    return null;
                }

                LogManager.Debug($"Loading node ID {nodeId}");

                if (idLength % 4 == 0)
                    s.Seek(4, SeekOrigin.Current);
                else
                    s.Seek(4 - (idLength % 4), SeekOrigin.Current);

                if (nodeTypeMap.TryGetValue(nodeId, out var type))
                {
                    ShaderNode node = (ShaderNode)Activator.CreateInstance(type);
                    node.LoadFromStream(s);
                    return node;
                }
                else
                {
                    LogManager.Error($"Shader node ID {nodeId} has no implementation!");
                    return null;
                }
            }

            public abstract void LoadFromStream(Stream s);

            public class eSID_MainOutput : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    BinaryReader br = new BinaryReader(s);

                    int dataLen = br.ReadInt32();

                    //4 bytes x 3 sets
                    s.Seek(dataLen * 4 * 3, SeekOrigin.Current);
                }
            }

            public class eSID_Tex2D : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(340, SeekOrigin.Current);
                }
            }

            public class eSID_UVSource : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(8, SeekOrigin.Current);
                }
            }

            public class eSID_DiffuseMultiplier : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_ConstantMUL : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(32, SeekOrigin.Current);
                }
            }

            public class eSID_PixelViewToWorld : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_ForceVisualPrepass : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(4, SeekOrigin.Current);
                }
            }

            public class eSID_ColorSelector : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(24, SeekOrigin.Current);
                }
            }

            public class eSID_MUL : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(28, SeekOrigin.Current);
                }
            }

            public class eSID_MaterialColor_Emissive : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(664, SeekOrigin.Current);
                }
            }

            public class eSID_Comment : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    BinaryReader br = new BinaryReader(s);

                    s.Seek(4, SeekOrigin.Current);
                    int commentLength = br.ReadInt32();

                    s.Read(buffers[commentLength], 0, commentLength);
                    string comment = Encoding.ASCII.GetString(buffers[commentLength], 0, commentLength);

                    LogManager.Info($"shader comment: {comment}");

                    if (commentLength % 4 != 0)
                        s.Seek(4 - (commentLength % 4), SeekOrigin.Current);
                }
            }

            public class eSID_TexBumpTangent : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(340, SeekOrigin.Current);
                }
            }

            public class eSID_Tangent2Screen : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_Normalize3D : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_AmbientCube : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_SpecularGlossMultiplier : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_SpecularPowerMultiplier : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_LODMUL : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_Normal : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(4, SeekOrigin.Current);
                }
            }

            public class eSID_LODBlender : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(28, SeekOrigin.Current);
                }
            }

            public class eSID_ADD : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(28, SeekOrigin.Current);
                }
            }

            public class eSID_ConstantVector : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(20, SeekOrigin.Current);
                }
            }

            public class eSID_MaterialColor_Diffuse : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(664, SeekOrigin.Current);
                }
            }

            public class eSID_VertexColor_Misc1 : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(664, SeekOrigin.Current);
                }
            }

            public class eSID_VertexInvert : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_Vertex_UV_MUL : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(28, SeekOrigin.Current);
                }
            }

            public class eSID_VertexUVSource : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(8, SeekOrigin.Current);
                }
            }

            public class eSID_VertexUVToPixelUV : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_VertexConstUVWQ : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(20, SeekOrigin.Current);
                }
            }

            public class eSID_Vertex_UV_ADD : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(28, SeekOrigin.Current);
                }
            }

            public class eSID_VertexUV_Combiner4D : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(52, SeekOrigin.Current);
                }
            }

            public class eSID_UV_Combiner4D : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(52, SeekOrigin.Current);
                }
            }

            public class eSID_VertexRGB2UV : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_PixelUVBoxAnimBlend : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(60, SeekOrigin.Current);
                }
            }

            public class eSID_UVScroll : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(32, SeekOrigin.Current);
                }
            }

            public class eSID_UntransformedNormal : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(4, SeekOrigin.Current);
                }
            }

            public class eSID_CustomCode : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    BinaryReader br = new BinaryReader(s);
                    s.Seek(100, SeekOrigin.Current);

                    int codeLen = br.ReadInt32();

                    s.Read(buffers[codeLen], 0, codeLen);
                    string code = Encoding.ASCII.GetString(buffers[codeLen], 0, codeLen);

                    LogManager.Info(code);

                    if (codeLen % 4 != 0)
                        s.Seek(4 - (codeLen % 4), SeekOrigin.Current);
                }
            }

            public class eSID_UV2RGB : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_RGB2UV : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(16, SeekOrigin.Current);
                }
            }

            public class eSID_ElapseTime : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(4, SeekOrigin.Current);
                }
            }

            public class eSID_PulseWave : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(24, SeekOrigin.Current);
                }
            }

            public class eSID_SpecularCubeMap : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(40, SeekOrigin.Current);
                }
            }

            public class eSID_FlatChrome : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(28, SeekOrigin.Current);
                }
            }

            public class eSID_PowFresnel : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(20, SeekOrigin.Current);
                }
            }

            public class eSID_VtxWorldPosition : ShaderNode
            {
                public override void LoadFromStream(Stream s)
                {
                    s.Seek(4, SeekOrigin.Current);
                }
            }
        }
    }
}
