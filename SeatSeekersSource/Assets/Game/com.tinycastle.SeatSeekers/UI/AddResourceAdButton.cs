using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class AddResourceAdButton : MonoBehaviour
    {
        [SerializeField] private string _resourceToAdd = "";
        [SerializeField] private int _addCount = 1;
        
        [SerializeField] private CompWrapper<UIButton> _button = "./";

        private void Awake()
        {
            _button.Comp.OnClicked += OnAd;
        }

        private void OnAd()
        {
            // TODO: Ad
            
            var value = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromResources(_resourceToAdd) ?? 0;
            value += _addCount;
            
            GM.Instance.Get<GameSaveManager>().PlayerData.SetInResources(_resourceToAdd, value, true);
        }
    }
}