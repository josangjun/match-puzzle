using UnityEngine;

namespace Studio.Framework
{
    [System.Serializable]
    public class InstanceMaterial
    {
        [SerializeField]
        private Material _asset;

        private Material _instance;

        public Material GetInstance()
        {
            if (_instance == null && _asset != null)
                _instance = new Material(_asset);
            return _instance;
        }

        public void Release()
        {
            if (_instance)
            {
                Object.DestroyImmediate(_instance);
                _instance = null;
            }
        }
    }
}