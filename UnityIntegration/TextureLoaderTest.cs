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
        public string currentTextureType = "RGBA32_1";
        public List<string> fileNames = new List<string>();
        public List<BigFileFile> metadataFiles = new List<BigFileFile>();
        public BigFileFile loadedPayload;
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
                    YetiTextureFormat format = (YetiTextureFormat)Enum.Parse(typeof(YetiTextureFormat), currentTextureType);
                    foreach (BigFileFile file in textureFiles)
                    {
                        if (file.ArchetypeAs<TextureMetadataFileArchetype>().Format == format)
                        {
                            if (!file.Name.Contains("LVM"))
                            {
                                fileNames.Add(file.Name);
                                metadataFiles.Add(file);
                            }
                        }
                    }
                    bigFile.FileLoader.LoadFiles(metadataFiles);
                    m_bigFile = bigFile;
                });
        }

        public void LoadTextureFile()
        {
            loadedPayload?.Unload();

            BigFileFile curr = metadataFiles[sel];
            TextureMetadataFileArchetype arch = curr.ArchetypeAs<TextureMetadataFileArchetype>();

            loadedPayload = arch.Payload.File;
            List<BigFileFile> list = new List<BigFileFile>
            {
                loadedPayload
            };
            m_bigFile.FileLoader.LoadFiles(list);

            texture = new Texture2D(arch.Width, arch.Height, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(arch.Payload.Data);
            texture.Apply();

            testPlane.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}
