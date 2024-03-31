using UnityEditor;

// Adapted from RainbowArt.CleanFlatUI.GradientModifier, original content is provided below

namespace com.brg.UnityComponents
{
    [CustomEditor(typeof(GradientModifier))]
    public class GradientModifierEditor : Editor
    {
        private SerializedProperty _gradientStyle;        
        private SerializedProperty _blend; 
        private SerializedProperty _moreVertices; 
        private SerializedProperty _offset; 
        private SerializedProperty _scale; 
        private SerializedProperty _gradient;    

        protected virtual void OnEnable()
        {
            _gradientStyle = serializedObject.FindProperty("_gradientStyle");
            _blend = serializedObject.FindProperty("_blend");    
            _moreVertices = serializedObject.FindProperty("_moreVertices");    
            _offset = serializedObject.FindProperty("_offset");    
            _scale = serializedObject.FindProperty("_scale");    
            _gradient = serializedObject.FindProperty("_gradient");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_gradientStyle); 
            EditorGUILayout.PropertyField(_blend); 
            EditorGUILayout.PropertyField(_moreVertices); 
            EditorGUILayout.PropertyField(_offset); 
            EditorGUILayout.PropertyField(_scale); 
            EditorGUILayout.PropertyField(_gradient); 
            serializedObject.ApplyModifiedProperties();           
        }
    }
}

// namespace RainbowArt.CleanFlatUI
// {
//     [CustomEditor(typeof(GradientModifier))]
//     public class GradientModifierEditor : Editor
//     {
//         SerializedProperty gradientStyle;        
//         SerializedProperty blend; 
//         SerializedProperty moreVertices; 
//         SerializedProperty offset; 
//         SerializedProperty scale; 
//         SerializedProperty gradient;    
//
//         protected virtual void OnEnable()
//         {
//             gradientStyle = serializedObject.FindProperty("gradientStyle");
//             blend = serializedObject.FindProperty("blend");    
//             moreVertices = serializedObject.FindProperty("moreVertices");    
//             offset = serializedObject.FindProperty("offset");    
//             scale = serializedObject.FindProperty("scale");    
//             gradient = serializedObject.FindProperty("gradient");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             serializedObject.Update();
//             EditorGUILayout.PropertyField(gradientStyle); 
//             EditorGUILayout.PropertyField(blend); 
//             EditorGUILayout.PropertyField(moreVertices); 
//             EditorGUILayout.PropertyField(offset); 
//             EditorGUILayout.PropertyField(scale); 
//             EditorGUILayout.PropertyField(gradient); 
//             serializedObject.ApplyModifiedProperties();           
//         }
//     }
// }
