using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Game/AppConfig", order = 0)]
    public class AppConfig : ScriptableObject
    {
        public int gemTypeCount = (int)GemType.Max-1;
        public float gravityScale = 100;
        public float speedLimit = 940; // units per tick
        public bool powerUp = true;

        public string gem1;
        public string gem2;
        public string gem3;
        public string gem4;
        public string gem5;
        public string gem6;
        public string gem7;

        public void OnValidate()
        {
            gemTypeCount = Mathf.Min((int)GemType.Max-1, gemTypeCount);
        }
    }
}