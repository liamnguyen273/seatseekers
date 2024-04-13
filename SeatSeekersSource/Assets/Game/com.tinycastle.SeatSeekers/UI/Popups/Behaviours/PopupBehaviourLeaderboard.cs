using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityComponents;
using com.tinycastle.SeatSeekers;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class PopupBehaviourLeaderboard : PopupBehaviour
    {
        [SerializeField] private Transform _itemHost;
        [SerializeField] private LeaderboardItem _playerLeaderboardItem;
        
        private List<string> _sortedNames = null;
        private int _youRank = -1;
        private List<LeaderboardItem> _items = null;

        public override IProgress Initialize()
        {
            _items = _itemHost.GetDirectOrderedChildComponents<LeaderboardItem>().ToList();
            return base.Initialize();
        }

        protected override void InnateOnShowStart()
        {
            RefreshLeaderboard();
            RefreshAppearance();
            base.InnateOnShowStart();
        }

        public void RefreshLeaderboard()
        {
            GM.Instance.Get<GameSaveManager>().PlayerData.FixPlayerLeaderboard();
            
            var leaderboard = GM.Instance.Get<GameSaveManager>().PlayerData.GetLeaderboards()
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key);

            _sortedNames = leaderboard.ToList();
            _youRank = _sortedNames.FindIndex(x => x == "You") + 1;
        }

        public int GetYouRank()
        {
            if (_youRank <= 0)
            {
                LogObj.Default.Warn("Leaderboard popup should be refreshed before calling GetYouRank(). Will now" +
                                    "return wrong value.");
            }
            return _youRank;
        }

        private void RefreshAppearance()
        {
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            var leaderboard = accessor.GetLeaderboards();
            for (var i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];
                if (_sortedNames.Count <= i)
                {
                    item.SetGOActive(false);
                    continue;
                }
                
                item.SetGOActive(true);
                
                var itemName = _sortedNames[i];
                leaderboard.TryGetValue(itemName, out var score);
                item.SetInfo(i + 1, itemName, score, itemName == "You");
            }
            
            var rank = GetYouRank();
            accessor.GetLeaderboards().TryGetValue("You", out var youScore);
            _playerLeaderboardItem.SetInfo(rank, "You", youScore, true);
        }
    }
}