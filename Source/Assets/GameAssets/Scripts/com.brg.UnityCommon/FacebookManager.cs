#if FACEBOOK_SDK
using Facebook.Unity;
#endif

using com.brg.Common.Initialization;
using UnityEngine;

namespace com.brg.UnityCommon.Facebook
{
    public class FacebookManager : MonoManagerBase, IGMComponent
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.ALLOW_ON_FAILED;

        public void OnFoundByGM()
        {
            // Do nothing
        }
        
        protected override void StartInitializationBehaviour()
        {
#if FACEBOOK_SDK
            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
#else
            EndInitialize(true);
#endif
        }

        protected override void EndInitializationBehaviour()
        {
            // Do nothing
        }

        private void InitCallback()
        {
#if FACEBOOK_SDK
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...

                EndInitialize(true);
            }
            else
            {
                Log.Warn("Failed to Initialize the Facebook SDK");
                EndInitialize(false);
            }
#else
            EndInitialize(true);
#endif
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
