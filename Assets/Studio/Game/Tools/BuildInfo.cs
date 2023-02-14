using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "BuildInfo.asset", menuName = "Game/BuildInfo")]
    public class BuildInfo : ScriptableObject
    {
        public string commitId;

        public string buildDate;

        public string GetString()
        {

            if (string.IsNullOrEmpty(commitId) || string.IsNullOrEmpty(buildDate))
            {
#if UNITY_EDITOR
                Sync();
#else
                return string.Empty;
#endif
            }
            return $"[BuildInfo] {commitId}, {buildDate}";
        }

#if UNITY_EDITOR
        [ContextMenu("Sync")]
        public void Sync()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var editorAssembly = assemblies.FirstOrDefault(arg => arg.GetName().Name.StartsWith("Studio.Editor"));
            var type = editorAssembly.GetType("Studio.EditorTool");
            var method = type.GetMethod("SyncAppCommit");
            method.Invoke(null, null);
        }
#endif
    }
}