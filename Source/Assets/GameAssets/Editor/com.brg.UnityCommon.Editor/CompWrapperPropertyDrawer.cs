using System.Reflection;
using UnityEditor;
using UnityEngine;
using GUIContent = UnityEngine.GUIContent;

namespace com.brg.UnityCommon.Editor
{
    public class CommonWrapperPropertyDrawer: PropertyDrawer
    {
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
            
            var buttonRect = new Rect(pos.x + pos.width - buttonWidth, pos.y, buttonWidth, pos.height);
            if (GUI.Button(buttonRect, "Validate"))
            {
                Validate(property);
            }
            
            EditorGUILayout.BeginVertical();

            var goLabel = new GUIContent("GO");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField(goLabel, GUILayout.Width(30));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("_comp"), GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
            
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        protected virtual void Validate(SerializedProperty property)
        {
            // do nothing
        }
    }

    [CustomPropertyDrawer(typeof(CompWrapper<>))]
    public class CompWrapperPropertyDrawer : CommonWrapperPropertyDrawer
    {
        protected override void Validate(SerializedProperty property)
        {
            var c = property.serializedObject.targetObject as Component;
            var cType = property.serializedObject.targetObject.GetType();
            var field = cType.GetField(property.propertyPath, BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldType = field!.FieldType;
            var tType = fieldType.GetGenericArguments()[0];
            
            // Debug.Log(tType);
            
            if (c is not null)
            {
                var pathProp = property.FindPropertyRelative("_path")!;
                var compProp = property.FindPropertyRelative("_comp")!;
                
                var path = pathProp.stringValue ?? ".";
                var comp = compProp.objectReferenceValue;

                bool isRelative = path == "." || (path.Length >= 2 && path[0] == '.' && path[1] == '/');
                path = path == "." ? "" : (isRelative ? path.Substring(2) : path);
                var baseGo = c.gameObject;

                // If both exists, validate path first, if that fails, regenerate
                if (!path.IsNullOrEmpty() && comp is not null)
                {
                    var compGo = baseGo.TraversePath(isRelative, path);
                    var newComp = compGo?.GetComponent(tType);
                    if (newComp is not null && newComp == comp)
                    {
                        // Validation completed, no errors
                        return;
                    }
                    else if (newComp is not null)
                    {
                        // Reset the comp to follow path
                        compProp.objectReferenceValue = newComp;
                    }
                    else
                    {
                        // Reset path to follow comp.
                        pathProp.stringValue = (comp as Component)!.gameObject.RegeneratePathUpTo(baseGo);
                    }
                }
                // If path is null, regenerate path
                else if (path.IsNullOrEmpty())
                {
                    pathProp.stringValue = (comp as Component)!.gameObject.RegeneratePathUpTo(baseGo);
                }
                // If missing comp, get from path
                else if (comp is null)
                {
                    var compGo = baseGo.TraversePath(isRelative, path);

                    if (compGo is null)
                    {
                        Debug.LogError($"Cannot find component at path \"{path}\"");
                        return;
                    }
                    
                    compProp.objectReferenceValue = compGo.GetComponent(tType);
                }
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(GOWrapper))]
    public class GameObjectWrapperPropertyDrawer : CommonWrapperPropertyDrawer
    {
        protected override void Validate(SerializedProperty property)
        {
            var c = property.serializedObject.targetObject as Component;
            
            if (c is not null)
            {
                var pathProp = property.FindPropertyRelative("_path")!;
                var compProp = property.FindPropertyRelative("_comp")!;
                
                var path = pathProp.stringValue ?? ".";
                var gameObject = compProp.objectReferenceValue as GameObject;

                bool isRelative = path == "." || (path.Length >= 2 && path[0] == '.' && path[1] == '/');
                path = path == "." ? "" : (isRelative ? path.Substring(2) : path);
                var baseGo = c.gameObject;

                // If both exists, validate path first, if that fails, regenerate
                if (!path.IsNullOrEmpty() && gameObject is not null)
                {
                    var newGameObject = baseGo.TraversePath(isRelative, path);
                    if (newGameObject is not null && newGameObject == gameObject)
                    {
                        // Validation completed, no errors
                        return;
                    }
                    else if (newGameObject is not null)
                    {
                        // Reset the comp to follow path
                        compProp.objectReferenceValue = newGameObject;
                    }
                    else
                    {
                        // Reset path to follow comp.
                        pathProp.stringValue = gameObject.RegeneratePathUpTo(baseGo);
                    }
                }
                // If path is null, regenerate path
                else if (path.IsNullOrEmpty())
                {
                    pathProp.stringValue = gameObject.RegeneratePathUpTo(baseGo);
                }
                // If missing comp, get from path
                else if (gameObject is null)
                {
                    var compGo = baseGo.TraversePath(isRelative, path);

                    if (compGo is null)
                    {
                        Debug.LogError($"Cannot find component at path \"{path}\"");
                        return;
                    }

                    compProp.objectReferenceValue = compGo;
                }
            }
        }
    }
}