using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "GemVisualizer", menuName = "Game/GemVisualizer", order = 0)]
    public class GemVisualizer : ScriptableObject
    {
        public virtual void SetAttribute(Gem gem, GemType type)
        {
            
        }
    }
}