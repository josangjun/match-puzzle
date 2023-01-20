using System.Linq;
using UnityEngine;

namespace Studio.Framework
{
    public abstract class ToggleComponent : MonoBehaviour {
        public abstract bool toggle { get; set; }
    }

    public class ToggleGroup : ToggleComponent
    {
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

        [SerializeField]
        private ToggleComponent[] _components = null;

        public void Refresh()
        {
            if (_components == null || _components.Length == 0)
            {
                _components = GetComponentsInChildren<ToggleComponent>().Where(arg => !ReferenceEquals(arg, this)).ToArray();
            }
            var dirty = false;
            for (var i = 0; i < _components.Length; ++i)
            {
                var target = _components[i];
                if (ReferenceEquals(target, this))
                    continue;
                if (!(Object)target)
                {
                    dirty = true;
                    continue;
                }
                target.toggle = this.toggle;
            }
            if (dirty)
                _components = null;
        }

        private void OnValidate() {
            Refresh();
        }
    }
}