using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using com.tinycastle.SeatSeekers;

namespace Game.com.tinycastle.SeatSeekers
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
    }
}