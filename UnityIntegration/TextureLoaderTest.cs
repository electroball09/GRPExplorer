using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.BigFile.Files.Archetypes;
using GRPExplorerLib.Logging;

namespace UnityIntegration
{
    public class TextureLoaderTest : MonoBehaviour
    {
        static TextureLoaderTest inst;

        [RuntimeInitializeOnLoadMethod]
        static void Load()
        {
            if (inst)
                return;

            inst = new GameObject("TextureLoaderTest").AddComponent<TextureLoaderTest>();

            LogManager.LogInterface = new UnityLogInterface();

            LogManager.GlobalLogFlags = LogFlags.Info | LogFlags.Error;
        }

        public GameObject testPlane;
        public BigFile m_bigFile;
        public string currentFilePath = "";
        public YetiTextureFormat textureType = YetiTextureFormat.RGBA32;
        public TextureFormat ImportAs = TextureFormat.RGBA32;
        public List<string> fileNames = new List<string>();
        public List<BigFileFile> metadataFiles = new List<BigFileFile>();
        public BigFileFile loadedPayload;
        public bool Transparent = false;
        public int ImportCount = 50;
        public int ImportStart = 0;
        public int sel = 0;

        Texture2D texture;

        bool isLoaded = false;

        void Start()
        {
            testPlane = (GameObject)Instantiate(Resources.Load("TextureTester"));
        }

        public void LoadBigFile()
        {
            if (isLoaded)
                return;

            isLoaded = true;
            IntegrationUtil.LoadBigFileInBackground
                (currentFilePath,
                (bigFile) =>
                {
                    List<BigFileFile> textureFiles = bigFile.RootFolder.GetAllFilesOfArchetype<TextureMetadataFileArchetype>();
                    bigFile.FileLoader.LoadFiles(textureFiles);
                    List<BigFileFile> imports = new List<BigFileFile>();
                    Debug.Log(textureType);
                    foreach (BigFileFile file in textureFiles)
                    {
                        YetiTextureFormat f = file.ArchetypeAs<TextureMetadataFileArchetype>().Format;
                        if (f == textureType)
                        {
                            imports.Add(file);
                        }
                    }
                    int ind = ImportStart;
                    foreach (BigFileFile file in imports)
                    {
                        if (ind - ImportStart > ImportCount)
                            break;

                        if (file.ArchetypeAs<TextureMetadataFileArchetype>().Format == textureType)
                        {
                            fileNames.Add(file.Name);
                            metadataFiles.Add(file);
                        }

                        ind++;
                    }
                    bigFile.FileLoader.LoadFiles(metadataFiles);
                    m_bigFile = bigFile;
                });
        }

        public void LoadTextureFile()
        {
            if (Transparent)
            {
                testPlane.GetComponent<Renderer>().material.ChangeRenderMode(StandardShaderUtils.BlendMode.Transparent);
            }
            else
            {
                testPlane.GetComponent<Renderer>().material.ChangeRenderMode(StandardShaderUtils.BlendMode.Opaque);
            }

            loadedPayload?.Unload();

            BigFileFile curr = metadataFiles[sel];
            TextureMetadataFileArchetype arch = curr.ArchetypeAs<TextureMetadataFileArchetype>();

            loadedPayload = arch.Payload.File;
            List<BigFileFile> list = new List<BigFileFile>
            {
                loadedPayload
            };
            m_bigFile.FileLoader.LoadFiles(list);
            
            texture = new Texture2D(arch.Width, arch.Height, ImportAs, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.LoadRawTextureData(arch.Payload.Data);
            texture.Apply();

            testPlane.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}

public static class StandardShaderUtils
{
    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
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
}