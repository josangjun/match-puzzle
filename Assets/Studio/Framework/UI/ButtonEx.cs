using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Studio.Framework
{
    public class ButtonEx : UnityEngine.UI.Button
    {
        public float clickInterval = 2f;
        private float _clickTime;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (interactable == false)
                return;
            var elapsed = Time.time - _clickTime;
            if (elapsed < clickInterval)
            {
                return;
            }
            _clickTime = Time.time;

            base.OnPointerClick(eventData);
        }
    }
}

#if UNITY_EDITOR
namespace Studio.Framework.EditorInternal
{
    using UnityEditor;
    using UnityEditor.UI;

    [CustomEditor(typeof(ButtonEx), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Button Component.
    ///   Extend this class to write a custom editor for a component derived from Button.
    /// </summary>
    public class ButtonExEditor : ButtonEditor
    {
        SerializedProperty m_OnClickProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var clickInterval = serializedObject.FindProperty("clickInterval");
            EditorGUILayout.PropertyField(clickInterval);
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif