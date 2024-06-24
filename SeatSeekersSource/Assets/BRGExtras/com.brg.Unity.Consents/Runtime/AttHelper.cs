using com.brg.Common;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace com.brg.Unity.ATT
{
    public static class ATTHelper
    {
        private static IProgress _requestProgress;
    
#if UNITY_IOS
        public static ATTrackingStatusBinding.AuthorizationTrackingStatus GetStatus()
        {
            return ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        }
#endif
        
        public static IProgress Request()
        {
            if (_requestProgress != null) return _requestProgress;
        
#if UNITY_IOS
            if (!Determined()) 
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
        
            _requestProgress = new SingleProgress(Determined, Determined, () => Determined() ? 1f : 0f, 1f);
            return _requestProgress;
        }

        public static bool Determined()
        {
#if UNITY_IOS
            return ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
#else
            return true;
#endif
        }
    }
}
