using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.SeatSeekers
{
    public class QuestItem : MonoBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _title;
        [SerializeField] private CompWrapper<TextLocalizer> _description;
        [SerializeField] private CompWrapper<TextLocalizer> _rewardText;
        [SerializeField] private CompWrapper<Slider> _progress;
        [SerializeField] private CompWrapper<TextLocalizer> _progressText;
        [SerializeField] private GOWrapper _gotGroup;
        [SerializeField] private GOWrapper _progressGroup;
        [SerializeField] private CompWrapper<UIButton> _getButton;
        [SerializeField] private CompWrapper<Image> _icon;

        public UIButton Button => _getButton.Comp;
        
        public void SetInfo((QuestInfo info, int progress, int total) tuple, Sprite sprite)
        {
            _title.Comp.Text = tuple.info.Title;
            _description.Comp.Text = tuple.info.Description;
            _rewardText.Comp.Text = $"+{tuple.info.Reward}";

            var got = tuple.progress == -1;
            var progress = tuple.progress == -1 ? tuple.total : tuple.progress;
            
            var progressPercentage = (float)progress / tuple.total;
            _progress.Comp.value = progressPercentage;
            _progressText.Comp.Text = $"{progress}/{tuple.total}";
            
            _gotGroup.SetActive(got);
            _progressGroup.SetActive(!got);

            _getButton.Comp.Interactable = !got && progress == tuple.total;
            
            _icon.Comp.sprite = sprite;
        }

        public void SetToGot()
        {
            _gotGroup.SetActive(true);
            _progressGroup.SetActive(false);
        }
    }
}