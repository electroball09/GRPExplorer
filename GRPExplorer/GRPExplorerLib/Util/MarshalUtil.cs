using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GRPExplorerLib.Util
{
    public static class MarshalUtil
    {
        public static byte[] StructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] datas = new byte[size];

            IntPtr dataPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, dataPtr, false);
            Marshal.Copy(dataPtr, datas, 0, size);
            Marshal.FreeHGlobal(dataPtr);

            return datas;
        }

        public static int StructToBytes<T>(T structure, byte[] bytes) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            IntPtr dataPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, dataPtr, false);
            Marshal.Copy(dataPtr, bytes, 0, size);
            Marshal.FreeHGlobal(dataPtr);
            return size;
        }

        public static T BytesToStruct<T>(byte[] datas) where T : struct
        {
            T structure = new T();

            int structSize = Marshal.SizeOf(structure);
            IntPtr locPtr = Marshal.AllocHGlobal(structSize);

            Marshal.Copy(datas, 0, locPtr, structSize);

            structure = (T)Marshal.PtrToStructure(locPtr, typeof(T));

            Marshal.FreeHGlobal(locPtr);

            return structure;
        }

        public static bool CompareByteArrays(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
                return false;

            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }

            return true;
        }
    }
}
