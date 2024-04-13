using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using com.tinycastle.SeatSeeker;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace com.tinycastle.SeatSeekers
{
    public class MultiplayerAddon : UnityComp
    {
        private struct AdvanceParams
        {
            public float InitAdvanceChance;
            public float AdvanceChanceIncrease;
            public int AdvanceMax;
        }
        
        [SerializeField] private GOWrapper _enemyHud;
        [SerializeField] private CompWrapper<TextLocalizer> _enemyPercentage;
        [SerializeField] private CompWrapper<TextLocalizer> _enemyNameText;
        [SerializeField] private CompWrapper<Image> _enemyAvatar;
        [SerializeField] private GOWrapper _playerHud;
        [SerializeField] private GOWrapper _playerHud2;
        [SerializeField] private CompWrapper<Image> _playerAvatar;
        [SerializeField] private CompWrapper<TextLocalizer> _playerPercentage;

        private bool _gameEnded = false;

        private int _intensity;
        private AdvanceParams _advanceParams;
        private float _updateChance = 0f;
        private int _enemyTotal;
        private int _enemySeated;     
        private int _playerTotal;
        private int _playerSeated;
        private string _enemyName;
        
        private float _enemyUpdateTimer = 0f;

        private Tween _enemyUpdateTween;
        private Tween _playerUpdateTween;

        public string EnemyName => _enemyName;
        public int Intensity => _intensity;

        public override IProgress Initialize()
        {
            _enemyHud.NullableComp?.gameObject?.SetActive(false);
            _playerHud.NullableComp?.gameObject?.SetActive(false);
            _playerHud2.NullableComp?.gameObject?.SetActive(false);
            return base.Initialize();
        }

        public override IProgress Activate()
        {
            _enemyHud.NullableComp?.gameObject?.SetActive(true);
            _playerHud.NullableComp?.gameObject?.SetActive(true);
            _playerHud2.NullableComp?.gameObject?.SetActive(true);
            return base.Activate();
        }

        public override IProgress Deactivate()
        {
            _playerUpdateTween?.Kill();
            _playerUpdateTween = null;
            _enemyUpdateTween?.Kill();
            _enemyUpdateTween = null;

            _gameEnded = false;
            
            _enemyHud.NullableComp?.gameObject?.SetActive(false);
            _playerHud.NullableComp?.gameObject?.SetActive(false);
            _playerHud2.NullableComp?.gameObject?.SetActive(false);
            return base.Deactivate();
        }

        public void StartGame(int intensity, string enemyName, List<Customer>[] customers, Action onCompletePopup)
        {
            _intensity = intensity;
            
            switch (intensity)
            {
                case 1:
                    _advanceParams = new AdvanceParams
                    {
                        InitAdvanceChance = 0.25f,
                        AdvanceChanceIncrease = 0.05f,
                        AdvanceMax = 4,
                    };
                    break;
                case 2:
                    _advanceParams = new AdvanceParams
                    {
                        InitAdvanceChance = 0.3f,
                        AdvanceChanceIncrease = 0.07f,
                        AdvanceMax = 5,
                    };
                    break;
                case 3:
                    _advanceParams = new AdvanceParams
                    {
                        InitAdvanceChance = 0.35f,
                        AdvanceChanceIncrease = 0.09f,
                        AdvanceMax = 6,
                    };
                    break;
                case 4:
                    _advanceParams = new AdvanceParams
                    {
                        InitAdvanceChance = 0.4f,
                        AdvanceChanceIncrease = 0.11f,
                        AdvanceMax = 6,
                    };
                    break;
            }

            _playerTotal = _enemyTotal = customers.SelectMany(list => list, (list, customer) => customer)
                .Count();

            _enemyUpdateTimer = 7f;
            _updateChance = _advanceParams.InitAdvanceChance;

            _enemyName = enemyName;
            var avatar = GM.Instance.Get<GameDataManager>().GetAvatar(_enemyName);
            var playerAvatar = GM.Instance.Get<GameDataManager>().GetAvatar("You");
            _enemyAvatar.NullableComp.sprite = avatar;
            _playerAvatar.NullableComp.sprite = playerAvatar;
            _enemyNameText.NullableComp.Text = _enemyName;
            
            UpdatePlayerHud(0, false);
            UpdateEnemyHud(0, false);

            _gameEnded = false;

            var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupMultiplayerLook>(out var behaviour);
            behaviour.OnHideEnd(onCompletePopup);
            behaviour.Setup(enemyName);
            popup.Show();
        }

        public void OnEnemyFinish()
        {
            _gameEnded = true;
            GM.Instance.Get<MainGameManager>().MultiplayerSetEnemyEndGame();
        }

        public void OnPlayerMove(List<Customer>[] customers)
        {
            var newCount = customers
                .SelectMany(list => list, (list, customer) => customer)
                .Count(x => x.Seated);
            UpdatePlayerHud(newCount, true);
        }

        private void UpdatePlayerHud(int playerSeated, bool anim)
        {
            var old = _playerSeated;
            _playerSeated = playerSeated;
            _playerPercentage.NullableComp.Text = $"{Math.Min(_playerSeated, _playerTotal)}/{_playerTotal}";

            if (old < _playerSeated && anim)
            {
                _playerUpdateTween?.Kill();
                _playerUpdateTween =
                    DOTween.Sequence()
                        .Append(_playerHud2.NullableComp.transform.DOScale(Vector3.one * 1.2f, 0.65f).SetEase(Ease.InOutCubic))
                        .Append(_playerHud2.NullableComp.transform.DOScale(Vector3.one, 0.65f).SetEase(Ease.InOutCubic))
                        .OnComplete(() =>
                        {
                            _playerUpdateTween = null;
                        })
                        .Play();
            }
        }

        private void UpdateEnemyHud(int enemySeatedSinceLast, bool anim)
        {
            var old = _enemySeated;
            _enemySeated += enemySeatedSinceLast;
            
            _enemyPercentage.NullableComp.Text = $"{Math.Min(_enemySeated,_enemyTotal)}/{_enemyTotal}";
            if (old < _enemySeated && anim)
            {
                _enemyUpdateTween?.Kill();
                _enemyUpdateTween =
                    DOTween.Sequence()
                        .Append(_enemyHud.NullableComp.transform.DOScale(Vector3.one * 1.2f, 0.65f).SetEase(Ease.InOutCubic))
                        .Append(_enemyHud.NullableComp.transform.DOScale(Vector3.one, 0.65f).SetEase(Ease.InOutCubic))
                        .OnComplete(() =>
                        {
                            _enemyUpdateTween = null;
                        })
                        .Play();
            }

            if (_enemySeated >= _enemyTotal)
            {
                OnEnemyFinish();
            }
        }
        
        private void Update()
        {
            if (!Activated || Paused || _playerSeated >= _playerTotal || _enemySeated >= _enemyTotal || _gameEnded) return;

            _enemyUpdateTimer -= Time.deltaTime;

            if (_enemyUpdateTimer <= 0f)
            {
                TryUpdateEnemy();
                _enemyUpdateTimer = Random.Range(3f, 6f);
            }
        }

        private void TryUpdateEnemy()
        {
            var willAdvance = Random.Range(0f, 1f) <= _updateChance;

            if (willAdvance)
            {
                var advanceCount = Random.Range(1, _advanceParams.AdvanceMax + 1);
                
                UpdateEnemyHud(advanceCount, true);
                
                _updateChance = _advanceParams.InitAdvanceChance;
            }
            else
            {
                _updateChance += _advanceParams.AdvanceChanceIncrease;
            }
        }
    }
}