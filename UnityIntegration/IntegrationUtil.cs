using System;
using System.Collections.Generic;
using System.Text;
using GRPExplorerLib;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;
using System.Threading;
using UnityEngine;

namespace UnityIntegration
{
    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

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

        public static void ChangeRenderMode(this Material standardShaderMaterial, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
            }

        }

        public static Vector3 ConvertToUnity(this System.Numerics.Vector3 from)
        {
            return new Vector3(from.X, from.Y, from.Z);
        }

        public static Vector3[] ConvertToUnity(this System.Numerics.Vector3[] from)
        {
            Vector3[] arr = new Vector3[from.Length];
            for (int i = 0; i < from.Length; i++)
                arr[i] = from[i].ConvertToUnity();
            return arr;
        }

        public static Vector2 ConvertToUnity(this System.Numerics.Vector2 from)
        {
            return new Vector2(from.X, from.Y);
        }

        public static Vector2[] ConvertToUnity(this System.Numerics.Vector2[] from)
        {
            Vector2[] arr = new Vector2[from.Length];
            for (int i = 0; i < from.Length; i++)
                arr[i] = from[i].ConvertToUnity();
            return arr;
        }

        public static Matrix4x4 ConvertToUnity(this System.Numerics.Matrix4x4 from)
        {
            return new Matrix4x4()
            {
                m00 = from.M11,
                m01 = from.M12,
                m02 = from.M13,
                m03 = from.M14,

                m10 = from.M21,
                m11 = from.M22,
                m12 = from.M23,
                m13 = from.M24,

                m20 = from.M31,
                m21 = from.M32,
                m22 = from.M33,
                m23 = from.M34,

                m30 = from.M41,
                m31 = from.M42,
                m32 = from.M43,
                m33 = from.M44,
            };
        }

        public static Matrix4x4[] ConvertToUnity(this System.Numerics.Matrix4x4[] from)
        {
            Matrix4x4[] arr = new Matrix4x4[from.Length];
            for (int i = 0; i < from.Length; i++)
                arr[i] = from[i].ConvertToUnity();
            return arr;
        }
    }
}
