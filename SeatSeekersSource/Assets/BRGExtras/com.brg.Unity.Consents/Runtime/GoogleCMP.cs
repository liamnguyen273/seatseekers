using System;
using System.Threading.Tasks;
using com.brg.Common;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace com.brg.Unity.Consents
{
    public static class GoogleCMP
    {
        private static IProgress _initProgress;
        private static readonly TaskCompletionSource<bool> _tcs = new();

        public static bool Initialized => _tcs.Task.IsCompletedSuccessfully && _tcs.Task.Result;

        public static event Action<ConsentStatus> PotentialConsentInformationChangeEvent;

        public static ConsentStatus GetConsentStatus()
        {
            return ConsentInformation.ConsentStatus;
        }

        public static bool CanRequestAds()
        {
            return ConsentInformation.CanRequestAds();
        }
        
        
        public static IProgress Initialize()
        {
            if (_initProgress != null) return _initProgress;
            _initProgress = new SingleTCSBoolProgress(_tcs, 1f);
            
            var request = new ConsentRequestParameters();
            
            // TODO: Test devices and stuff
            
            UpdateConsent(request);

            return _initProgress;
        }

        public static void UpdateConsent(ConsentRequestParameters request)
        {
            LogObj.Default.Info("GoogleCMP", "Now gathering GDPR consent...");
            
            // Check the current consent information status.
            ConsentInformation.Update(request, OnConsentInfoUpdated);
        }
        
        private static void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                LogObj.Default.Error("GoogleCMP", $"Consent Error code: {consentError.ErrorCode}, {consentError.Message}.");
                
                // Return true either way
                _tcs.SetResult(true);
                PotentialConsentInformationChangeEvent?.Invoke(ConsentInformation.ConsentStatus);
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    LogObj.Default.Error("GoogleCMP", $"Form Error code: {formError.ErrorCode}, {formError.Message}.");
                }
                else
                {
                    // Consent has been gathered.
                    LogObj.Default.Success("GoogleCMP", "GDPR consent gathered without errors.");
                }

                var status = ConsentInformation.ConsentStatus;
                LogObj.Default.Info("GoogleCMP", $"Consent status for GDPR is: {nameof(status)}");

                // var hasConsent = status == ConsentStatus.Obtained || status == ConsentStatus.NotRequired;

                _tcs.SetResult(true);
                
                PotentialConsentInformationChangeEvent?.Invoke(ConsentInformation.ConsentStatus);
            });
        }
                
        /// <summary>
        /// Shows the privacy options form to the user.
        /// </summary>
        public static void ShowPrivacyOptionsForm()
        {
            LogObj.Default.Info("GoogleCMP", "Showing privacy options form.");

            ConsentForm.ShowPrivacyOptionsForm((FormError formError) =>
            {
                if (formError != null)
                {
                    LogObj.Default.Error("GoogleCMP", "Error showing privacy options form with error: " + formError.Message);
                }
                
                PotentialConsentInformationChangeEvent?.Invoke(ConsentInformation.ConsentStatus);
            });
        }

        public static void ResetConsentInformation()
        {
            ConsentInformation.Reset();
            PotentialConsentInformationChangeEvent?.Invoke(ConsentInformation.ConsentStatus);
        }
    }
}