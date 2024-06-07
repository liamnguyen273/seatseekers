using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class EnergyTimeWatch : TextLocalizer
    {
        public override LocalizableText Text
        {
            set { /* Do not set */ } 
        }
            
        protected override void OnEnable()
        {
            base.OnEnable();
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            
            if (!accessor.HasData) return;
            
            var value = accessor.EnergyRechargeTimer;
            base.Text = value.ToString();
            accessor.EnergyRechargeTimerChangedEvent += OnResourceChange;
        }

        private void OnResourceChange(object sender, int time)
        {
            base.Text = time.ToString();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.EnergyRechargeTimerChangedEvent -= OnResourceChange;
        }
    }
}