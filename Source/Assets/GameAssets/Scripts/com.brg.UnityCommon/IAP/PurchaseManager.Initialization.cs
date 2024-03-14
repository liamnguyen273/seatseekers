using com.brg.Common.Initialization;
using com.brg.UnityCommon.Data;
using com.brg.UnityCommon.Player;

namespace com.brg.UnityCommon.IAP
{
    public partial class PurchaseManager : ManagerBase
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.ALLOW_ON_FAILED;

        public void SetComponents(DataManager dataManager, PlayerManager playerManager)
        {
            _playerManager = playerManager;
            _dataManager = dataManager;
        }

        public void RetryIAPInitialization()
        {
            if (Usable && IAPUsable)
            {
                Log.Warn("Unity IAP is already usable, cannot reinitialize");
            }
            
            InitializeIAP();
        }
        
        protected override void StartInitializationBehaviour()
        {
            InitializeUnityServices();
            InitializeIAP();
        }
        
        private void OnInitializeFailed()
        {
            EndInitialize(false);
        }

        protected override void EndInitializationBehaviour()
        {
            ProductEntryAccessibleEvent?.Invoke();
        }
    }
}