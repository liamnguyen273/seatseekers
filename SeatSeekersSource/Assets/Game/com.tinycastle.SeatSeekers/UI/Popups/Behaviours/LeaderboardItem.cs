using com.brg.UnityComponents;
using com.tinycastle.SeatSeekers;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.SeatSeekers
{
    public class LeaderboardItem : MonoBehaviour
    {
        [Header("Fields")]
        [SerializeField] private TextLocalizer _rankText;
        [SerializeField] private TextLocalizer _nameText;
        [SerializeField] private TextLocalizer _scoreText;
        [SerializeField] private Image _avatar;
        [SerializeField] private GameObject[] _stars;
        
        public void SetInfo(int rank, string name, int score, bool isPlayer)
        {
            _rankText.Text = rank.ToString();
            _nameText.Text = name;
            _scoreText.Text = score.ToString();

            _avatar.sprite = GM.Instance.Get<GameDataManager>().GetAvatar(name);

            for (int i = 1; i <= 3; ++i)
            {
                _stars[i - 1].SetActive(i == rank);
            }
        }
    }
}