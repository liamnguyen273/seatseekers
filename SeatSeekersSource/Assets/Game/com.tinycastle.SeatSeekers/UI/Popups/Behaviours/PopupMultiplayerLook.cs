using System;
using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using com.tinycastle.SeatSeekers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.SeatSeeker
{
    public class PopupMultiplayerLook : PopupBehaviour
    {
        [SerializeField] private CompWrapper<Image> _enemyAvatar;
        [SerializeField] private CompWrapper<TextLocalizer> _enemyName;      
        [SerializeField] private CompWrapper<Image> _playerAvatar;
        [SerializeField] private CompWrapper<TextLocalizer> _timeText;

        private bool _ticking = false;
        private float _time = 0f;
        
        public void Setup(string enemyName)
        {
            var avatar = GM.Instance.Get<GameDataManager>().GetAvatar(enemyName);
            var playerAvatar = GM.Instance.Get<GameDataManager>().GetAvatar("You");
            _enemyAvatar.Comp.sprite = avatar;
            _playerAvatar.Comp.sprite = playerAvatar;
            _enemyName.Comp.Text = enemyName;

        }

        private void Update()
        {
            if (!_ticking) return;
            
            _time -= Time.deltaTime;
            var tickEnd = _time <= 0f;
            _time = Math.Max(0f, _time);
            _timeText.Comp.Text = $"Game starts in {Mathf.CeilToInt(_time)}";

            if (tickEnd)
            {
                _ticking = false;
                Popup.Hide();
            }
        }

        protected override void InnateOnShowStart()
        {
            base.InnateOnShowStart();
        }

        protected override void InnateOnShowEnd()
        {
            _ticking = true;

            _time = 3f;
            _timeText.Comp.Text = $"Game starts in {Mathf.CeilToInt(_time)}";
            base.InnateOnShowEnd();
        }
    }
}