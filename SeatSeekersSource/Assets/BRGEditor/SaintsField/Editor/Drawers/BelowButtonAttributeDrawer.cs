﻿using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BelowButtonAttribute))]
    public class BelowButtonAttributeDrawer: DecButtonAttributeDrawer
    {

        #region IMGUI
        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: ImGuiHelpBox.GetHeight(DisplayError, width, MessageType.Error));


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            Rect leftRect = Draw(position, property, label, saintsAttribute, info, parent);

            if (DisplayError != "")
            {
                leftRect = ImGuiHelpBox.Draw(leftRect, DisplayError, MessageType.Error);
            }

            return leftRect;
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            visualElement.Add(DrawUIToolkit(property, saintsAttribute, index, info, parent, container));
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));
            return visualElement;
        }
        #endregion

#endif
    }
}
