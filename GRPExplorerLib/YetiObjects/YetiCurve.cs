using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public struct CurveKeyframe
    {
        public float x;
        public float y;
        public float @in;
        public float @out;
        public byte flags; // maybe?
    }

    public class YetiCurve : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.cur;

        public int Version { get; private set; }
        public byte Flags { get; private set; }
        public int KeyframeCount { get; private set; }
        public CurveKeyframe[] Keyframes { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                Version = br.ReadInt32();

                if (Version == 0)
                    LoadVersion0(ms, br);
                else if (Version == 2)
                    LoadVersion2(ms, br);
                else if (Version == 4)
                    LoadVersion4(ms, br);
                else
                {
                    log.Error("Bad version! {1} ({0})", Version, Object.Name);
                    return;
                }

                Array.Sort(Keyframes, (a, b) => { return a.x >= b.x ? 1 : -1; });
            }
        }

        private void LoadVersion0(MemoryStream ms, BinaryReader br)
        {
            KeyframeCount = 1;

            float val = br.ReadSingle();

            Keyframes = new CurveKeyframe[1];
            Keyframes[0].x = 0f;
            Keyframes[0].y = val;
        }

        private void LoadVersion2(MemoryStream ms, BinaryReader br)
        {
            KeyframeCount = br.ReadInt16();

            Keyframes = new CurveKeyframe[KeyframeCount];

            for (int i = 0; i < KeyframeCount; i++)
            {
                Keyframes[i].x = br.ReadSingle();
                Keyframes[i].y = br.ReadSingle();
            }
        }

        private void LoadVersion4(MemoryStream ms, BinaryReader br)
        {
            KeyframeCount = br.ReadInt16();
            Flags = br.ReadByte();

            int keyframeOffset = 0;
            //if (Flags == 0x0F) 
            //{
            //    keyframeOffset = 1;
            //    KeyframeCount++;
            //}

            Keyframes = new CurveKeyframe[KeyframeCount];

            for (int i = keyframeOffset; i < KeyframeCount; i++)
            {
                CurveKeyframe kf = new CurveKeyframe
                {
                    flags = br.ReadByte(),
                    x = br.ReadSingle(),
                    y = br.ReadSingle(),
                    @in = br.ReadSingle(),
                    @out = br.ReadSingle()
                };

                Keyframes[i] = kf;
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("CURVE keyframes:{0}", KeyframeCount);
            for (int i = 0; i < Keyframes.Length; i++)
            {
                log.Info("{0} {1} {2} {3}", Keyframes[i].x, Keyframes[i].y, Keyframes[i].@in, Keyframes[i].@out);
            }
        }
    }
}
