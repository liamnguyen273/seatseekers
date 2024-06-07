using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class ResourceWatcherText : TextLocalizer
    {
        [SerializeField] private string _key;
        
        public override LocalizableText Text
        {
            set { /* Do not set */ } 
        }
            
        protected override void OnEnable()
        {
            base.OnEnable();
            
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.ResourcesChangedEvent += OnResourceChange;

            if (!accessor.HasData) return;

            var value = accessor.GetFromResources(_key) ?? 0;
            base.Text = value.ToString();
        }

        private void OnResourceChange(object sender, (string key, bool isRemoved, int newCount) e)
        {
            if (e.key != _key) return;
            base.Text = e.newCount.ToString();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.ResourcesChangedEvent -= OnResourceChange;
        }
    }
}