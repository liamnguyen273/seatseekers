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
        private const string QUEST_PROGRESS_2 = "quest_progress_welcome";

        private readonly Dictionary<string, QuestInfo> _questInfos;
        private readonly GameSaveManager _gameSaveManager;
        
        public QuestManager(GameSaveManager saveManager)
        {
            _gameSaveManager = saveManager;
        }

        protected override Task<bool> InitializeBehaviourAsync()
        {
            
            return base.InitializeBehaviourAsync();
        }

        public static Dictionary<string, (int progress, int total)> GetNewQuestItems()
        {
            return null;
        }
    }
}