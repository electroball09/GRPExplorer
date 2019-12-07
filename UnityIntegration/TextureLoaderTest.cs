using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.YetiObjects;
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
        public List<YetiObject> textureMetadataFiles = new List<YetiObject>();
        public List<YetiObject> displayedFiles = new List<YetiObject>();
        public YetiObject loadedPayload;
        public bool Transparent = false;
        public int ImportCount = 50;
        public int ImportStart = 0;
        public int sel = 0;

        Texture2D texture;

        bool isLoaded = false;

        void Start()
        {
            testPlane = (GameObject)Instantiate(Resources.Load("TextureTester"));

            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(new Vector3(35f, 0.4325f, -5f), Quaternion.identity, Vector3.one);
            Debug.Log(m);
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
                    textureMetadataFiles = bigFile.RootFolder.GetAllObjectsOfArchetype<YetiTextureMetadata>();
                    bigFile.FileLoader.LoadFiles(textureMetadataFiles);
                    m_bigFile = bigFile;
                });
        }

        public void ChangeDisplayedTextures()
        {
            List<YetiObject> imports = new List<YetiObject>();
            foreach (YetiObject textureFile in textureMetadataFiles)
            {
                if (textureFile.ArchetypeAs<YetiTextureMetadata>().Format == textureType)
                    imports.Add(textureFile);
            }

            if (ImportStart > imports.Count - 1)
            {
                Debug.LogErrorFormat("ImportStart larger than imports count ({0} > {1})", ImportStart, imports.Count);
                return;
            }

            if (ImportStart + ImportCount > imports.Count)
            {
                ImportCount = imports.Count - ImportStart;
            }

            sel = 0;
            fileNames.Clear();
            displayedFiles.Clear();
            for (int i = ImportStart; i < ImportStart + ImportCount; i++)
            {
                YetiObject file = imports[i];
                fileNames.Add(file.Name);
                displayedFiles.Add(file);
            }
        }

        public void LoadTextureFile()
        {
            if (Transparent)
            {
                testPlane.GetComponent<Renderer>().material.ChangeRenderMode(BlendMode.Transparent);
            }
            else
            {
                testPlane.GetComponent<Renderer>().material.ChangeRenderMode(BlendMode.Opaque);
            }

            loadedPayload?.Unload();

            YetiObject curr = displayedFiles[sel];
            YetiTextureMetadata arch = curr.ArchetypeAs<YetiTextureMetadata>();

            loadedPayload = arch.Payload.Object;
            List<YetiObject> list = new List<YetiObject>
            {
                loadedPayload
            };
            m_bigFile.FileLoader.LoadFiles(list);
            
            texture = new Texture2D(arch.Width, arch.Height, ImportAs, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.LoadRawTextureData(arch.Payload.Data);
            texture.Apply();

            testPlane.GetComponent<Renderer>().material.mainTexture = texture;
            testPlane.transform.localScale = new Vector3(arch.Width / arch.Height, 1, 1);
            Camera.main.orthographicSize = Mathf.Clamp(5 + (.37f * (testPlane.transform.localScale.x - 1)), 5, 100);
        }
    }
}