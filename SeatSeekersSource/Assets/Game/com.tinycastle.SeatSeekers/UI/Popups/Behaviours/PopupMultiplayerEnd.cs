using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.SeatSeekers
{
    public class PopupMultiplayerEnd : PopupBehaviour
    {
        [SerializeField] private CompWrapper<Image> _winnerAvatar;
        [SerializeField] private CompWrapper<TextLocalizer> _winnerNameText;
        [SerializeField] private CompWrapper<TextLocalizer> _winnerRewardText;
        
        [SerializeField] private CompWrapper<Image> _loserAvatar;
        [SerializeField] private CompWrapper<TextLocalizer> _loserNameText;
        [SerializeField] private CompWrapper<TextLocalizer> _loserRewardText;

        [SerializeField] private CompWrapper<TextLocalizer> _title;

        public void Setup(bool isPlayerWin, string enemyName, int intensity)
        {
            _title.Comp.Text = isPlayerWin ? "You won!" : "Too bad...";
            
            var winReward = GetReward(intensity, true);
            var loseReward = GetReward(intensity, false);
            
            var data = GM.Instance.Get<GameDataManager>();
            var playerAvatar = data.GetAvatar("You");
            var enemyAvatar = data.GetAvatar(enemyName);
                
            _winnerAvatar.Comp.sprite = isPlayerWin ? playerAvatar : enemyAvatar;
            _winnerNameText.Comp.Text = isPlayerWin ? "You" : enemyName;
            _winnerNameText.Comp.Text = winReward.ToString();                
            _loserAvatar.Comp.sprite = !isPlayerWin ? playerAvatar : enemyAvatar;
            _loserNameText.Comp.Text = !isPlayerWin ? "You" : enemyName;
            _loserRewardText.Comp.Text = loseReward.ToString();

            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            var coin = accessor.GetFromResources(Constants.COIN_RESOURCE) ?? 0;
            coin += isPlayerWin ? winReward : loseReward;
            if (!accessor.HasPlayedMultiplayer) accessor.HasPlayedMultiplayer = true; 
            accessor.SetInResources(Constants.COIN_RESOURCE, coin, true);
            
            var enemyScore = accessor.GetFromLeaderboard(enemyName) ?? 0;
            enemyScore += isPlayerWin ? loseReward : winReward;
            accessor.FixPlayerLeaderboard();
            accessor.SetInLeaderboard(enemyName, enemyScore, true);
            accessor.WriteDataAsync();
        }

        private static int GetReward(int intensity, bool isWin)
        {
            return intensity switch
            {
                1 => isWin ? 160 : 20,
                2 => isWin ? 200 : 25,
                3 => isWin ? 320 : 40,
                4 => isWin ? 400 : 50,
                _ => 0
            };
        }
    }
}