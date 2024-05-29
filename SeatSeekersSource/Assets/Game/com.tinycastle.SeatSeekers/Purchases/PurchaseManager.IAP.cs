using System;
using System.Collections.Generic;
using com.brg.Common;
using com.brg.UnityComponents;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace com.tinycastle.SeatSeekers
{
    public partial class PurchaseManager : IDetailedStoreListener
    {
        private void InitializeIAP()
        {
            try
            {
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                var products = GM.Instance.Get<GameDataManager>().GetAllProducts();
                foreach (var (key, entry) in products)
                {
                    if (entry.IsIAP)
                    {
                        Log.Info($"Product: {entry.Id} (Consumable: {entry.IsConsumable})");
                        builder.AddProduct(entry.Id, entry.IsConsumable ? ProductType.Consumable : ProductType.NonConsumable);
                    }
                }
            
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            
            _iapProductMetadatas = new Dictionary<string, ProductMetadata>();
            foreach (var (id, entry) in GM.Instance.Get<GameDataManager>().GetAllProducts())
            {
                if (!entry.IsIAP) continue;
                
                var prod = _controller.products.WithID(id);

                if (prod == null)
                {
                    Log.Error($"Missing on Unity IAP's product catalog, or wrong client's product entry:" +
                               $"\"{id}\" is marked as IAP, but does not exist in Unity IAP's catalog.");
                    continue;
                }
                
                var metadata = prod.metadata;
                _iapProductMetadatas.Add(id, metadata);
            }
            
            Log.Success("IAP initialized.");

            ProductMetadataAccessibleEvent?.Invoke(this, EventArgs.Empty);
        }

        public void RestorePurchases()
        {
            Log.Info("Restore purchase requested.");
#if UNITY_ANDROID || UNITY_IOS
            _restorePurchaseRequested = true;
            UnityGM.Instance.WaitHelper.StartWait();
#endif
            
#if UNITY_ANDROID
            _extensions.GetExtension<IGooglePlayStoreExtensions>()
#elif UNITY_IOS
            _extensions.GetExtension<IAppleExtensions>()
#endif
                
#if UNITY_ANDROID || UNITY_IOS
            .RestoreTransactions((result, error) =>
            {
                UnityGM.Instance.WaitHelper.EndWait();
                
                if (result)
                {
                    var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupBehaviourGeneric>("popup_generic", out var behaviour);
                    behaviour.SetupAsNotify(
                        "Success!", 
                        "Purchases restored successfully.",
                        null,
                        "OK");
                    popup.Show();
                }
                else
                {
                    Log.Error($"Restore purchase failed. Reason: {error}");
                    var popup = GM.Instance.Get<PopupManager>().GetPopup<PopupBehaviourGeneric>("popup_generic", out var behaviour);
                    behaviour.SetupAsNotify(
                        "Oh no!", 
                        "Something went wrong, please retry later.",
                        null,
                        "OK");
                    popup.Show();
                }
            });
#else
            Log.Error("Restoring purchases are not supported.");
#endif
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Log.Error("Unity Unity IAP failed to initialize, reason: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Log.Error("Unity IAP failed to initialize, reason: " + error + ".\n Message: " + message);
        }

        private void HandleIAPPurchase(ProductEntry entry)
        {
#if UNITY_EDITOR
            FinalizePurchaseSuccess(entry);
            return;
#endif
            Log.Info($"Unity IAP's handling the purchase request.");
            var id = entry.Id;
            _controller.InitiatePurchase(id);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Log.Error($"Unity IAP purchase failed for id \"{product.definition.id}\"");
            Log.Error("Failure Description: " + failureDescription);

            if (_currentIapEntry == null)
            {
                if (!_restorePurchaseRequested)
                {
                    Log.Warn(
                        $"Unity IAP process purchase is called for id (\"${product.definition.id}\")" +
                        $"but it is not invoked by RequestPurchase, this is likely the automatic restore purchase behaviour," +
                        $" since no restore purchase is requested, this will be ignored.");
                    return ;
                }
            }
            else if (product.definition.id != _currentIapEntry?.Id)
            {
                Log.Warn($"Unity IAP product id (\"${product.definition.id}\") on purchase failed " +
                         $"is different than the cached id (\"${_currentIapEntry?.Id}\")." +
                         $"\nWill resolve as FAILED regardless.");
            }

            _currentIapEntry = null;
            _onComplete?.Invoke(false);
            _onComplete = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Log.Error($"Unity IAP purchase failed for id \"{product.definition.id}\"");
            Log.Error("Failure Reason: " + failureReason);

            if (_currentIapEntry == null)
            {
                if (!_restorePurchaseRequested)
                {
                    Log.Warn(
                        $"Unity IAP process purchase is called for id (\"${product.definition.id}\")" +
                        $"but it is not invoked by RequestPurchase, this is likely the automatic restore purchase behaviour," +
                        $" since no restore purchase is requested, this will be ignored.");
                    return;
                }
            }
            else if (product.definition.id != _currentIapEntry?.Id)
            {
                Log.Warn($"Unity IAP product id (\"${product.definition.id}\") on purchase failed " +
                          $"is different than the cached id (\"${_currentIapEntry?.Id}\")." +
                          $"\nWill resolve as FAILED regardless.");
            }
            
            _currentIapEntry = null;
            _onComplete?.Invoke(false);
            _onComplete = null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Log.Info($"Purchase success for id \"{purchaseEvent.purchasedProduct.definition.id}\"");

            if (_currentIapEntry?.Id == null)
            {
                if (!_restorePurchaseRequested)
                {
                    Log.Warn(
                        $"Unity IAP process purchase is called for id (\"${purchaseEvent.purchasedProduct.definition.id}\")" +
                        $"but it is not invoked by RequestPurchase, this is likely the automatic restore purchase behaviour," +
                        $" since no restore purchase is requested, this will be ignored.");
                    return PurchaseProcessingResult.Complete;
                }
            }
            else if (purchaseEvent.purchasedProduct.definition.id != _currentIapEntry?.Id)
            {
                Log.Warn($"Unity IAP purchased product id (\"${purchaseEvent.purchasedProduct.definition.id}\")" +
                          $"is different than the cached id (\"${_currentIapEntry?.Id}\")" +
                          $"\nWill resolve purchase with \"${purchaseEvent.purchasedProduct.definition.id}\" and" +
                          $"perform the callback as SUCCESS.");
            }

            FinalizePurchaseSuccess(purchaseEvent.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Complete;
        }
    }
}
