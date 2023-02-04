using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Game/AppConfig", order = 0)]
    public class AppConfig : ScriptableObject
    {
        public int gemTypeCount = (int)GemType.Max-1;
        public float speedScale = 100;
        public float speedLimit = 940; // units per tick

        public void OnValidate()
        {
            gemTypeCount = Mathf.Min((int)GemType.Max-1, gemTypeCount);
        }
    }
}