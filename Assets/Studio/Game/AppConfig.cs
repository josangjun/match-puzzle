using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "Game/AppConfig", order = 0)]
    public class AppConfig : ScriptableObject {
        public float fallSpeed = 100;
    }
}