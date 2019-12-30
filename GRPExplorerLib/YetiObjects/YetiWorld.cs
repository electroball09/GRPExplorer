﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiWorld : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.wor;

        public YetiGameObjectList GameObjectList { get; private set; }
        public YetiWorldIncludeList SubWorldList { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            GameObjectList = objectReferences[0].ArchetypeAs<YetiGameObjectList>();
            SubWorldList = objectReferences[4]?.ArchetypeAs<YetiWorldIncludeList>();
            if (GameObjectList == null)
            {
                throw new Exception(objectReferences[0].Archetype.GetType().Name);
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("WORLD");
        }
    }

    public class YetiGameObjectList : YetiObjectArchetype
    {
        public enum GOL_Flags
        {
            Default = 0x0D
        }

        public override YetiObjectType Identifier => YetiObjectType.gol;

        public YetiObject[] ObjectList { get; private set; }
        public GOL_Flags[] ObjectFlags { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            ObjectList = objectReferences;
            ObjectFlags = new GOL_Flags[ObjectList.Length];

            int count = (size - 8) / 4;
            int remainder = (size - 8) % 4;
            if (remainder != 0)
                throw new Exception("remainder is " + remainder.ToString());
            if (count != ObjectList.Length)
                throw new Exception("count != ObjectList.Length");

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(8, SeekOrigin.Current);

                for (int i = 0; i < ObjectList.Length; i++)
                {
                    ObjectFlags[i] = (GOL_Flags)br.ReadInt32();
                    if (ObjectFlags[i] != GOL_Flags.Default)
                    {
                        throw new Exception("weird flags!");
                    }
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("GAMEOBJECTLIST count: {0}", ObjectList.Length);
        }
    }

    public class YetiWorldIncludeList : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.wil;

        public YetiWorld[] IncludeList { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            IncludeList = new YetiWorld[objectReferences.Length];
            for (int i = 0; i < objectReferences.Length; i++)
                if (objectReferences[i] != null)
                    IncludeList[i] = objectReferences[i].ArchetypeAs<YetiWorld>();
        }

        public override void Log(ILogProxy log)
        {
            
        }
    }
}
