using UnityEngine;
using UnityEngine.UI;

namespace Studio.Framework
{
    [System.Serializable]
    public class SpriteToggle
    {
        public Sprite[] activeSprites;
        public Sprite[] inactiveSprites;
        public Image[] images;
        public bool isToggled { get; private set; }
        public void Set(bool toggle)
        {
            isToggled = toggle;
            var sprites = toggle ? activeSprites : inactiveSprites;
            var length = Mathf.Min(images.Length, sprites.Length);
            for (var i = 0; i < length; ++i)
            {
                images[i].sprite = sprites[i];
            }
        }
    }
}