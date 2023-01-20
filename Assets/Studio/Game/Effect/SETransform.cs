using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Framework
{
    public class SETransform : SubEffect
    {
        [System.Flags]
        public enum Options { None = 0, Position = 1, Rotation = 2, Scale = 4, Stretch = 8 }
        [SerializeField]
        private Options _options;

        [SerializeField]
        private Vector3 _position;
        [SerializeField]
        private Vector3 _rotation;
        [SerializeField]
        private Vector3 _scale;

        [SerializeField]
        private Rect _stretch;
        
        private bool IsChecked(Options option)
        {
            return (_options & option) == option;
        }

        public override void Play(GameObject owner)
        {
            if (IsChecked(Options.Rotation))
                transform.localRotation = Quaternion.Euler(_rotation);
            if (IsChecked(Options.Scale))
                transform.localScale = _scale;
            if (IsChecked(Options.Position))
                transform.localPosition = _position;
            if (IsChecked(Options.Stretch))
            {
                var rectTm = transform as RectTransform;
                if (rectTm == null)
                    return;
                rectTm.anchorMin = new Vector2(0, 0);
                rectTm.anchorMax = new Vector2(1, 1);
                rectTm.offsetMin = new Vector2(_stretch.xMin, _stretch.yMax);
                rectTm.offsetMax = new Vector2(_stretch.xMax, _stretch.yMin);
            }
        }
    }
}