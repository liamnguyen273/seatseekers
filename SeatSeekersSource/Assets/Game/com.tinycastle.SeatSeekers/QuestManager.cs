using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using com.tinycastle.SeatSeekers;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class QuestInfo
    {
        public string Id;
        public string Title;
        public string Description;
        public int Reward;
    }
    
    public class QuestManager : ManagerBase
    {
        private const string QUEST_PROGRESS_1 = "quest_progress_welcome";
        private const string QUEST_PROGRESS_2 = "quest_progress_fast_learner";
        private const string QUEST_PROGRESS_3 = "quest_progress_novice";
        private const string QUEST_PROGRESS_4 = "quest_progress_rookie";
        private const string QUEST_PROGRESS_5 = "quest_progress_adept";
        private const string QUEST_PROGRESS_6 = "quest_progress_expert";
        private const string QUEST_PROGRESS_7 = "quest_progress_master";
        private const string QUEST_WELCOME_BACK = "quest_progress_return";
        private const string QUEST_MULTIPLAYER = "quest_progress_multiplayer";
        private const string QUEST_IAP = "quest_progress_iap";

        private Dictionary<string, QuestInfo> _questInfos;
        private readonly GameSaveManager _gameSaveManager;
        
        public QuestManager(GameSaveManager saveManager)
        {
            _gameSaveManager = saveManager;
        }

        protected override Task<bool> InitializeBehaviourAsync()
        {
            _questInfos = new Dictionary<string, QuestInfo>()
            {
                { QUEST_PROGRESS_1, new QuestInfo() {Id = QUEST_PROGRESS_1, Title = "Newcomer", Description = "Complete 5 levels.", Reward = 10 } },
                { QUEST_PROGRESS_2, new QuestInfo() {Id = QUEST_PROGRESS_2, Title = "Fast Learner", Description = "Complete 10 levels.", Reward = 25 } },
                { QUEST_PROGRESS_3, new QuestInfo() {Id = QUEST_PROGRESS_3, Title = "Novice", Description = "Complete 25 levels.", Reward = 30 } },
                { QUEST_PROGRESS_4, new QuestInfo() {Id = QUEST_PROGRESS_4, Title = "Rookie", Description = "Complete 50 levels.", Reward = 50 } },
                { QUEST_PROGRESS_5, new QuestInfo() {Id = QUEST_PROGRESS_5, Title = "Adept", Description = "Complete 100 levels.", Reward = 100 } },
                { QUEST_PROGRESS_6, new QuestInfo() {Id = QUEST_PROGRESS_6, Title = "Expert", Description = "Complete 150 levels.", Reward = 200 } },
                { QUEST_PROGRESS_7, new QuestInfo() {Id = QUEST_PROGRESS_7, Title = "Master", Description = "Complete 200 levels.", Reward = 300 } },
                { QUEST_WELCOME_BACK, new QuestInfo() {Id = QUEST_WELCOME_BACK, Title = "Welcome back", Description = "Return to the game after a rest.", Reward = 35 } },
                { QUEST_MULTIPLAYER, new QuestInfo() {Id = QUEST_MULTIPLAYER, Title = "Competitor", Description = "Complete a multiplayer game.", Reward = 20 } },
                { QUEST_IAP, new QuestInfo() {Id = QUEST_IAP, Title = "Shopper", Description = "Purchase an item in shop.", Reward = 40 } },
            };
            
            return base.InitializeBehaviourAsync();
        }

        public bool CheckQuestProgress()
        {
            var hasCompletion = false;
            try
            {
                var progressQuestProgress1 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_1) ?? 0;
                var progressQuestProgress2 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_2) ?? 0;
                var progressQuestProgress3 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_3) ?? 0;
                var progressQuestProgress4 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_4) ?? 0;
                var progressQuestProgress5 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_5) ?? 0;
                var progressQuestProgress6 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_6) ?? 0;
                var progressQuestProgress7 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_7) ?? 0;
                var progressQuestWelcomeBack = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_WELCOME_BACK) ?? 0;
                var progressQuestMultiplayer = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_MULTIPLAYER) ?? 0;
                var progressQuestIAP = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_IAP) ?? 0;

                var levelProgress = _gameSaveManager.PlayerData.GetCompletedLevelCount();
                var hasWelcomeBack = _gameSaveManager.PlayerData.HasReturned;
                var hasMultiplayer = _gameSaveManager.PlayerData.HasPlayedMultiplayer;
                var hasIap = _gameSaveManager.PlayerData.HasIAP;
                
                if (progressQuestProgress1 != -1) 
                {
                    progressQuestProgress1 = Mathf.Min(levelProgress, 5);
                    hasCompletion |= progressQuestProgress1 >= 5;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_1, levelProgress, true); 
                }
                if (progressQuestProgress2 != -1) 
                {
                    progressQuestProgress2 = Mathf.Min(levelProgress, 10);
                    hasCompletion |= progressQuestProgress2 >= 10;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_2, levelProgress, true); 
                }
                if (progressQuestProgress3 != -1) 
                {
                    progressQuestProgress3 = Mathf.Min(levelProgress, 20);
                    hasCompletion |= progressQuestProgress3 >= 20;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_3, levelProgress, true); 
                }
                if (progressQuestProgress4 != -1) 
                {
                    progressQuestProgress4 = Mathf.Min(levelProgress, 50);
                    hasCompletion |= progressQuestProgress4 >= 50;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_4, levelProgress, true); 
                }
                if (progressQuestProgress5 != -1) 
                {
                    progressQuestProgress5 = Mathf.Min(levelProgress, 100);
                    hasCompletion |= progressQuestProgress5 >= 100;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_5, levelProgress, true); 
                }
                if (progressQuestProgress6 != -1) 
                {
                    progressQuestProgress6 = Mathf.Min(levelProgress, 150);
                    hasCompletion |= progressQuestProgress6 >= 150;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_6, levelProgress, true); 
                }
                if (progressQuestProgress7 != -1) 
                {
                    progressQuestProgress7 = Mathf.Min(levelProgress, 200);
                    hasCompletion |= progressQuestProgress7 >= 200;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_PROGRESS_7, levelProgress, true); 
                }

                if (progressQuestWelcomeBack != -1)
                {
                    hasCompletion |= hasWelcomeBack;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_WELCOME_BACK, hasWelcomeBack ? 1 : 0, true);
                }

                if (progressQuestMultiplayer != -1)
                {
                    hasCompletion |= hasMultiplayer;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_MULTIPLAYER, hasMultiplayer ? 1 : 0, true);
                }

                if (progressQuestIAP != -1)
                {
                    hasCompletion |= hasIap;
                    _gameSaveManager.PlayerData.SetInQuestProgresses(QUEST_IAP, hasIap ? 1 : 0, true);
                } 
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }

            return hasCompletion;
        }

        public bool CollectRewards(string questId)
        {
            if (!_questInfos.ContainsKey(questId)) return false;
            var info = _questInfos[questId];
            var progress = _gameSaveManager.PlayerData.GetFromQuestProgresses(questId) ?? 0;

            if (progress == -1) return false;
            
            var coin = info.Reward;
            var current = _gameSaveManager.PlayerData.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
            current += coin;
            _gameSaveManager.PlayerData.SetInResources(Constants.COIN_RESOURCE, current, true);
            _gameSaveManager.PlayerData.SetInQuestProgresses(questId, -1, true);

            return true;
        }

        public IEnumerable<(QuestInfo info, int progress, int total)> GetQuestForUIs()
        {
            var progressQuestProgress1 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_1);
            var progressQuestProgress2 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_2);
            var progressQuestProgress3 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_3);
            var progressQuestProgress4 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_4);
            var progressQuestProgress5 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_5);
            var progressQuestProgress6 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_6);
            var progressQuestProgress7 = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_PROGRESS_7);
            var progressQuestWelcomeBack = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_WELCOME_BACK);
            var progressQuestMultiplayer = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_MULTIPLAYER);
            var progressQuestIAP = _gameSaveManager.PlayerData.GetFromQuestProgresses(QUEST_IAP);

            yield return (_questInfos[QUEST_PROGRESS_1], progressQuestProgress1 ?? 0, 5);
            yield return (_questInfos[QUEST_PROGRESS_2], progressQuestProgress2 ?? 0, 10);
            yield return (_questInfos[QUEST_PROGRESS_3], progressQuestProgress3 ?? 0, 20);
            yield return (_questInfos[QUEST_PROGRESS_4], progressQuestProgress4 ?? 0, 50);
            yield return (_questInfos[QUEST_PROGRESS_5], progressQuestProgress5 ?? 0, 100);
            yield return (_questInfos[QUEST_PROGRESS_6], progressQuestProgress6 ?? 0, 150);
            yield return (_questInfos[QUEST_PROGRESS_7], progressQuestProgress7 ?? 0, 200);
            yield return (_questInfos[QUEST_WELCOME_BACK], progressQuestWelcomeBack ?? 0, 1);
            yield return (_questInfos[QUEST_MULTIPLAYER], progressQuestMultiplayer ?? 0, 1);
            yield return (_questInfos[QUEST_IAP], progressQuestIAP ?? 0, 1);
        }

        public static Dictionary<string, int> GetNewQuestItems()
        {
            return new Dictionary<string, int>()
            {
                { QUEST_PROGRESS_1, 0 },
                { QUEST_PROGRESS_2, 0 },
                { QUEST_PROGRESS_3, 0 },
                { QUEST_PROGRESS_4, 0 },
                { QUEST_PROGRESS_5, 0 },
                { QUEST_PROGRESS_6, 0 },
                { QUEST_PROGRESS_7, 0 },
                { QUEST_WELCOME_BACK, 0 },
                { QUEST_MULTIPLAYER, 0 },
                { QUEST_IAP, 0 },
            };
        }

        public static int GetSpriteOrderFor(string id)
        {
            return id switch
            {
                QUEST_PROGRESS_1 => 0,
                QUEST_PROGRESS_2 => 1,
                QUEST_PROGRESS_3 => 2,
                QUEST_PROGRESS_4 => 3,
                QUEST_PROGRESS_5 => 4,
                QUEST_PROGRESS_6 => 5,
                QUEST_PROGRESS_7 => 6,
                QUEST_WELCOME_BACK => 7,
                QUEST_MULTIPLAYER => 8,
                QUEST_IAP => 9,
                _ => -1,
            };
        }
    }
}