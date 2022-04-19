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
        public byte flags;
        public float x;
        public float y;
        public float inTangent;
        public float outTangent;
    }

    public enum CurveFormat : int
    {
        Constant = 0,
        Simple = 2,
        Full = 4
    }

    public class YetiCurve : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.cur;

        public CurveFormat Format { get; private set; }
        public byte Flags { get; private set; }
        public int KeyframeCount { get; private set; }
        public CurveKeyframe[] Keyframes { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                Format = (CurveFormat)br.ReadInt32();

                if (Format == CurveFormat.Constant)
                    LoadConstantCurve(ms, br);
                else if (Format == CurveFormat.Simple)
                    LoadSimpleCurve(ms, br);
                else if (Format == CurveFormat.Full)
                    LoadFullCurve(ms, br);
                else
                {
                    log.Error("Bad format! {1} ({0})", (int)Format, Object.Name);
                    return;
                }

                Array.Sort(Keyframes, (a, b) => { return a.x >= b.x ? 1 : -1; });
            }
        }

        private void LoadConstantCurve(MemoryStream ms, BinaryReader br)
        {
            KeyframeCount = 1;

            float val = br.ReadSingle();

            Keyframes = new CurveKeyframe[1];
            Keyframes[0].x = 0f;
            Keyframes[0].y = val;
        }

        private void LoadSimpleCurve(MemoryStream ms, BinaryReader br)
        {
            KeyframeCount = br.ReadInt16();

            Keyframes = new CurveKeyframe[KeyframeCount];

            for (int i = 0; i < KeyframeCount; i++)
            {
                Keyframes[i].x = br.ReadSingle();
                Keyframes[i].y = br.ReadSingle();
            }
        }

        private void LoadFullCurve(MemoryStream ms, BinaryReader br)
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
                    inTangent = br.ReadSingle(),
                    outTangent = br.ReadSingle()
                };

                Keyframes[i] = kf;
            }

            ushort magic = br.ReadUInt16();
            if (magic != 0x7879)
            {
                log.Error("Magic number is not 0x7879! ({0:X4})", magic);
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("CURVE keyframes:{0}", KeyframeCount);
            for (int i = 0; i < Keyframes.Length; i++)
            {
                log.Info("{0} {1} {2} {3}", Keyframes[i].x, Keyframes[i].y, Keyframes[i].inTangent, Keyframes[i].outTangent);
            }
        }
    }
}
