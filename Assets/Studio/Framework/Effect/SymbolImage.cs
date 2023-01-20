using UnityEngine;
using UnityEngine.UI;

namespace Studio.Game
{
    public class SymbolImage : MonoBehaviour
    {
        [SerializeField]
        private Image[] _images;
        [SerializeField]
        private Renderer[] _renderers;

        public void SetImage(Sprite sprite)
        {
            foreach (var image in _images)
                image.sprite = sprite;

            var texture = sprite != null ? sprite.texture : null;

            foreach (var renderer in _renderers)
                renderer.material.mainTexture = texture;
        }
    }
}