using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Game/AppConfig", order = 0)]
    public class AppConfig : ScriptableObject
    {
        public float fallSpeed = 100;
        public int gemTypeCount = (int)GemType.Max-1;

        public void OnValidate()
        {
            gemTypeCount = Mathf.Min((int)GemType.Max-1, gemTypeCount);
        }
    }
}