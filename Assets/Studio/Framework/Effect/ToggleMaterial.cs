using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Framework
{
    public class ToggleMaterial : ToggleComponent
    {
        [SerializeField]
        private Material _activate;
        [SerializeField]
        private Material _deactivate;

        [SerializeField]
        private bool _toggle;
        public override bool toggle
        {
            get => _toggle;
            set
            {
                _toggle = value;
                Refresh();
            }
        }

        private Renderer _renderer;

        private Renderer GetRenderer()
        {
            if (!_renderer)
                _renderer = GetComponent<Renderer>();
            return _renderer;
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_activate && _deactivate)
            {
                var renderer = GetRenderer();
                renderer.sharedMaterial = toggle ? _activate : _deactivate;
            }
        }

        private void OnValidate() {
            Refresh();
        }
    }
}