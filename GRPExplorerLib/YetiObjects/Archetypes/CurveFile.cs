using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;

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

    public class CurveFileArchetype : BigFileFileArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.cur;

        public int KeyframeCount { get; private set; }

        public CurveKeyframe[] Keyframes { get; private set; }

        public override void Load(byte[] buffer, int size, BigFileFile[] fileReferences)
        {
            KeyframeCount = BitConverter.ToInt16(buffer, 4);
            Keyframes = new CurveKeyframe[KeyframeCount];

            //if (rawData.Length < 25 + (KeyframeCount + (KeyframeCount * 16) + 12))
            //    return;
            
            for (int i = 0; i < KeyframeCount; i++)
            {
                try
                {
                    CurveKeyframe kf = new CurveKeyframe();
                    //25 is base offset into curve file
                    //using i because after every keyframe there's 1 byte of seemingly random data
                    //  (that may be important later)

                    //kf.Number1 = 25 + (i + (i * 16) + 0);
                    //kf.Number2 = 25 + (i + (i * 16) + 4);
                    //kf.Number3 = 25 + (i + (i * 16) + 8);
                    //kf.Number4 = 25 + (i + (i * 16) + 12);

                    kf.x = BitConverter.ToSingle(buffer, 25 + (i + (i * 16) + 0));
                    kf.y = BitConverter.ToSingle(buffer, 25 + (i + (i * 16) + 4));
                    kf.@in = BitConverter.ToSingle(buffer, 25 + (i + (i * 16) + 8));
                    kf.@out = BitConverter.ToSingle(buffer, 25 + (i + (i * 16) + 12));
                    kf.flags = buffer[25 + (i + (i * 16) + 16)];

                    Keyframes[i] = kf;
                }
                catch (Exception ex)
                {
                    LogManager.Error(ex.Message);
                    Keyframes[i].x = -69;
                }
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
