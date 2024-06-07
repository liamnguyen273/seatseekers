using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using com.tinycastle.SeatSeekers;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupQuest : PopupBehaviour
    {
        [SerializeField] private GOWrapper _host;
        [SerializeField] private Sprite[] _sprites;
        
        private List<QuestItem> _items;

        public override IProgress Initialize()
        {
            _items = _host.Transform.GetDirectOrderedChildComponents<QuestItem>().ToList();
            return base.Initialize();
        }

        protected override void InnateOnShowStart()
        {
            var quests = GM.Instance.Get<QuestManager>().GetQuestForUIs()
                .OrderBy(tuple =>
                {
                    if (tuple.progress == -1) return 999f;
                    return (float)tuple.progress / tuple.total;
                });

            var i = 0;
            foreach (var quest in quests)
            {
                var si = QuestManager.GetSpriteOrderFor(quest.info.Id);
                var sprite = si >= 0 ? _sprites[si] : null;
                var item = _items[i];
                
                item.SetInfo(quest, sprite);
                item.Button.OnClicked.ClearEvents();
                item.Button.OnClicked += () =>
                {
                    CollectRewardFor(item, quest.info, quest.info.Id);
                };
                
                ++i;
            }
            
            base.InnateOnShowStart();
        }

        private void CollectRewardFor(QuestItem item, QuestInfo info, string id)
        {
            var collectRewards = GM.Instance.Get<QuestManager>().CollectRewards(id);

            if (collectRewards)
            {
                item.SetToGot();
            }
        }
    }
}