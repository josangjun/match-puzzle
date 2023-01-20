using UnityEngine;

namespace Studio.Framework
{
    public class SEOverrideLayer : SubEffect
    {
        public enum LayerMode { Owner, Specify }
        [SerializeField]
        private LayerMode _mode;
        [SerializeField]
        private string _layer;

        public override void Play(GameObject owner)
        {
            if (_mode == LayerMode.Owner)
            {
                if (gameObject.layer != owner.layer)
                    gameObject.SetLayer(owner.layer);
                return;
            }
            if (_mode == LayerMode.Specify)
            {
                var layer = LayerMask.NameToLayer(_layer);
                if (gameObject.layer != layer)
                    gameObject.SetLayer(layer);
            }
        }
    }
}