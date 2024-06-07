using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class EnergyWatch : MonoBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _timeText;
        [SerializeField] private CompWrapper<TextLocalizer> _energyText;
        [SerializeField] private GOWrapper _infTimeGroup;
        [SerializeField] private CompWrapper<TextLocalizer> _infTimeText;

        private static PlayerDataAccessor Accessor => GM.Instance.Get<GameSaveManager>().PlayerData;
        
        protected void OnEnable()
        {            
            Accessor.EnergyRechargeTimerChangedEvent += OnRechargeTimerChange;
            Accessor.ResourcesChangedEvent += OnResourceChange;
            
            if (!Accessor.HasData) return;
            
            var value = Accessor.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;
            var infTime = Accessor.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0;
            RefreshAppearance(value, infTime, Accessor.EnergyRechargeTimer);
        }

        private void OnResourceChange(object sender, (string key, bool isRemoved, int item) e)
        {
            switch (e.key)
            {
                case Constants.ENERGY_RESOURCE:
                {
                    var infTime = Accessor.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0; 
                    RefreshAppearance(e.item, infTime, Accessor.EnergyRechargeTimer);
                    break;
                }
                case Constants.INFINITE_ENERGY_RESOURCE:
                {
                    var energy = Accessor.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0; 
                    RefreshAppearance(energy, e.item, Accessor.EnergyRechargeTimer);
                    break;
                }
            }
        }

        private void OnRechargeTimerChange(object sender, int time)
        {
            var infTime = Accessor.GetFromResources(Constants.INFINITE_ENERGY_RESOURCE) ?? 0;
            var energy = Accessor.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;
            RefreshAppearance(energy, infTime, Accessor.EnergyRechargeTimer);
        }

        private void RefreshAppearance(int energyCount, int infTime, int rechargeTime)
        {
            if (infTime > 0)
            {
                if (_energyText.NullableComp != null) _energyText.Comp.Text = "\u221e";
                if (_infTimeGroup.NullableComp != null) _infTimeGroup.SetActive(true);
                if (_infTimeText.NullableComp != null) _infTimeText.Comp.Text = Utils.FormatTime(infTime);
                if (_timeText.NullableComp != null) _timeText.Comp.Text = "Max";
            }
            else if (energyCount >= Constants.MAX_ENERGY)
            {
                if (_energyText.NullableComp != null) _energyText.Comp.Text = $"{energyCount}";
                if (_infTimeGroup.NullableComp != null) _infTimeGroup.SetActive(false);
                if (_timeText.NullableComp != null) _timeText.Comp.Text = "Max";
            }
            else
            {
                if (_energyText.NullableComp != null) _energyText.Comp.Text = $"{energyCount}";
                if (_infTimeGroup.NullableComp != null) _infTimeGroup.SetActive(false);
                if (_timeText.NullableComp != null) _timeText.Comp.Text = Utils.FormatTime(rechargeTime);
            }
        }

        protected void OnDisable()
        {
            Accessor.EnergyRechargeTimerChangedEvent -= OnRechargeTimerChange;
            Accessor.ResourcesChangedEvent -= OnResourceChange;
        }
    }
}