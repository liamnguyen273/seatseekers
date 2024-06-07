﻿using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewAttributeDrawer : SaintsPropertyDrawer
    {
        private Texture2D _previewTexture;
        // private int _cachedWidth;
        // private int _cachedHeight;
        // private Texture2D _cachedWidthTexture;

        ~AssetPreviewAttributeDrawer()
        {
            // if (_previewTexture)
            // {
            //     Object.DestroyImmediate(_previewTexture);
            // }
            _previewTexture = null;

            // if (_cachedWidthTexture)
            // {
            //     Object.DestroyImmediate(_cachedWidthTexture);
            // }
        }

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            if (!_previewTexture)
            {
                return;
            }
            Object.DestroyImmediate(_previewTexture);
            _previewTexture = null;
        }

        private Texture2D GetPreview(Object target)
        {
            if (!target)
            {
                return null;
            }
            // if (property.propertyType != SerializedPropertyType.ObjectReference ||
            //     property.objectReferenceValue == null)
            // {
            //     return null;
            // }
            // Debug.Log($"check preview {_previewTexture?.width}");
            if(_previewTexture == null || _previewTexture.width == 1)
            {
                // Debug.Log($"load preview {target}");
                if(AssetPreview.IsLoadingAssetPreview(target.GetInstanceID()))
                {
                    return null;
                }

                ImGuiEnsureDispose(target);
                _previewTexture = AssetPreview.GetAssetPreview(target);
            }

            if (_previewTexture == null || _previewTexture.width == 1)
            {
                return null;
            }

            return _previewTexture;

            // bool widthOk = width == -1 || _previewTexture.width <= viewWidth;
            // bool heightOk = maxHeight == -1 && _previewTexture.height <= maxHeight;
            // if (widthOk && heightOk)
            // {
            //     return _cachedWidthTexture = _previewTexture;
            // }
            //
            // // Debug.Log($"viewWidth={viewWidth}, width={width}, maxHeight={maxHeight}, _previewTexture.width={_previewTexture.width}, _previewTexture.height={_previewTexture.height}");
            // (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, _previewTexture.width, _previewTexture.height);
            // // Debug.Log($"scaleWidth={scaleWidth}, scaleHeight={scaleHeight}");
            //
            // if (_cachedWidth == scaleWidth && _cachedHeight == scaleHeight && _cachedWidthTexture != null && _cachedWidthTexture.width != 1 && _cachedWidthTexture.height != 1)
            // {
            //     return _cachedWidthTexture;
            // }
            // _cachedWidth = scaleWidth;
            // _cachedHeight = scaleHeight;
            // // return _cachedWidthTexture = formatted;
            //
            // // _cachedWidthTexture = Tex.TextureTo(_previewTexture, scaleWidth, scaleHeight);
            // _cachedWidthTexture = _previewTexture;
            //
            // if (_cachedWidthTexture.width == 1)
            // {
            //     return _previewTexture;
            // }
            //
            // return _cachedWidthTexture;
        }

        // private static (int width, int height) GetPreviewScaleSize(int width, int maxHeight, float viewWidth, Texture previewTexture)
        // {
        //     return Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, previewTexture.width, previewTexture.height);
        // }

        #region IMGUI

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return ((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return 0;
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above? GetExtraHeight(property, width, saintsAttribute): 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return position;
            }

            return Draw(position, property, saintsAttribute);
        }


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            if (MismatchError(property) != null)
            {
                return true;
            }

            return !((AssetPreviewAttribute)saintsAttribute).Above;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
            }

            return ((AssetPreviewAttribute)saintsAttribute).Above? 0: GetExtraHeight(property, width, saintsAttribute);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            string error = MismatchError(property);
            if (error != null)
            {
                return ImGuiHelpBox.Draw(position, error, MessageType.Error);
            }

            return Draw(position, property, saintsAttribute);
        }

        private float GetExtraHeight(SerializedProperty property, float width, ISaintsAttribute saintsAttribute)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            // int maxWidth = assetPreviewAttribute.Width;
            // // int useWidth = maxWidth == -1? Mathf.FloorToInt(width): Mathf.Min(maxWidth, Mathf.FloorToInt(width));
            // int maxHeight = assetPreviewAttribute.Height;

            if (width - 1 < Mathf.Epsilon)
            {
                return 0;
            }

            Texture2D previewTexture = GetPreview(property.objectReferenceValue);
            if (previewTexture == null)
            {
                return 0;
            }

            float useWidth = assetPreviewAttribute.Align == EAlign.FieldStart
                ? Mathf.Max(width - EditorGUIUtility.labelWidth, 1)
                : width;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
            Debug.Log($"#AssetPreview# useWidth={useWidth}, width={width}, labelWidth={EditorGUIUtility.labelWidth}, Align={assetPreviewAttribute.Align}");
#endif

            (int width, int height) size =
                Tex.GetProperScaleRect(
                    Mathf.FloorToInt(useWidth),
                    assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                    previewTexture.width, previewTexture.height);
                // GetPreviewScaleSize(maxWidth, maxHeight, useWidth, previewTexture);
            // ReSharper disable once Unity.NoNullPropagation
            float result = size.height;
            // Debug.Log($"GetExtraHeight: {result}");
            return result;
        }

        private Rect Draw(Rect position, SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            // int maxWidth = assetPreviewAttribute.Width;
            // // int useWidth = maxWidth == -1? Mathf.FloorToInt(position.width): Mathf.Min(maxWidth, Mathf.FloorToInt(position.width));
            // // int maxHeight = Mathf.Min(assetPreviewAttribute.MaxHeight, Mathf.FloorToInt(position.height));
            // int maxHeight = assetPreviewAttribute.Height;

            // return position;

            if (position.width - 1 < Mathf.Epsilon)
            {
                return position;
            }

            Texture2D previewTexture = GetPreview(property.objectReferenceValue);

            if (previewTexture == null || previewTexture.width == 1)
            {
                return position;
            }

            // Debug.Log($"render texture {previewTexture.width}x{previewTexture.height} @ {position}");

            float useWidth = assetPreviewAttribute.Align == EAlign.FieldStart
                ? Mathf.Max(position.width - EditorGUIUtility.labelWidth, 1)
                : position.width;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
            Debug.Log($"useWidth={useWidth}, width={position.width}, labelWidth={EditorGUIUtility.labelWidth}, Align={assetPreviewAttribute.Align}");
#endif

            (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(
                Mathf.FloorToInt(useWidth),
                assetPreviewAttribute.Width, assetPreviewAttribute.Height,
                previewTexture.width, previewTexture.height);
                // GetPreviewScaleSize(maxWidth, maxHeight, useWidth, previewTexture);

            EAlign align = assetPreviewAttribute.Align;
            float xOffset;
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (align)
            {
                case EAlign.Start:
                    xOffset = 0;
                    break;
                case EAlign.Center:
                    xOffset = (position.width - scaleWidth) / 2;
                    break;
                case EAlign.End:
                    xOffset = position.width - scaleWidth;
                    break;
                case EAlign.FieldStart:
                    xOffset = EditorGUIUtility.labelWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(align), align, null);
            }

            Rect previewRect = new Rect(position)
            {
                x = position.x + xOffset,
                height = scaleHeight,
                width = scaleWidth,
            };

            // GUI.Label(previewRect, previewTexture);
            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);

            // EditorGUI.DrawRect(previewRect, Color.blue);

            Rect leftOutRect = RectUtils.SplitHeightRect(position, scaleHeight).leftRect;
            // Debug.Log($"leftOutRect={leftOutRect}");

            // return leftOutRect;
            return leftOutRect;
        }
        #endregion

        private static string MismatchError(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return $"Expect string or int, get {property.propertyType}";
            }
            return property.objectReferenceValue == null
                ? "field is null"
                : null;
        }

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameRoot(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__AssetPreview";
        private static string NameImage(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__AssetPreview_Image";
        private static string NameFakePlaceholder(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__AssetPreview_Placeholder";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (!assetPreviewAttribute.Above)
            {
                return null;
            }

            Texture2D preview = GetPreview(property.objectReferenceValue);
            return CreateUIToolkitElement(property, index, preview, assetPreviewAttribute);
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
            if (assetPreviewAttribute.Above)
            {
                return null;
            }

            Texture2D preview = GetPreview(property.objectReferenceValue);
            return CreateUIToolkitElement(property, index, preview, assetPreviewAttribute);
        }

        private struct Payload
        {
            // ReSharper disable InconsistentNaming
            public Object ObjectReferenceValue;
            public float ProcessedWidth;
            // ReSharper enable InconsistentNaming
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement root = container.Q<VisualElement>(NameRoot(property, index));
            Image image = root.Q<Image>(NameImage(property, index));
            Payload payload = (Payload)image.userData;
            // ReSharper disable once Unity.NoNullPropagation
            // int curInstanceId = property.objectReferenceValue?.GetInstanceID() ?? 0;

            // Debug.Log($"cur={curInstanceId}, pre={preInstanceId}");
            if (ReferenceEquals(payload.ObjectReferenceValue, property.objectReferenceValue))
            {
                return;
            }

            _previewTexture = null;
            root.style.display = DisplayStyle.None;

            if(property.objectReferenceValue == null)
            {
                image.userData = new Payload
                {
                    ObjectReferenceValue = null,
                    ProcessedWidth = payload.ProcessedWidth,
                };
                return;
            }

            Texture2D preview = GetPreview(property.objectReferenceValue);

            // UpdateImage(image, preview, (AssetPreviewAttribute)saintsAttribute);
            if (preview != null)
            {
                AssetPreviewAttribute assetPreviewAttribute = (AssetPreviewAttribute)saintsAttribute;
                SetImage(root, image, preview);
                OnRootGeoChanged(
                    root,
                    root.Q<Label>(NameFakePlaceholder(property, index)),
                    root.Q<Image>(NameImage(property, index)),
                    assetPreviewAttribute.Width, assetPreviewAttribute.Height);
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static VisualElement CreateUIToolkitElement(SerializedProperty property, int index, Texture2D preview,
            AssetPreviewAttribute assetPreviewAttribute)
        {
            VisualElement root = new VisualElement
            {
                name = NameRoot(property, index),
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            Label fakeLabel = Util.PrefixLabelUIToolKit(" ", 0);
            fakeLabel.name = NameFakePlaceholder(property, index);

            root.Add(fakeLabel);

            Image image = new Image
            {
                name = NameImage(property, index),
                scaleMode = ScaleMode.ScaleToFit,
                // ReSharper disable once Unity.NoNullPropagation
                // userData = property.objectReferenceValue?.GetInstanceID() ?? int.MinValue ,
                sourceRect = new Rect(0, 0, 0, 0),
                // style =
                // {
                //     flexGrow = 1,
                //     // flexShrink = 1,
                // },
            };

            switch (assetPreviewAttribute.Align)
            {
                case EAlign.Start:
                    fakeLabel.style.display = DisplayStyle.None;
                    break;
                case EAlign.Center:
                    fakeLabel.style.display = DisplayStyle.None;
                    root.style.justifyContent = Justify.Center;
                    break;
                case EAlign.End:
                    fakeLabel.style.display = DisplayStyle.None;
                    // image.style.alignSelf = Align.FlexEnd;
                    root.style.justifyContent = Justify.FlexEnd;
                    break;
                case EAlign.FieldStart:
                    fakeLabel.style.display = DisplayStyle.Flex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetPreviewAttribute.Align), assetPreviewAttribute.Align, null);
            }

            // UpdateImage(image, preview, assetPreviewAttribute);

            // image.AddToClassList(NameImage(property, index));

            SetImage(root, image, preview);
            if (preview == null)
            {
                image.userData = new Payload
                {
                    ObjectReferenceValue = null,
                    ProcessedWidth = 0,
                };
            }
            else
            {
                image.style.width = preview.width;
                image.style.height = preview.height;
                image.userData = new Payload
                {
                    ObjectReferenceValue = property.objectReferenceValue,
                    ProcessedWidth = 0,
                };
            }

            root.Add(image);

            root.RegisterCallback<GeometryChangedEvent>(_ => OnRootGeoChanged(root, fakeLabel, image, assetPreviewAttribute.Width, assetPreviewAttribute.Height));

            return root;
        }

        private static void SetImage(VisualElement root, Image image, Texture2D preview)
        {
            if (preview == null)
            {
                root.style.display = DisplayStyle.None;
                return;
            }

            root.style.display = DisplayStyle.Flex;
            image.image = preview;
        }

        private static void OnRootGeoChanged(VisualElement root, Label fakeLabel, Image image, int widthConfig, int heightConfig)
        {
            if(root.style.display == DisplayStyle.None)
            {
                return;
            }

            float rootWidth = root.resolvedStyle.width;

            if(rootWidth < Mathf.Epsilon)
            {
                return;
            }

            float fakeLabelWidth = fakeLabel.style.display == DisplayStyle.None
                ? 0
                : fakeLabel.resolvedStyle.width;

            float imageWidth = image.resolvedStyle.width;

            if(new []{rootWidth, fakeLabelWidth, imageWidth}.Any(float.IsNaN))
            {
                return;
            }

            Payload payload = (Payload)image.userData;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(payload.ProcessedWidth == rootWidth)
            {
                return;
            }

            image.userData = new Payload
            {
                ObjectReferenceValue = payload.ObjectReferenceValue,
                ProcessedWidth = rootWidth,
            };

            if (image.image == null)
            {
                return;
            }

            int maxWidth = Mathf.FloorToInt(rootWidth - fakeLabelWidth);
            int width = 0;
            int height = 0;
            if (maxWidth > 0)
            {
                Texture2D preview = (Texture2D)image.image;
                (width, height) = Tex.GetProperScaleRect(
                    maxWidth,
                    widthConfig, heightConfig, preview.width, preview.height);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_ASSET_PREVIEW
                Debug.Log($"{width}x{height}<-rootWidth={rootWidth}, fakeLabel={fakeLabelWidth}, widthConfig={widthConfig}, heightConfig={heightConfig}, preview={preview.width}x{preview.height}");
#endif
            }

            image.style.width = Mathf.Max(width, 0);
            image.style.height = Mathf.Max(height, 0);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        // private static void UpdateImage(Image image, Texture2D preview, AssetPreviewAttribute assetPreviewAttribute)
        // {
        //     image.image = preview;
        //     if (preview == null)
        //     {
        //         image.style.display = DisplayStyle.None;
        //         return;
        //     }
        //
        //     image.style.display = DisplayStyle.Flex;
        //
        //     if (assetPreviewAttribute.MaxWidth > 0 || assetPreviewAttribute.MaxHeight > 0)
        //     {
        //         // image.style.width = 500;
        //         // image.style.height = StyleKeyword.Auto;
        //         // int imageWidth = preview.width;
        //         // int imageHeight = preview.height;
        //         // int maxHeight = assetPreviewAttribute.MaxHeight > 0? assetPreviewAttribute.MaxHeight: imageWidth;
        //         // int maxWidth = assetPreviewAttribute.MaxWidth > 0? assetPreviewAttribute.MaxWidth: imageHeight;
        //         //
        //         // (int fittedWidth, int fittedHeight) = Tex.FitScale(maxWidth, maxHeight, imageWidth, imageHeight);
        //         //
        //         // image.style.maxWidth = fittedWidth;
        //         // image.style.maxHeight = fittedHeight;
        //     }
        //     else
        //     {
        //         image.style.maxWidth = image.style.maxHeight = StyleKeyword.Null;
        //     }
        //
        // }
        #endregion

#endif
    }
}
