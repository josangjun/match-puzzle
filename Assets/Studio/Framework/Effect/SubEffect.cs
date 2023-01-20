using UnityEngine;

namespace Studio.Framework
{
    public class SubEffect : MonoBehaviour
    {
        public virtual void Preprocess(GameObject owner) {}

        public virtual void Play(GameObject owner) {}

        public virtual void OnRent() {}

        public string sortLayerName { get; set; }
    }
}