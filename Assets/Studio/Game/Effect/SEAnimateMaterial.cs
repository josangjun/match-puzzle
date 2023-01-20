using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Studio.Framework
{
    public class SEAnimateMaterial : SubEffect
    {
        [SerializeField]
        private Material _material;

        [System.Serializable]
        public class Property {
            public string name;

            public int id;

            public void OnValidate()
            {
                id = Shader.PropertyToID(name);
            }
        }

        [System.Serializable]
        public struct ColorProperty
        {
            public string name;

            public int id;

            public void OnValidate()
            {
                id = Shader.PropertyToID(name);
            }

            public Color value;
        }
        [System.Serializable]
        public struct FloatProperty 
        {
            public string name;

            public int id;

            public void OnValidate()
            {
                id = Shader.PropertyToID(name);
            }

            public float value;
        }

        [SerializeField]
        public ColorProperty _color;
        [SerializeField]
        public FloatProperty _float;

        private List<Material> _materials = new List<Material>();
        private Material instance;
        private Renderer _renderer;

        private string matName;

        public override void Play(GameObject owner)
        {
            _renderer = owner.GetComponentInChildren<Renderer>();
            if (!_renderer || !_material)
                return;
            //Debug.Log($"[SEAnimateMaterial] Inject:{owner.GetHierarchy()}");
            _renderer.GetSharedMaterials(_materials);
            instance = new Material(_material);
            matName = $"{this.GetInstanceID():X}:{_material.name}";
            instance.name = matName;
            _materials.Add(instance);
            _renderer.sharedMaterials = _materials.ToArray();
        }

        protected void LateUpdate()
        {
            if (!_renderer)
                return;
            
            _renderer.GetSharedMaterials(_materials);
            for (var i = 0; i < _materials.Count; ++i)
            {
                var mat = _materials[i];
                if (!IsInstance(mat))
                    continue;
                
                if (_color.id != 0)
                    mat.SetColor(_color.id, _color.value);
                
                if (_float.id != 0)
                    mat.SetFloat(_float.id, _float.value);
            }
        }

        protected void OnValidate()
        {
            _color.OnValidate();
            _float.OnValidate();
        }

        private bool IsInstance(Material mat)
        {
            return mat && mat.name.StartsWith(matName, System.StringComparison.OrdinalIgnoreCase);
        }

        protected virtual void OnDisable()
        {
            if (!_renderer)
                return;
            
            _renderer.GetSharedMaterials(_materials);
            for (var i = _materials.Count-1; i >= 0; --i)
            {
                var mat = _materials[i];
                if (!IsInstance(mat))
                    continue;
                Destroy(mat);
                _materials.RemoveAt(i);
            }
            if (instance)
            {
                Destroy(instance);
                instance = null;
            }
            _renderer.sharedMaterials = _materials.ToArray();
            _materials.Clear();
            _renderer = null;
        }
    }
}