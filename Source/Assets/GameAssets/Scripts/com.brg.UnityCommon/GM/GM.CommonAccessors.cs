using com.brg.Common.AnalyticsEvents;
using com.brg.UnityCommon.Ads;
using com.brg.UnityCommon.Data;
using com.brg.UnityCommon.Effects;
using com.brg.UnityCommon.Facebook;
using com.brg.UnityCommon.IAP;
using com.brg.UnityCommon.Player;
using com.brg.UnityCommon.RemoteConfig;
using com.brg.UnityCommon.UI;

namespace com.brg.UnityCommon
{
    public partial class GM
    {
        public PopupManager Popups => Get<PopupManager>();
        public AdManager Ad => Get<AdManager>();
        
        public FacebookManager Facebook => Get<FacebookManager>();

        public EffectMaker Effects => Get<EffectMaker>();
        
        public PlayerManager Player => Get<GMComponentPlayerManagerWrapper>().WrappedObject;
        public DataManager Data => Get<GMComponentDataManagerWrapper>().WrappedObject;
        public PurchaseManager Purchases => Get<GMComponentPurchaseManagerWrapper>().WrappedObject;
        public AnalyticsEventManager Events => Get<GMComponentAnalyticsEventManagerWrapper>().WrappedObject;
        public RemoteConfigManager RemoteConfigs => Get<GMComponentRemoteConfigManagerWrapper>().WrappedObject;
    }
}