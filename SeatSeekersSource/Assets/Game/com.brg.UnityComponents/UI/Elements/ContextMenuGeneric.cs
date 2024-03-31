using System;
using System.Collections;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.UnityCommon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

// Adapted from namespace RainbowArt.CleanFlatUI, original content is below.

namespace com.brg.UnityComponents
{
    public class ContextMenuGeneric : MonoBehaviour, IPointerDownHandler
    {
        protected internal class ContextMenuItem : MonoBehaviour, IPointerEnterHandler
        {
            public TextMeshProUGUI itemText;
            public Image itemImage;
            public Image itemLine;
            public Button button;

            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        
        [Serializable]
        public class OptionItem
        {
            [FormerlySerializedAs("text")] public string Text;
            [FormerlySerializedAs("icon")] public Sprite Icon;
            public EventWrapper ClickEvent;

            public OptionItem() { }

            public OptionItem(string newText)
            {
                Text = newText;
            }

            public OptionItem(Sprite newImage)
            {
                Icon = newImage;
            }

            public OptionItem(string newText, Sprite newImage)
            {
                Text = newText;
                Icon = newImage;
            }

            public OptionItem(string text, Sprite icon, Action callback)
            {
                Text = text;
                Icon = icon;
                ClickEvent += callback;
            }
        }
        
        private enum Origin
        {
            RightBottom,
            LeftBottom,
            RightTop,
            LeftTop,
        }
        
        private const float DISABLE_TIME = 0.15f;
        private const float DISTANCE_X = 2.0f;
        private const float DISTANCE_Y = 2.0f;
        private const string ANIM_IN_RIGHT_BOTTOM = "In Right Bottom";
        private const string ANIM_IN_RIGHT_TOP = "In Right Top";
        private const string ANIM_IN_LEFT_TOP = "In Left Top";
        private const string ANIM_IN_LEFT_BOTTOM = "In Left Bottom";
        private const string ANIM_OUT_RIGHT_BOTTOM = "Out Right Bottom";
        private const string ANIM_OUT_RIGHT_TOP = "Out Right Top";
        private const string ANIM_OUT_LEFT_TOP = "Out Left Top";
        private const string ANIM_OUT_LEFT_BOTTOM = "Out Left Bottom";

        [Header("Items")]
        [SerializeField] private GameObject _itemTemplate;
        [SerializeField] private TextMeshProUGUI _itemText;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _itemLine;

        [Header("Components")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private RectTransform _clickerBlocker;
        [SerializeField] private CompAnyPlayable _playable;
        [SerializeField] private RectOffset _padding = new ();
        [SerializeField] private float _spacing = 2;

        [Header("Options")]
        [SerializeField] List<OptionItem> _options = new ();
        [SerializeField] public EventWrapper<int, OptionItem> ValueChangedEvent;

        private Canvas _rootCanvas = null;
        private Origin _origin = Origin.RightBottom;

        private readonly List<ContextMenuItem> _menuItems = new ();
        private IEnumerator _disableCoroutine;

        public RectOffset Padding
        {
            get => _padding;
            set => _padding = value;
        }

        public float Spacing
        {
            get => _spacing;
            set => _spacing = value;
        }

        public void Awake()
        {
            Hide(false);
        }

        public void Show(Vector2 mousePosition, RectTransform areaScope)
        {
            if (_options.Count <= 0) return;
            
            gameObject.SetActive(true);
            DestroyAllMenuItems();
            SetupTemplate();
            CreateAllMenuItems(_options);
            UpdatePosition(mousePosition, areaScope);
            CreateClickBlocker();
            PlayAnimation(true);
        }

        public void Show(Vector2 mousePosition, RectTransform areaScope, List<OptionItem> options)
        {
            _options.Clear();
            AddOptions(options);
            
            Show(mousePosition, areaScope);
        }

        public void AddOptions(List<OptionItem> optionList)
        {
            if (optionList is null || optionList.Count == 0) return;
            
            _options.AddRange(optionList);
        }

        public void AddOptions(List<string> optionList)
        {
            foreach (var t in optionList)
            {
                _options.Add(new OptionItem(t));
            }
        }

        public void AddOptions(List<Sprite> optionList)
        {
            foreach (var t in optionList)
            {
                _options.Add(new OptionItem(t));
            }
        }

        public void ClearOptions()
        {
            _options.Clear();
        }

        private void SetupTemplate()
        {
            var menuItem = _itemTemplate.GetComponent<ContextMenuItem>();
            if (menuItem == null)
            {
                menuItem = _itemTemplate.AddComponent<ContextMenuItem>();
                menuItem.itemText = _itemText;
                menuItem.itemImage = _itemImage;
                menuItem.itemLine = _itemLine;
                menuItem.button = _itemTemplate.GetComponent<Button>();
            }

            _itemTemplate.SetActive(false);
        }

        private void CreateAllMenuItems(List<OptionItem> options)
        {
            var itemWidth = _itemTemplate.GetComponent<RectTransform>().rect.width;
            
            var dataCount = options.Count;
            float curY = -_padding.top;
            for (var i = 0; i < dataCount; ++i)
            {
                var itemData = options[i];
                var index = i;
                
                var go = Instantiate(_itemTemplate, _itemTemplate.gameObject.transform.parent, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                
                go.SetActive(true);
                go.name = "MenuItem" + i;
                
                var curMenuItem = go.GetComponent<ContextMenuItem>();
                _menuItems.Add(curMenuItem);
                curMenuItem.itemText.text = itemData.Text;
                if (itemData.Icon == null)
                {
                    curMenuItem.itemImage.gameObject.SetActive(false);
                    curMenuItem.itemImage.sprite = null;
                }
                else
                {
                    curMenuItem.itemImage.gameObject.SetActive(true);
                    curMenuItem.itemImage.sprite = itemData.Icon;
                }

                if (curMenuItem.itemLine != null)
                {
                    curMenuItem.itemLine.gameObject.SetActive(i != dataCount - 1);
                }

                curMenuItem.button.onClick.RemoveAllListeners();
                curMenuItem.button.onClick.AddListener(delegate { OnItemClicked(index); });
                curMenuItem.button.onClick.AddListener(delegate { itemData.ClickEvent?.Invoke(); });
                
                var curRectTransform = go.GetComponent<RectTransform>();
                curRectTransform.anchoredPosition3D = new Vector3(_padding.left, curY, 0);
                var curItemHeight = curRectTransform.rect.height;
                
                curY -= curItemHeight;
                if (i < dataCount - 1)
                {
                    curY -= _spacing;
                }
            }
            
            GetComponent<RectTransform>().ForceUpdateRectTransforms();

            gameObject.GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(curY) + _padding.bottom);
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                itemWidth + _padding.left + _padding.right);
        }
        
        private Canvas GetRootCanvas()
        {
            if (_rootCanvas is null)
            {
                var list = new List<Canvas>();
                gameObject.GetComponentsInParent(false, list);
            
                if (list.Count == 0)
                {
                    return null;
                }

                var listCount = list.Count;
                var rootCanvas = list[listCount - 1];
                for (var i = 0; i < listCount; i++)
                {
                    if (!list[i].isRootCanvas && !list[i].overrideSorting) continue;
                
                    rootCanvas = list[i];
                    break;
                }

                _rootCanvas = rootCanvas;
            }
            
            return _rootCanvas;
        }

        private RectTransform GetRootCanvasRectTrans()
        {
            var rootCanvas = GetRootCanvas();
            return rootCanvas == null ? null : rootCanvas.GetComponent<RectTransform>();
        }

        private void CreateClickBlocker()
        {
            var rootCanvas = GetRootCanvas();
            if (rootCanvas == null)
            {
                return;
            }
            
            _clickerBlocker.anchorMin = new Vector2(0.5f, 0.5f);
            _clickerBlocker.anchorMax = new Vector2(0.5f, 0.5f);
            _clickerBlocker.pivot = new Vector2(0.5f, 0.5f);
            
            var rootCanvasRect = rootCanvas.GetComponent<RectTransform>().rect;
            var rootCanvasWidth = rootCanvasRect.width;
            var rootCanvasHeight = rootCanvasRect.height;
            
            _clickerBlocker.SetParent(rootCanvas.transform, false);
            _clickerBlocker.localPosition = Vector3.zero;
            _clickerBlocker.SetParent(gameObject.transform, true);
            _clickerBlocker.SetAsFirstSibling();
            _clickerBlocker.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rootCanvasHeight);
            _clickerBlocker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rootCanvasWidth);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Hide(true);
        }

        private void OnItemClicked(int index)
        {
            ValueChangedEvent.Invoke(index, _options[index]);
            Hide(true);
        }

        private void DestroyAllMenuItems()
        {
            var itemsCount = _menuItems.Count;
            for (int i = 0; i < itemsCount; i++)
            {
                if (_menuItems[i] != null)
                {
                    Destroy(_menuItems[i].gameObject);
                }
            }

            _menuItems.Clear();
        }

        public void Hide(bool playAnim)
        {
            if (_disableCoroutine != null)
            {
                StopCoroutine(_disableCoroutine);
                _disableCoroutine = null;
            }

            if (playAnim)
            {
                PlayAnimation(false);
                _disableCoroutine = DisableTransition();
                StartCoroutine(_disableCoroutine);
            }
            else
            {
                if (_playable != null)
                {
                    _panel.localScale = Vector3.one;
                    _panel.localEulerAngles = Vector3.zero;
                }

                gameObject.SetActive(false);
                DestroyAllMenuItems();
            }
        }

        private IEnumerator DisableTransition()
        {
            yield return new WaitForSeconds(DISABLE_TIME);
            gameObject.SetActive(false);
            DestroyAllMenuItems();
        }

        public bool IsShowing()
        {
            return gameObject.activeSelf;
        }

        private void UpdatePosition(Vector2 mousePosition, RectTransform areaScope)
        {
            if (areaScope == null)
            {
                areaScope = GetRootCanvasRectTrans();
                if (areaScope == null)
                {
                    return;
                }
            }

            var anchorPosArea = areaScope.anchoredPosition;
            var localMousePos = mousePosition - anchorPosArea;

            var selfRect = GetComponent<RectTransform>();
            selfRect.localPosition = new Vector3(localMousePos.x, localMousePos.y, 0);
            var corners = new Vector3[4];
            selfRect.GetWorldCorners(corners);
            var cornersInArea = new Vector3[4];
            float correctionX = 0;
            float correctionY = 0;
            
            for (int i = 0; i < 4; i++)
            {
                cornersInArea[i] = areaScope.InverseTransformPoint(corners[i]);
            }

            if (cornersInArea[2].x >= areaScope.rect.xMax)
            {
                if (cornersInArea[0].x - selfRect.rect.width < areaScope.rect.xMin)
                {
                    correctionX = cornersInArea[0].x - selfRect.rect.width - areaScope.rect.xMin;
                }

                if (cornersInArea[0].y < areaScope.rect.yMin)
                {
                    _origin = Origin.LeftTop;
                    if (cornersInArea[2].y + selfRect.rect.height > areaScope.rect.yMax)
                    {
                        correctionY = cornersInArea[2].y + selfRect.rect.height - areaScope.rect.yMax;
                    }
                }
                else
                {
                    _origin = Origin.LeftBottom;
                }
            }
            else if (cornersInArea[0].y < areaScope.rect.yMin)
            {
                _origin = Origin.RightTop;
                if (cornersInArea[2].y + selfRect.rect.height > areaScope.rect.yMax)
                {
                    correctionY = cornersInArea[2].y + selfRect.rect.height - areaScope.rect.yMax;
                }
            }
            else
            {
                _origin = Origin.RightBottom;
            }

            var pos = selfRect.localPosition;
            var selfWidth = selfRect.rect.width;
            var selfHeight = selfRect.rect.height;
            switch (_origin)
            {
                case Origin.RightBottom:
                {
                    pos.x = pos.x + DISTANCE_X;
                    pos.y = pos.y - DISTANCE_Y;
                    break;
                }
                case Origin.RightTop:
                {
                    pos.x = pos.x + DISTANCE_X;
                    if (correctionY == 0)
                    {
                        pos.y = pos.y + selfHeight + DISTANCE_Y;
                    }
                    else
                    {
                        pos.y = pos.y + selfHeight - correctionY;
                    }

                    break;
                }
                case Origin.LeftTop:
                {
                    if (correctionX == 0)
                    {
                        pos.x = pos.x - selfWidth - DISTANCE_X;
                    }
                    else
                    {
                        pos.x = pos.x - selfWidth - correctionX;
                    }

                    if (correctionY == 0)
                    {
                        pos.y = pos.y + selfHeight + DISTANCE_Y;
                    }
                    else
                    {
                        pos.y = pos.y + selfHeight - correctionY;
                    }

                    break;
                }
                case Origin.LeftBottom:
                {
                    if (correctionX == 0)
                    {
                        pos.x = pos.x - selfWidth - DISTANCE_X;
                    }
                    else
                    {
                        pos.x = pos.x - selfWidth - correctionX;
                    }

                    pos.y = pos.y - DISTANCE_Y;
                    break;
                }
            }

            selfRect.localPosition = pos;
        }

        private void PlayAnimation(bool bShow)
        {
            if (_playable == null) return;
            
            // Reset panels
            _panel.localScale = Vector3.one;
            _panel.localEulerAngles = Vector3.zero;

            string animationStr = null;
            if (bShow)
            {
                animationStr = _origin switch
                {
                    Origin.RightBottom => ANIM_IN_RIGHT_BOTTOM,
                    Origin.RightTop => ANIM_IN_RIGHT_TOP,
                    Origin.LeftTop => ANIM_IN_LEFT_TOP,
                    Origin.LeftBottom => ANIM_IN_LEFT_BOTTOM,
                    _ => throw new ArgumentNullException()
                };
            }
            else
            {
                animationStr = _origin switch
                {
                    Origin.RightBottom => ANIM_OUT_RIGHT_BOTTOM,
                    Origin.RightTop => ANIM_OUT_RIGHT_TOP,
                    Origin.LeftTop => ANIM_OUT_LEFT_TOP,
                    Origin.LeftBottom => ANIM_OUT_LEFT_BOTTOM,
                    _ => throw new ArgumentNullException()
                };
            }

            _playable.Play(animationStr, null);
        }
    }
}

// namespace RainbowArt.CleanFlatUI
// {
//     [ExecuteAlways]
//     public class ContextMenu : MonoBehaviour, IPointerDownHandler
//     {
//         protected internal class ContextMenuItem : MonoBehaviour, IPointerEnterHandler
//         {
//             public TextMeshProUGUI itemText;
//             public Image itemImage;
//             public Image itemLine;
//             public Button button;
//             public virtual void OnPointerEnter(PointerEventData eventData)
//             {
//                 EventSystem.current.SetSelectedGameObject(gameObject);
//             }
//         }
//
//         [SerializeField]
//         GameObject itemTemplate;
//
//         [SerializeField]
//         TextMeshProUGUI itemText;
//
//         [SerializeField]
//         Image itemImage;
//
//         [SerializeField]
//         Image itemLine;
//
//         [SerializeField]
//         Animator animator;
//
//         [SerializeField] 
//         RectOffset padding = new RectOffset();
//
//         [SerializeField]
//         float spacing = 2;
//
//         [Serializable]
//         public class OptionItem
//         {
//             public string text;
//             public Sprite icon;
//
//             public OptionItem()
//             {
//             }
//
//             public OptionItem(string newText)
//             {
//                 text = newText;
//             }
//
//             public OptionItem(Sprite newImage)
//             {
//                 icon = newImage;
//             }
//             public OptionItem(string newText, Sprite newImage)
//             {
//                 text = newText;
//                 icon = newImage;
//             }
//         }
//
//         [SerializeField]
//         List<OptionItem> options = new List<OptionItem>();
//
//         [Serializable]
//         public class ContextMenuEvent: UnityEvent<int> { }
//
//         [SerializeField]
//         ContextMenuEvent onValueChanged = new ContextMenuEvent();            
//         
//         enum Origin
//         {
//             RightBottom,
//             LeftBottom, 
//             RightTop,
//             LeftTop, 
//         }   
//         Origin origin = Origin.RightBottom;   
//
//         List<ContextMenuItem> menuItems = new List<ContextMenuItem>();
//         GameObject clickerBlocker;
//         IEnumerator diableCoroutine; 
//         float disableTime = 0.15f; 
//         float distanceX = 2.0f;
//         float distanceY = 2.0f;    
//
//         public RectOffset Padding
//         {
//             get => padding;
//             set
//             {
//                 padding = value;
//             }
//         }
//
//         public float Spacing
//         {
//             get => spacing;
//             set
//             {
//                 spacing = value;
//             }
//         }
//
//         public ContextMenuEvent OnValueChanged
//         {
//             get => onValueChanged;
//             set
//             {
//                 onValueChanged = value;
//             }
//         }  
//
//         public void Show(Vector2 mousePosition, RectTransform areaScope)
//         {
//             if(options.Count > 0)
//             {
//                 gameObject.SetActive(true);
//                 DestroyAllMenuItems();
//                 DestroyClickBlocker();
//                 SetupTemplate();
//                 CreateAllMenuItems(options);
//                 UpdatePosition(mousePosition,areaScope);
//                 CreateClickBlocker();
//                 PlayAnimation(true);
//             }            
//         }
//
//         public void AddOptions(List<OptionItem> optionList)
//         {
//             options.AddRange(optionList);
//         }
//
//         public void AddOptions(List<string> optionList)
//         {
//             for (int i = 0; i < optionList.Count; i++)
//             {
//                 options.Add(new OptionItem(optionList[i]));
//             }                
//         }
//
//         public void AddOptions(List<Sprite> optionList)
//         {
//             for (int i = 0; i < optionList.Count; i++)
//             {
//                 options.Add(new OptionItem(optionList[i]));
//             }                
//         }
//
//         public void ClearOptions()
//         {
//             options.Clear();
//         }
//
//         void SetupTemplate()
//         {
//             ContextMenuItem menuItem = itemTemplate.GetComponent<ContextMenuItem>();
//             if (menuItem == null)
//             {
//                 menuItem = itemTemplate.AddComponent<ContextMenuItem>();
//                 menuItem.itemText = itemText;
//                 menuItem.itemImage = itemImage;   
//                 menuItem.itemLine = itemLine;             
//                 menuItem.button = itemTemplate.GetComponent<Button>();
//             }
//             itemTemplate.SetActive(false);
//         }
//
//         void CreateAllMenuItems(List<OptionItem> options)
//         {
//             float itemWidth = itemTemplate.GetComponent<RectTransform>().rect.width;
//             RectTransform templateParentTransform = itemTemplate.transform.parent as RectTransform;
//             int dataCount = options.Count;
//             float curY = -padding.top;
//             for (int i = 0; i < dataCount; ++i)
//             {
//                 OptionItem itemData = options[i];
//                 int index = i;
//                 GameObject go = Instantiate(itemTemplate) as GameObject;
//                 go.transform.SetParent(itemTemplate.gameObject.transform.parent, false);
//                 go.transform.localPosition = Vector3.zero;
//                 go.transform.localEulerAngles = Vector3.zero;
//                 go.SetActive(true);
//                 go.name = "MenuItem" + i;
//                 ContextMenuItem curMenuItem = go.GetComponent<ContextMenuItem>();
//                 menuItems.Add(curMenuItem);
//                 curMenuItem.itemText.text = itemData.text;
//                 if(itemData.icon == null)
//                 {
//                     curMenuItem.itemImage.gameObject.SetActive(false);
//                     curMenuItem.itemImage.sprite = null;
//                 }
//                 else
//                 {
//                     curMenuItem.itemImage.gameObject.SetActive(true);
//                     curMenuItem.itemImage.sprite = itemData.icon;
//                 }   
//                 if(curMenuItem.itemLine != null)
//                 {
//                     if(i == (dataCount-1))
//                     {
//                         curMenuItem.itemLine.gameObject.SetActive(false);
//                     }
//                     else
//                     {
//                         curMenuItem.itemLine.gameObject.SetActive(true);
//                     }
//                 }
//                 curMenuItem.button.onClick.RemoveAllListeners();
//                 curMenuItem.button.onClick.AddListener(delegate { OnItemClicked(index); });
//                 
//                 RectTransform curRectTransform = go.GetComponent<RectTransform>();
//                 curRectTransform.anchoredPosition3D = new Vector3(padding.left, curY, 0);
//                 float curItemHeight = curRectTransform.rect.height;
//                 curY = curY - curItemHeight;
//                 if (i < (dataCount - 1))
//                 {
//                     curY = curY - spacing;
//                 }                
//             }
//             
//             gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(curY) + padding.bottom);
//             gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemWidth + padding.left+padding.right);
//         }
//
//         Canvas GetRootCanvas()
//         {
//             List<Canvas> list = new List<Canvas>();
//             gameObject.GetComponentsInParent(false, list);
//             if (list.Count == 0)
//             {
//                 return null;
//             }
//             var listCount = list.Count;
//             Canvas rootCanvas = list[listCount - 1];
//             for (int i = 0; i < listCount; i++)
//             {
//                 if (list[i].isRootCanvas || list[i].overrideSorting)
//                 {
//                     rootCanvas = list[i];
//                     break;
//                 }
//             }
//             return rootCanvas;
//         }
//
//         RectTransform GetRootCanvasRectTrans()
//         {
//             Canvas rootCanvas = GetRootCanvas();
//             if (rootCanvas == null)
//             {
//                 return null;
//             }
//             return rootCanvas.GetComponent<RectTransform>();
//         }
//
//         void CreateClickBlocker()
//         {
//             Canvas rootCanvas = GetRootCanvas();
//             if(rootCanvas == null)
//             {
//                 return;
//             }
//             clickerBlocker = new GameObject("ClickBlocker");
//             RectTransform blockerRect = clickerBlocker.AddComponent<RectTransform>();
//             blockerRect.anchorMin = new Vector2(0.5f, 0.5f);
//             blockerRect.anchorMax = new Vector2(0.5f, 0.5f);
//             blockerRect.pivot = new Vector2(0.5f, 0.5f);
//             Image blockerImage = clickerBlocker.AddComponent<Image>();
//             blockerImage.color = Color.clear;
//             RectTransform rootCanvasRect = rootCanvas.GetComponent<RectTransform>();
//             float rootCanvasWidth = rootCanvasRect.rect.width;
//             float rootCanvasHeight = rootCanvasRect.rect.height;
//             blockerRect.SetParent(rootCanvas.transform, false);
//             blockerRect.localPosition = Vector3.zero;
//             blockerRect.SetParent(gameObject.transform, true);
//             blockerRect.SetAsFirstSibling();
//             blockerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rootCanvasHeight);
//             blockerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rootCanvasWidth);
//         }
//
//         public void OnPointerDown(PointerEventData eventData)
//         {
//             Hide(true);
//         }
//
//         void OnItemClicked(int index)
//         {
//             onValueChanged.Invoke(index);
//             Hide(true);
//         }
//
//         void DestroyAllMenuItems()
//         {
//             var itemsCount = menuItems.Count;
//             for (int i = 0; i < itemsCount; i++)
//             {
//                 if (menuItems[i] != null)
//                 {
//                     Destroy(menuItems[i].gameObject);
//                 }
//             }
//             menuItems.Clear();
//         }
//
//         void DestroyClickBlocker()
//         {
//             if(clickerBlocker != null)
//             {
//                 Destroy(clickerBlocker);
//                 clickerBlocker = null;
//             }
//         }
//
//         public void Hide(bool playAnim)
//         {
//             if (diableCoroutine != null)
//             {
//                 StopCoroutine(diableCoroutine);
//                 diableCoroutine = null;
//             }
//             if(playAnim)
//             {
//                 PlayAnimation(false);
//                 diableCoroutine = DisableTransition();
//                 StartCoroutine(diableCoroutine);
//             }
//             else
//             {
//                 if (animator != null)
//                 {
//                     animator.enabled = false;
//                     animator.gameObject.transform.localScale = Vector3.one;
//                     animator.gameObject.transform.localEulerAngles = Vector3.zero;
//                 }
//                 gameObject.SetActive(false);
//                 DestroyAllMenuItems();
//                 DestroyClickBlocker();
//             }                 
//         }   
//
//         IEnumerator DisableTransition()
//         {
//             yield return new WaitForSeconds(disableTime);
//             gameObject.SetActive(false);
//             DestroyAllMenuItems();       
//         }    
//
//         public bool IsShowing()
//         {
//             return gameObject.activeSelf;
//         }
//
//         void UpdatePosition(Vector2 mousePosition, RectTransform areaScope)
//         {
//             if (areaScope == null)
//             {
//                 areaScope = GetRootCanvasRectTrans();
//                 if (areaScope == null)
//                 {
//                     return;
//                 }
//             }
//             RectTransform selfRect = GetComponent<RectTransform>();      
//             selfRect.localPosition = new Vector3(mousePosition.x,mousePosition.y,0);                                
//             Vector3[] corners = new Vector3[4];
//             selfRect.GetWorldCorners(corners);
//             Vector3[] cornersInArea = new Vector3[4];
//             float correctionX = 0;
//             float correctionY = 0;
//             for(int i = 0; i < 4; i++)
//             {
//                 cornersInArea[i] = areaScope.InverseTransformPoint(corners[i]); 
//             } 
//             if(cornersInArea[2].x >= areaScope.rect.xMax)
//             {
//                 if(cornersInArea[0].x - selfRect.rect.width < areaScope.rect.xMin)
//                 {
//                     correctionX = cornersInArea[0].x - selfRect.rect.width - areaScope.rect.xMin;
//                 }
//                 if(cornersInArea[0].y < areaScope.rect.yMin)
//                 {
//                     origin = Origin.LeftTop;
//                     if(cornersInArea[2].y + selfRect.rect.height > areaScope.rect.yMax)
//                     {
//                         correctionY = cornersInArea[2].y + selfRect.rect.height - areaScope.rect.yMax;
//                     }   
//                 }
//                 else
//                 {
//                     origin = Origin.LeftBottom;
//                 }
//             }            
//             else if(cornersInArea[0].y < areaScope.rect.yMin)
//             {
//                 origin = Origin.RightTop;  
//                 if(cornersInArea[2].y + selfRect.rect.height > areaScope.rect.yMax)
//                 {
//                     correctionY = cornersInArea[2].y + selfRect.rect.height - areaScope.rect.yMax;
//                 }                                        
//             }   
//             else
//             {
//                 origin = Origin.RightBottom;     
//             }   
//             
//             Vector3 pos = selfRect.localPosition;
//             float selfWidth = selfRect.rect.width;
//             float selfHeight = selfRect.rect.height;
//             switch (origin)
//             {
//                 case Origin.RightBottom:
//                 {
//                     pos.x = pos.x + distanceX;
//                     pos.y = pos.y - distanceY;
//                     break;
//                 }
//                 case Origin.RightTop:
//                 {
//                     pos.x = pos.x + distanceX;
//                     if(correctionY == 0)
//                     {
//                         pos.y = pos.y + selfHeight + distanceY; 
//                     }
//                     else
//                     {                        
//                         pos.y = pos.y + selfHeight - correctionY;
//                     }
//                     break;
//                 }
//                 case Origin.LeftTop:
//                 {
//                     if(correctionX == 0)
//                     {
//                         pos.x = pos.x - selfWidth - distanceX;
//                     }
//                     else
//                     {
//                         pos.x = pos.x - selfWidth - correctionX;
//                     }  
//                     if(correctionY == 0)
//                     {
//                         pos.y = pos.y + selfHeight + distanceY; 
//                     }
//                     else
//                     {                        
//                         pos.y = pos.y + selfHeight - correctionY;
//                     }                    
//                     break;
//                 }               
//                 case Origin.LeftBottom:
//                 {
//                     if(correctionX == 0)
//                     {
//                         pos.x = pos.x - selfWidth - distanceX;
//                     }
//                     else
//                     {
//                         pos.x = pos.x - selfWidth - correctionX;
//                     }  
//                     pos.y = pos.y - distanceY;                     
//                     break;
//                 } 
//             }  
//             selfRect.localPosition = pos;
//         }
//
//         void PlayAnimation(bool bShow)
//         {
//             if (animator != null)
//             {
//                 animator.enabled = false;
//                 animator.gameObject.transform.localScale = Vector3.one;
//                 animator.gameObject.transform.localEulerAngles = Vector3.zero;
//             }
//             if (animator != null)
//             {
//                 if(animator.enabled == false)
//                 {
//                     animator.enabled = true;
//                 }
//                 string animationStr = null; 
//                 if(bShow)
//                 {      
//                     animationStr =  "In Right Bottom";               
//                     switch (origin)
//                     {
//                         case Origin.RightBottom:
//                         {                            
//                             break;
//                         }
//                         case Origin.RightTop:
//                         {
//                             animationStr = "In Right Top"; 
//                             break;
//                         }
//                         case Origin.LeftTop:
//                         {
//                             animationStr = "In Left Top"; 
//                             break;
//                         }               
//                         case Origin.LeftBottom:
//                         {
//                             animationStr = "In Left Bottom"; 
//                             break;
//                         } 
//                     }                       
//                 }
//                 else
//                 {
//                     animationStr = "Out Right Bottom"; 
//                     switch (origin)
//                     {
//                         case Origin.RightBottom:
//                         {                            
//                             break;
//                         }
//                         case Origin.RightTop:
//                         {
//                             animationStr = "Out Right Top"; 
//                             break;
//                         }
//                         case Origin.LeftTop:
//                         {
//                             animationStr = "Out Left Top"; 
//                             break;
//                         }               
//                         case Origin.LeftBottom:
//                         {
//                             animationStr = "Out Left Bottom"; 
//                             break;
//                         } 
//                     }                    
//                 }
//                 animator.Play(animationStr,0,0);
//             }            
//         }   
//     }
// }