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

        public static Vector3 ConvertYetiToUnityCoords(this System.Numerics.Vector3 from)
        {
            return new Vector3(-from.Y, from.Z, from.X);
        }

        public static Vector3 ConvertYetiToUnityCoords(this Vector3 from)
        {
            return new Vector3(-from.y, from.z, from.x);
        }

        public static Quaternion ConvertYetiToUnityRotation(this Quaternion from)
        {
            return Quaternion.Euler(from.eulerAngles.x, from.eulerAngles.z, from.eulerAngles.y);
        }
        
        /// <summary>
         /// Extract translation from transform matrix.
         /// </summary>
         /// <param name="matrix">Transform matrix. This parameter is passed by reference
         /// to improve performance; no changes will be made to it.</param>
         /// <returns>
         /// Translation offset.
         /// </returns>
        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }

        /// <summary>
        /// Identity quaternion.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
        /// </remarks>
        public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
        /// <summary>
        /// Identity matrix.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
        /// </remarks>
        public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>
        /// The translation transform matrix.
        /// </returns>
        public static Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = IdentityMatrix;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }
    }
}
