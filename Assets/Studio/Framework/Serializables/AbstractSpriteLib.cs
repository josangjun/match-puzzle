using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Framework
{
    public abstract class AbstractSpriteLib : ScriptableObject
    {
        public abstract Sprite GetSprite(string name);

        public abstract Sprite GetSprite(string category, string name);

        public abstract Task<Sprite> GetSpriteAsync(string category, string name);
    }
}