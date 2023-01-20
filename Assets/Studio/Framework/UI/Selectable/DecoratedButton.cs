using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEngine.Events;
using System.Collections;

namespace Studio.Framework
{
    public class DecoratedButton : ButtonEx
    {
        [SerializeField]
        private Selectable[] _targets = new Selectable[0];
        
        private MethodInfo _method;

        private MethodInfo GetTransitionMethod()
        {
            if (_method == null)
                _method = typeof(Selectable).GetMethod(
                    "DoStateTransition", BindingFlags.NonPublic|BindingFlags.Instance, null,
                    new[] { typeof(SelectionState), typeof(bool) }, null);
            return _method;
        }

        private object[] parameters = new object[2];

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            parameters[0] = state;
            parameters[1] = instant;
            var method = GetTransitionMethod();
            foreach (var target in _targets)
            {
                if (target == null) continue;
                target.interactable = interactable;
                method.Invoke(target, parameters);
            }
        }
    }
}
#if UNITY_EDITOR
namespace Studio.Framework.EditorInternal
{
    using UnityEngine;
    using UnityEditor;
    using Studio.Framework;

    [CustomEditor(typeof(DecoratedButton))]
    public class DecoratedButtonEditor : ButtonExEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var targets = serializedObject.FindProperty("_targets");
            EditorGUILayout.PropertyField(targets);
        }
    }
}

#endif