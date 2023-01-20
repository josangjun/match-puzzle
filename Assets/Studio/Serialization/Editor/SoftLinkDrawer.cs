using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;

namespace Studio.Common.EditorInternal
{
    using System.Collections.Generic;
    [CustomPropertyDrawer(typeof(SoftLink<>))]
    public class SoftLinkDrawer : PropertyDrawer
    {
        protected SerializedProperty nameProp;
        protected SerializedProperty pathProp;
        protected SerializedProperty guidProp;

        private ResourceAnchorAttribute anchorAttrb;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (anchorAttrb != null && anchorAttrb.height > 0f)
                return anchorAttrb.height;
            var type = GetFieldType();
            if (type.IsSubclassOf(typeof(Texture)))
                return 48f;
            return base.GetPropertyHeight(property, label);
        }

        protected virtual System.Type GetFieldType()
        {
            if (anchorAttrb != null)
                return anchorAttrb.type;

            var type = fieldInfo.FieldType;
            if (type.IsArray)
            {
                return type.GetElementType().GenericTypeArguments[0];
            }
            if (typeof(List<>) == type.GetGenericTypeDefinition())
            {
                var elementType = type.GenericTypeArguments[0];
                var resourceType = elementType.GenericTypeArguments[0];
                return resourceType;
            }
            return type.GenericTypeArguments[0];
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            anchorAttrb = attribute as ResourceAnchorAttribute;

            if (property.propertyType != SerializedPropertyType.Generic &&
                property.propertyType != SerializedPropertyType.ObjectReference)
            {
                base.OnGUI(position, property, label);
                return;
            }

            nameProp = property.FindPropertyRelative("name");
            pathProp = property.FindPropertyRelative("path");
            guidProp = property.FindPropertyRelative("guid");

            if (pathProp == null || guidProp == null || nameProp == null)
            {
                base.OnGUI(position, property, label);
                return;
            }
            EditorGUI.BeginProperty(position, label, property);
            //Debug.LogFormat("filedName:{0}, target:{1}, time:{2}",
            //    property.displayName, property.serializedObject.targetObject.GetType(), Time.time);

            DrawProperty(position, property);

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public T GetCachedObject<T>(string guid) where T : Object
        {
            if (anchorAttrb != null)
                return anchorAttrb.GetCachedObject<T>(guid);

            var path = string.IsNullOrEmpty(guid) ? pathProp.stringValue : AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<T>(path);
            return obj;
        }

        protected virtual void DrawProperty(Rect position, SerializedProperty property)
        {
            try
            {
                var pt = GUIUtility.GUIToScreenPoint(position.position);
                if (pt.y < -EditorGUIUtility.singleLineHeight || pt.y > Screen.height)
                    return;
                
                var fieldName = property.displayName;
                var fieldType = GetFieldType();
                var asset = GetCachedObject<Object>(
                    guidProp.stringValue);
                if (asset == null)
                {
                    pathProp.stringValue = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
                    
                    asset = GetCachedObject<Object>(guidProp.stringValue);
                }
                
                var alloc = EditorGUI.ObjectField(position, fieldName, asset, fieldType, false);
                
                //if (alloc != asset)
                {
                    pathProp.stringValue = AssetDatabase.GetAssetPath(alloc);
                    guidProp.stringValue = AssetDatabase.AssetPathToGUID(pathProp.stringValue);
                    nameProp.stringValue = alloc ? alloc.name : string.Empty;
                    if (!string.IsNullOrEmpty(guidProp.stringValue))
                        EditorExtension.MakeAddressFromGUID(guidProp.stringValue);
                }

                property.serializedObject.ApplyModifiedProperties();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    public static class EditorExtension
    {
        public static bool MakeAddress(this UnityEngine.Object asset)
        {
            if (asset == null)
                return false;
            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);
            return MakeAddressFromGUID(guid);
        }

        public static bool HasAddress(string guid)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(guid);
            return entry != null;
        }

        public static bool MakeAddressFromGUID(string guid)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
                return false;
            var assetRef = (AssetReference)settings.CreateAssetReference(guid);
            return true;
        }
    }
}