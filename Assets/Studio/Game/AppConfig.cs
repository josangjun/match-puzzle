using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Game/AppConfig", order = 0)]
    public class AppConfig : ScriptableObject
    {
        public float fallSpeed = 100;
        public int GemTypeCount = (int)GemType.Max;

        public void OnValidate()
        {
            GemTypeCount = Mathf.Min((int)GemType.Max, GemTypeCount);
        }
    }
}