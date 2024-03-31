#if HAS_FACEBOOK_DSK
using Facebook.Unity;
using System.Threading.Tasks;
using com.brg.Common;
using UnityEngine;

namespace com.brg.ExtraComponents
{
    public class FacebookManager : ManagerBase
    {
        private bool? _initDoneFlag = null;
        
        protected override async Task<bool> InitializeBehaviourAsync()
        {
            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
                await Utils.WaitUntil(() => _initDoneFlag is not null);
                return _initDoneFlag!.Value;
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
                return true;
            }
        }
        
        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...
                _initDoneFlag = true;
            }
            else
            {
                Log.Warn("Failed to Initialize the Facebook SDK");
                _initDoneFlag = false;
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }
    }
}
#endif