using System;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupDailyRewards : PopupBehaviour
    {
        public static bool Check()
        {
            var hasDailyRewards = CheckHasDailyRewards(out int progress);
            hasDailyRewards &= progress <= 7;
            
            return hasDailyRewards;
        }

        [SerializeField] private CompWrapper<UIButton> _buttonRewards;
        [SerializeField] private GOWrapper _rewardHost;

        private List<(GameObject highLightGroup, GameObject gotGroup)> _rewards;

        public override IProgress Initialize()
        {
            var list = new List<(GameObject highLightGroup, GameObject gotGroup)>();
            for (int i = 0; i < _rewardHost.Transform.childCount; ++i)
            {
                var child = _rewardHost.Transform.GetChild(i);
                var highlight = child.Find("HighlightGroup")?.gameObject ?? null;
                var gotGroup = child.Find("GotGroup")?.gameObject ?? null;

                if (highlight == null || gotGroup == null)
                {
                    throw new NullReferenceException("Cannot find groups for daily rewards item.");
                }
                
                list.Add((highlight, gotGroup));
            }

            _rewards = list;
            _buttonRewards.Comp.OnClicked += OnDailyRewardsButton;
            
            return base.Initialize();
        }

        private int _currentRewardProgress = 0;
        private bool _hasDailyRewards;

        protected override void InnateOnShowStart()
        {
            var hasDailyRewards = CheckHasDailyRewards(out int progress);
            hasDailyRewards &= progress <= 7;

            _currentRewardProgress = progress;
            _hasDailyRewards = hasDailyRewards;
            
            for (int number = 1; number <= Mathf.Min(7, _rewards.Count); ++number)
            {
                var i = number - 1;
                var got = number <= progress;
                var highlight = number == (progress + 1) && hasDailyRewards;

                var (highlightGroup, gotGroup) = _rewards[i];
                highlightGroup.SetActive(highlight);
                gotGroup.SetActive(got);
            }

            _buttonRewards.Comp.Interactable = hasDailyRewards;
            
            base.InnateOnShowStart();
        }

        protected override void InnateOnHideEnd()
        {
            _currentRewardProgress = 0;
            _hasDailyRewards = false;
            base.InnateOnHideEnd();
        }

        private void OnDailyRewardsButton()
        {
            if (_hasDailyRewards)
            {
                var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
                var got = true;
                
                LogObj.Default.Info("Daily Rewards", $"Getting rewards {_currentRewardProgress}.");

                var reward = _currentRewardProgress + 1;
                switch (reward)
                {
                    case 1:
                    {
                        var curr = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
                        curr += 20;
                        accessor.SetInResources(Constants.COIN_RESOURCE, curr, true);
                        break;
                    }
                    case 2:
                    {
                        var curr = accessor.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0;
                        curr += 15 * 60;
                        accessor.SetInResources(Constants.INFINITE_ENERGY_RESOURCE, curr, true);
                        break;
                    }
                    case 3:
                    {
                        var curr = accessor.GetFromResources(Constants.BOOSTER_EXPAND_RESOURCE) ?? 0;
                        curr += 1;
                        accessor.SetInResources(Constants.BOOSTER_EXPAND_RESOURCE, curr, true);
                        break;
                    }
                    case 4:
                    {
                        var curr = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
                        curr += 25;
                        accessor.SetInResources(Constants.COIN_RESOURCE, curr, true);
                        break;
                    }
                    case 5:
                    {
                        var curr = accessor.GetFromResources(Constants.BOOSTER_JUMP_RESOURCE) ?? 0;
                        curr += 1;
                        accessor.SetInResources(Constants.BOOSTER_JUMP_RESOURCE, curr, true);
                        break;
                    }
                    case 6:
                    {
                        var curr = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
                        curr += 30;
                        accessor.SetInResources(Constants.COIN_RESOURCE, curr, true);
                        break;
                    }             
                    case 7:
                    {
                        var curr1 = accessor.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0;
                        curr1 += 60 * 60;
                        accessor.SetInResources(Constants.INFINITE_ENERGY_RESOURCE, curr1, true);       
                        
                        var curr2 = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
                        curr2 += 100;
                        accessor.SetInResources(Constants.COIN_RESOURCE, curr2, true);
                        
                        var curr3 = accessor.GetFromResources(Constants.BOOSTER_EXPAND_RESOURCE) ?? 0;
                        curr3 += 1;
                        accessor.SetInResources(Constants.BOOSTER_EXPAND_RESOURCE, curr3, true);
                        break;
                    }
                    default:
                        got = false;
                        break;
                }

                if (got)
                {
                    _hasDailyRewards = false;
                    accessor.DailyRewardProgress += 1;
                    accessor.DailyRewardTime = GetNowTime();
                    GM.Instance.Get<GameSaveManager>().SaveAll();
                }
            }

            Popup.Hide();
        }

        private static bool CheckHasDailyRewards(out int progress)
        {
            progress = -1;
            var saveManager = GM.Instance?.Get<GameSaveManager>();

            if (saveManager == null) return false;
            var lastDate = saveManager.PlayerData.DailyRewardTime.Date;
            var nowDate = GetNowTime().Date;

            progress = saveManager.PlayerData.DailyRewardProgress;

            LogObj.Default.Info("Daily Rewards", $"Saved date: {lastDate}, now: {nowDate}.");
            
            return nowDate > lastDate;
        }

        private static DateTime GetNowTime()
        {
            return DateTime.UtcNow;
        }
    }
}