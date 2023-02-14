using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "BuildInfo.asset", menuName = "Game/BuildInfo")]
    public class BuildInfo : ScriptableObject
    {
        public string commitId;

        public string buildDate;
    }
}