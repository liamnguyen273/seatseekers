using com.brg.UnityCommon.Data;
using com.brg.UnityCommon.IAP;
using com.brg.UnityCommon.Player;

namespace com.brg.UnityCommon
{
    public class GMComponentPurchaseManagerWrapper : GMComponentWrapper<PurchaseManager>
    {
        private PlayerManager _playerManager;
        private DataManager _dataManager;

        public void SetComponents(DataManager dataManager, PlayerManager playerManager)
        {
            _playerManager = playerManager;
            _dataManager = dataManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            _wrappedObject.SetComponents(_dataManager, _playerManager);
        }
    }
}