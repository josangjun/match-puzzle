using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using Studio.Game;
using Studio.Common;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Studio.Framework;
using Studio.Framework.Audio;
using System.IO.Compression;

namespace Studio
{
    public class EditorTool
    {
        [InitializeOnLoadMethod]
        public static void OnInitialize()
        {
            
        }
#if false
        [MenuItem("Tools/Game/Reimport Item Textures")]
        public static void ReimportItemTextures()
        {
            var pathList = AssetDatabase.FindAssets("t:texture", new[] { 
                "Assets/UIGuide/Origin/item",
                "Assets/UIGuide/Origin/slot/icon/Remote",
            }).Select(AssetDatabase.GUIDToAssetPath).ToList();

            var formats = new[] {
                TextureImporterFormat.ASTC_4x4, TextureImporterFormat.ETC2_RGBA8, 
                TextureImporterFormat.ETC2_RGB4, TextureImporterFormat.ETC_RGB4
            };

            foreach (var path in pathList)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                var androidSetting = importer.GetPlatformTextureSettings("Android");
                var iosSetting = importer.GetPlatformTextureSettings("iPhone");

                if (formats.Contains(androidSetting.format) && formats.Contains(iosSetting.format))
                    continue;

                androidSetting.overridden = true;
                androidSetting.format = TextureImporterFormat.ETC2_RGBA8;
                iosSetting.overridden = true;
                iosSetting.format = TextureImporterFormat.ASTC_4x4;

                importer.SetPlatformTextureSettings(androidSetting);
                importer.SetPlatformTextureSettings(iosSetting);
                importer.SaveAndReimport();
            }
        }

        [MenuItem("Tools/Game/Disable GPU Instancing")]
        public static async void DisableGPUInstancing()
        {
            var sb = new System.Text.StringBuilder();
            var pathList = Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep).Select(AssetDatabase.GetAssetPath).ToArray();

            if (pathList.Length == 0)
            {
                pathList = Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets).Select(AssetDatabase.GetAssetPath).ToArray();
            }

            if (pathList.Length == 0)
            {
                pathList = AssetDatabase.FindAssets("t:prefab", new[] {
                    "Assets/Art/Effect/prefab",
                    "Assets/Scenes/Spin/Prefabs/Effect"
                }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            }

            foreach (var path in pathList)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var renderers = go.GetComponentsInChildren<ParticleSystemRenderer>(true).Where(arg =>
                    arg.renderMode == ParticleSystemRenderMode.Mesh &&
                    arg.enableGPUInstancing).ToArray();
                foreach (var renderer in renderers)
                {
                    renderer.enableGPUInstancing = false;
                    sb.Append(renderer.gameObject.GetHierarchy()).AppendLine();
                }
                if (renderers.Length > 0)
                {
                    EditorUtility.SetDirty(go);
                    await Task.Yield();
                }
            }
            Debug.Log(sb);
        }

        [MenuItem("Tools/Game/Light Quality")]
        public static async void SearchLightQuality()
        {
            var sceneList = AssetDatabase.FindAssets("t:scene", new[] { "Assets/Art/Stage" }).Select(AssetDatabase.GUIDToAssetPath);
            foreach (var arg in sceneList)
            {
                var scene = EditorSceneManager.OpenScene(arg);
                foreach (var light in Object.FindObjectsOfType<Light>())
                {
                    light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.FromQualitySettings;
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                await Task.Yield();
            }
        }

        [MenuItem("Tools/Game/Search Override Texture")]
        public static async void SearchOverrideTexture()
        {
            var pathList = AssetDatabase.FindAssets("t:texture").Select(AssetDatabase.GUIDToAssetPath).ToList();
            var sb = new System.Text.StringBuilder();
            var available = new TextureImporterFormat[]
            {
                TextureImporterFormat.ETC2_RGB4,
                TextureImporterFormat.ETC2_RGBA8,
                TextureImporterFormat.ETC_RGB4,
                TextureImporterFormat.ASTC_4x4,
                TextureImporterFormat.ETC2_RGBA8Crunched,
                TextureImporterFormat.ETC_RGB4Crunched,
                TextureImporterFormat.AutomaticCompressed
            };
            int i = 0;
            var tick = pathList.Count / 10;
            
            foreach (var path in pathList)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.Log($"importer is null. path:{path}");
                    continue;
                }
                var settings = importer.GetPlatformTextureSettings("Android");
                if (settings == null) continue;

                void SaveAndReImport(TextureImporterFormat fmt)
                {
                    settings.format = fmt;
                    importer.SetPlatformTextureSettings(settings);
                    importer.SaveAndReimport();
                }

                if (available.Contains(settings.format) == false)
                    sb.Append($"{path}, {settings.format}").AppendLine();
                if (settings.format == TextureImporterFormat.PVRTC_RGBA4)
                {
                    SaveAndReImport(TextureImporterFormat.ETC2_RGBA8);
                }
                else if (settings.format == TextureImporterFormat.PVRTC_RGB4)
                {
                    SaveAndReImport(TextureImporterFormat.ETC2_RGB4);
                }
                
                if (i++ % tick == 0)
                    await Task.Yield();
            }
            Debug.Log(sb);
        }
#endif
        
        [MenuItem("Tools/Game/Reserailzie Prefabs")]
        public static void ReserializePrefabs()
        {
            foreach (var asset in Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets))
            {
                var prefab = asset as GameObject;
                if (prefab == null)
                    continue;

                EditorUtility.SetDirty(prefab);
            }
        }

        [MenuItem("Tools/Migrate Shaders")]
        public static async void MigrateShaders()
        {
            var targetNames = new string[] { "Stencil/Transparent", "Stencil/Transparent Cutout" };
            var pathList = AssetDatabase.FindAssets("t:material").Select(AssetDatabase.GUIDToAssetPath).ToList();
            var tick = 30;
            var sb = new System.Text.StringBuilder();
            var shader = Shader.Find("UI/Transparent Options");
            for (var i = 0; i < pathList.Count; ++i)
            {
                var path = pathList[i];
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (targetNames.Contains(material.shader.name))
                {
                    var rq = material.renderQueue;
                    material.shader = shader;
                    material.renderQueue = rq;
                    EditorUtility.SetDirty(material);
                    sb.Append(path).AppendLine();
                }
                if ((i + 1) % tick == 0)
                    await Task.Yield();
            }
            Debug.Log($"finished:{sb}");
        }

        //[MenuItem("Tools/Decompress Gzip")]
        public static async void DecompressGzip()
        {
            var rootDir = Application.dataPath + "/../Resource";
            var gzNames = System.IO.Directory.GetFiles(rootDir, "*.gz");
            
            foreach (var gzName in gzNames)
            {
                var outName = System.IO.Path.GetFileNameWithoutExtension(gzName);
                var outPath = $"{rootDir}/{outName}";
                using var inFile = new FileStream(gzName, FileMode.Open);
                using var gzStream = new GZipStream(inFile, CompressionMode.Decompress);
                using var outFile = new FileStream(outPath, FileMode.CreateNew);
                await gzStream.CopyToAsync(outFile);
            }
        }

        [MenuItem("Tools/Game/Find Sound Effects")]
        public static void FindSoundEffects()
        {
            var sb = new System.Text.StringBuilder();
            var list = new List<string>();
            var pathList = AssetDatabase.FindAssets("t:prefab").Select(AssetDatabase.GUIDToAssetPath).ToList();
            foreach (var path in pathList)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var effect = go.GetComponent<GoEffect>();
                if (effect == null) continue;
                for (var i = 0; i < effect.Count; ++i) {
                    if (effect[i] is SESoundSource)
                    {
                         list.Add(path);
                         i = effect.Count;
                         break;
                    }
                }
            }
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(list));
        }

        [MenuItem("Tools/Game/Pause #Q")]
        public static void PauseEditor()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        public static string FindAssetPath(string arg, params string[] subDirs)
        {
            return AssetDatabase.FindAssets(arg, subDirs).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
        }

        public static IEnumerable<string> FindAssetPathList(string arg, params string[] subDirs)
        {
            return AssetDatabase.FindAssets(arg, subDirs).Select(AssetDatabase.GUIDToAssetPath);
        }

        public static T LoadAssetAtPath<T>(string arg, params string[] subDirs) where T : UnityEngine.Object
        {
            var path = AssetDatabase.FindAssets(arg, subDirs).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        [MenuItem("Tools/Game/Prepare Start #F2")]
        public static void PrepareStart()
        {
            var path1 = FindAssetPath("t:scene Game", "Assets/Scenes");
            EditorSceneManager.OpenScene(path1, OpenSceneMode.Single);

            var path2 = FindAssetPath("t:scene Service", "Assets/Scenes");
            EditorSceneManager.OpenScene(path2, OpenSceneMode.Additive);
        }

        [MenuItem("Tools/Game/Prepare Splash #F4")]
        public static void PrepareSplash()
        {
            var path = FindAssetPath("t:scene Splash", "Assets/Scenes");
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("Tools/Game/Strip AudioListeners")]
        public static async void StripAudioListeners()
        {
            var pathList = FindAssetPathList("t:scene", "Assets/Art/Stage");

            foreach (var path in pathList)
            {
                await Task.Yield();
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.EndsWith("_bg"))
                    continue;
                var scene = EditorSceneManager.OpenScene(path);
                var listeners = Camera.allCameras
                    .Where(arg => arg.gameObject.scene.Equals(scene))
                    .Select(arg => arg.GetComponent<AudioListener>())
                    .Where(arg => arg != null).ToArray();
                if (listeners.Length > 0)
                {
                    listeners.ForEach(arg => Object.DestroyImmediate(arg));
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        [MenuItem("Tools/Game/Create New Prefab")]
        static void CreatePrefab()
        {
            GameObject[] objs = Selection.gameObjects;

            var path = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();

            foreach (GameObject go in objs)
            {
                string localPath = $"{path}/{go.name}.prefab";
                //Debug.Log(localPath);
                if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?",
                        "The prefab already exists. Do you want to overwrite it?",
                        "Yes",
                        "No"))
                    {
                        CreateNew(go, localPath);
                    }
                }
                else
                {
                    Debug.Log(go.name + " Prefab Created");
                    CreateNew(go, localPath);
                }
            }
        }

        // Disable the menu item if no selection is in place
        [MenuItem("Tools/Game/Create New Prefab", true)]
        static bool ValidateCreatePrefab()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("Tools/Game/Delete PlayerPreferences")]
        public static void DeletePlayerPreferences()
        {
            PlayerPrefs.DeleteAll();
        }

        static void CreateNew(GameObject obj, string localPath)
        {
            bool success;
            PrefabUtility.SaveAsPrefabAsset(obj, localPath, out success);
            // Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
            // PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }

        public static void ExecuteCommand(string command, string param, Encoding encoding, string workDir)
        {
            ExecuteCommand(command, param, encoding, out var stdout, out var stderr, workDir);
        }

        [MenuItem("Tools/Set Dirty")]
        public static void SetDirty()
        {
            var filtered = Selection.GetFiltered<Object>(SelectionMode.Assets);
            foreach (var obj in filtered)
            {
                EditorUtility.SetDirty(obj);
            }
        }
        

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        /// ExecuteCommand("/bin/bash", "./build/script/compile_error_fix.sh", Encoding.ASCII);
        public static void ExecuteCommand(string command, string param, Encoding enc, out string stdout, out string stderr, string workDir = null)
        {
            // Debug.Log("Running command: " + command);
            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            string cmd = null;
            string prm = null;
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                cmd = "cmd";
                prm = string.Format("/c \"{0}\" {1}", command, param);
            } else {
                cmd = command;
                prm = param;
            }
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(cmd, prm);
            Debug.LogWarningFormat("Exec> {0} {1}", cmd, prm);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.StandardErrorEncoding = enc;
            procStartInfo.StandardOutputEncoding = enc;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;
            if (string.IsNullOrEmpty(workDir) == false)
                procStartInfo.WorkingDirectory = workDir;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            if (!proc.Start())
            {
                throw new System.InvalidOperationException(cmd);
            }
            // Get the output into a string
            stdout = Encode(proc.StandardOutput.BaseStream, enc);
            stderr = Encode(proc.StandardError.BaseStream, enc);
            if (!string.IsNullOrEmpty(stdout))
            {
                Debug.Log(stdout);
            }
            if (!string.IsNullOrEmpty(stderr))
            {
                Debug.Log(stderr);
            }
        }

        public static string Encode(Stream stream, Encoding enc)
        {
            StreamReader reader = new StreamReader(stream, enc);
            string str = reader.ReadToEnd();
            reader.Close();
            return str;
        }
        /*
        [MenuItem("Tools/MigrateTransparentMasked")]
        public static async void MigrateTransparentMasked()
        {
            var pathList = AssetDatabase.FindAssets("t:Material", new [] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).ToList();
            var sb = new System.Text.StringBuilder();
            foreach (var path in pathList)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!mat) continue;

                var shader = mat.shader;
                if (!shader || shader.name != "Unlit/Transparent Masked") continue;
                
                var tint = mat.GetColor("_TintColor");
                mat.SetColor("_Color", tint);
                mat.SetColor("_TintColor", Color.black);
                EditorUtility.SetDirty(mat);
                sb.Append(path).AppendLine();
                await Task.Yield();
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Finished:\n{sb}");
        }
        */

        [MenuItem("GameObject/Effects/Add Order In Layer")]
        public static void AddOrderInLayer()
        {
            var particles = Selection.activeGameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var p in particles)
            {
                p.sortingOrder++;
                EditorUtility.SetDirty(p);
            }
        }
    }
}