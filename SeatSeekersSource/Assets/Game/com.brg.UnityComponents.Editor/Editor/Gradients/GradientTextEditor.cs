using UnityEditor;
using TMPro.EditorUtilities;

// Adapted from RainbowArt.CleanFlatUI.GradientModifier, original content is provided below

namespace com.brg.UnityComponents
{
    [CustomEditor(typeof(GradientText))]
    public class GradientTextEditor : TMP_EditorPanelUI
    {
        private SerializedProperty _colorGradientLine;
        private SerializedProperty _gradientColors;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _colorGradientLine = serializedObject.FindProperty("_colorGradientLine");
            _gradientColors = serializedObject.FindProperty("_gradientColors");            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_colorGradientLine);
            if (_colorGradientLine.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_gradientColors);
                --EditorGUI.indentLevel;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

// namespace RainbowArt.CleanFlatUI
// {
//     [CustomEditor(typeof(GradientText))]
//     public class GradientTextEditor : TMP_EditorPanelUI
//     {
//         SerializedProperty colorGradientLine;
//         SerializedProperty gradientColors;
//         
//         protected override void OnEnable()
//         {
//             base.OnEnable();
//             colorGradientLine = serializedObject.FindProperty("colorGradientLine");
//             gradientColors = serializedObject.FindProperty("gradientColors");            
//         }
//
//         public override void OnInspectorGUI()
//         {
//             base.OnInspectorGUI();
//             serializedObject.Update();
//             EditorGUILayout.PropertyField(colorGradientLine);
//             if (colorGradientLine.boolValue)
//             {
//                 ++EditorGUI.indentLevel;
//                 EditorGUILayout.PropertyField(gradientColors);
//                 --EditorGUI.indentLevel;
//             }
//             serializedObject.ApplyModifiedProperties();
//         }
//     }
// }
