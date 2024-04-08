// #define DEBUG_VIEW_PARENT

using System;
using System.Reflection;
using com.brg.Common;
using UnityEditor;
using UnityEngine;
using GUIContent = UnityEngine.GUIContent;

namespace com.brg.UnityCommon.Editor
{
    public class CommonWrapperPropertyDrawer: PropertyDrawer
    {
        private bool _validated = false;
        // public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        // {
        //     var height = EditorGUI.GetPropertyHeight(property, label);
        //     return height;
        // }

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(pos, label, property);

            // Draw label
            pos = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
            
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var offset = 40f;
            var buttonWidth = 70f;
            var pathRect = new Rect(pos.x - offset, pos.y, pos.width + offset - buttonWidth, pos.height);
            EditorGUI.PropertyField(pathRect, property.FindPropertyRelative("_path"), GUIContent.none);

            if (!_validated)
            {
                Validate(property, false);
            }
            
            var buttonRect = new Rect(pos.x + pos.width - buttonWidth, pos.y, buttonWidth, pos.height);
            if (GUI.Button(buttonRect, "Validate"))
            {
                Validate(property, true);
            }
            
            EditorGUILayout.BeginVertical();

            var goLabel = new GUIContent("GO");
            var parentLabel = new GUIContent("Parent");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField(goLabel, GUILayout.Width(30));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("_comp"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            
#if DEBUG_VIEW_PARENT
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField(parentLabel, GUILayout.Width(30));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("_parent"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
#endif

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
            
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        protected virtual void Validate(SerializedProperty property, bool shouldLog)
        {
            _validated = true;
        }
    }

    [CustomPropertyDrawer(typeof(CompWrapper<>))]
    public class CompWrapperPropertyDrawer : CommonWrapperPropertyDrawer
    {
        protected override void Validate(SerializedProperty property, bool shouldLog)
        {
            var c = property.serializedObject.targetObject as Component;
            var cType = property.serializedObject.targetObject.GetType();
            
            if (c is null) return;
            
            var fieldType = fieldInfo!.FieldType;
            Type tType = null;

            if (fieldType.IsArray)
            {
                var elementType = fieldType.GetElementType();
                tType = elementType!.GetGenericArguments()[0];
            }
            else
            {
                tType = fieldType.GetGenericArguments()[0];
            }
            
            var parentProp = property.FindPropertyRelative("_parent")!;
            var pathProp = property.FindPropertyRelative("_path")!;
            var compProp = property.FindPropertyRelative("_comp")!;

            var path = pathProp.stringValue ?? string.Empty;
            var comp = compProp.objectReferenceValue;
            
            var pathIsRelative = path.Length switch
            {
                >= 2 when path[0] == '.' && path[1] == '/' => true,
                >= 1 when path[0] == '/' => false,
                _ => false
            };
            
            var baseGo = c.gameObject;

            var goAtPath = baseGo.TraversePath(pathIsRelative, path);
            var compAtPath = goAtPath is null ? null : goAtPath.GetComponent(tType);
                
            if (comp is null && compAtPath is null)
            {
                // No comp at path, default to self
                parentProp.objectReferenceValue = baseGo;
                if (shouldLog) Debug.LogError($"CompWrapper: Cannot find a \"{tType}\" at path \"{path}\".");
            }
            else if (comp is null && compAtPath is not null)
            {
                // CompAtPath is now comp.
                compProp.objectReferenceValue = compAtPath;
                pathProp.stringValue = path;
                parentProp.objectReferenceValue = pathIsRelative ? baseGo : null;
            }
            else if (comp is not null && compAtPath is null)
            {
                // Regenerate path
                pathProp.stringValue = (comp as Component)!.gameObject.RegeneratePathUpTo(baseGo, out var relative);
                parentProp.objectReferenceValue = relative ? baseGo : null;
            }
            else // comp is not null && compAtPath is not null
            {
                var compPath = (comp as Component)!.gameObject.RegeneratePathUpTo(baseGo, out var compPathIsRelative);
                if (comp == compAtPath)
                {
                    // Cool, set parent though.
                    parentProp.objectReferenceValue = pathIsRelative ? baseGo : null;
                }
                else
                {
                    // Prioritize the component that is relative
                    if (pathIsRelative)
                    {
                        compProp.objectReferenceValue = compAtPath;
                        parentProp.objectReferenceValue = baseGo;
                    }
                    else if (compPathIsRelative)
                    {
                        // Keep as is
                        parentProp.objectReferenceValue = baseGo;
                        pathProp.stringValue = compPath;
                    }
                    else
                    {
                        // Prioritize the compPath
                        compProp.objectReferenceValue = compAtPath;
                        parentProp.objectReferenceValue = null;
                    }
                }
            }
            
            base.Validate(property, shouldLog);
        }
    }
    
    [CustomPropertyDrawer(typeof(GOWrapper))]
    public class GameObjectWrapperPropertyDrawer : CommonWrapperPropertyDrawer
    {
        protected override void Validate(SerializedProperty property, bool shouldLog)
        {
            var c = property.serializedObject.targetObject as Component;

            if (c is null) return;
            
            var parentProp = property.FindPropertyRelative("_parent")!;
            var pathProp = property.FindPropertyRelative("_path")!;
            var compProp = property.FindPropertyRelative("_comp")!;
                
            var path = pathProp.stringValue ?? ".";
            var go = compProp.objectReferenceValue as GameObject;
            
            var pathIsRelative = path.Length switch
            {
                >= 2 when path[0] == '.' && path[1] == '/' => true,
                >= 1 when path[0] == '/' => false,
                _ => false
            };
            
            var baseGo = c.gameObject;

            var goAtPath = baseGo.TraversePath(pathIsRelative, path);
                
            if (go is null && goAtPath == null)
            {
                // No comp at path
                if (shouldLog) Debug.LogError($"GOWrapper: Cannot find a GameObject at path \"{path}\".");
            }
            else if (go is null && goAtPath != null)
            {
                // goAtPath is now comp.
                compProp.objectReferenceValue = goAtPath;
                pathProp.stringValue = path;
                parentProp.objectReferenceValue = pathIsRelative ? baseGo : null;
            }
            else if (go is not null && goAtPath == null)
            {
                // Regenerate path
                pathProp.stringValue = go.RegeneratePathUpTo(baseGo, out var relative);
                parentProp.objectReferenceValue = relative ? baseGo : null;
            }
            else // go is not null && compAtPath != null
            {
                var goPath = go!.RegeneratePathUpTo(baseGo, out var goPathIsRelative);
                if (go == goAtPath)
                {
                    // Cool, set parent though.
                    parentProp.objectReferenceValue = pathIsRelative ? baseGo : null;
                }
                else
                {
                    // Prioritize the game object that is relative
                    if (pathIsRelative)
                    {
                        compProp.objectReferenceValue = goAtPath;
                        parentProp.objectReferenceValue = baseGo;
                    }
                    else if (goPathIsRelative)
                    {
                        // Keep as is
                        parentProp.objectReferenceValue = baseGo;
                        pathProp.stringValue = goPath;
                    }
                    else
                    {
                        // Prioritize the go Path
                        compProp.objectReferenceValue = goAtPath;
                        parentProp.objectReferenceValue = null;
                    }
                }
            }
            
            base.Validate(property, shouldLog);
        }
    }
}