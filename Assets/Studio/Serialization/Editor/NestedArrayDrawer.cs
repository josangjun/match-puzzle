using UnityEditor;
using UnityEngine;

namespace NaughtyAttributes.Editor
{
	[CustomPropertyDrawer(typeof(NestedArrayAttribute))]
	public class NestedArrayDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect _, SerializedProperty property, GUIContent __)
		{
			var childDepth = property.depth + 1;
			EditorGUILayout.PropertyField(property, includeChildren: false);
			if (!property.isExpanded)
			{
				return;
			}
			EditorGUI.indentLevel++;
			foreach (SerializedProperty child in property)
			{
				if (child.depth == childDepth && PropertyUtility.IsVisible(child))
				{
					EditorGUILayout.PropertyField(child);
				}
			}
			EditorGUI.indentLevel--;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, includeChildren: false) - 20;
		}
	}
}