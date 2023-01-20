using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.Framework
{
    [Serializable]
    public class SpriteSelector<T> where T : Enum
    {
        [Serializable]
        public class TypeSpritePair
        {
            public T type;
            public Sprite sprite;
        }

        [Serializable]
        public class SpriteSelectorItem
        {
            public Image image;
            public TypeSpritePair[] spritePairs;
        } 
        
        public SpriteSelectorItem[] items;

        public void SetSprite(T type)
        {
            foreach (var item in items)
            {
                if (!item.image)
                    continue;
                var match = item.spritePairs.FirstOrDefault(pair => pair.type.Equals(type));
                if (match != null && match.sprite != null)
                {
                    item.image.sprite = match.sprite;
                }
            }
        }
    }
}