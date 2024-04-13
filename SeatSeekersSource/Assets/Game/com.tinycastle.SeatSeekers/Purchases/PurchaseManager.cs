using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.UnityComponents;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;

namespace com.tinycastle.SeatSeekers
{
    public partial class PurchaseManager : ManagerBase
    {        
        public const string ENVIRONMENT = "production";
        
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private Dictionary<string, ProductMetadata> _iapProductMetadatas;

        private bool _restorePurchaseRequested;

        public event EventHandler ProductMetadataAccessibleEvent;

        public bool IapUsable => _controller != null && _extensions != null;

        public void LaunchIAPInitialization()
        {
            var options = new InitializationOptions().SetEnvironmentName(ENVIRONMENT);
            UnityServices.InitializeAsync(options).ContinueWith(task =>
            {
                InitializeIAP();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        
        protected override Task<bool> InitializeBehaviourAsync()
        {
            _controller = null;
            _extensions = null;
            LaunchIAPInitialization();
            return base.InitializeBehaviourAsync();
        }

        private ProductEntry _currentIapEntry;
        private Action<bool> _onComplete;
        public void Purchase(ProductEntry product, Action<bool> onComplete = null)
        {
            if (!Initialized)
            {
                onComplete?.Invoke(false);
                return;
            };

            if (_currentIapEntry != null)
            {
                onComplete?.Invoke(false);
                return;
            }
            
            if (product.IsIAP)
            {
                if (!IapUsable)
                {
                    onComplete?.Invoke(false);
                    return;
                }
                
                _onComplete = onComplete;
                HandleIAPPurchase(product);
            }

            // Not IAP, can handle immediately
            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;

            var resource = product.Currency;
            var price = product.Price;
            var currentResourceCount = accessor.GetFromResources(resource) ?? 0;

            if (price > currentResourceCount)
            {
                onComplete?.Invoke(false);
            }

            currentResourceCount -= price;
            accessor.SetInResources(resource, currentResourceCount, true);
            FinalizePurchaseSuccess(product);
            
            onComplete?.Invoke(true);
        }

        
        public ProductMetadata GetProductMetaData(string id)
        {
            if (!Initialized)
            {
                Log.Warn($"IAP is not ready, getting metadata for \"{id}\" failed, returning null.");
                return null;
            }

            var entry = GM.Instance.Get<GameDataManager>().GetProductEntry(id);
            if (entry == null)
            {
                Log.Warn($"Product \"{id}\" doesn't exist, cannot get product metadata, returning null.");
                return null;
            }

            if (!entry.IsIAP)
            {
                Log.Warn($"Product \"{id}\" isn't an IAP, returning null.");
                return null;
            }
            
            if (!_iapProductMetadatas.TryGetValue(id, out var metadata))
            {
                Log.Error($"Cannot get metadata for product \"{id}\" returning null.");
                return null;
            }

            return metadata;
        }

        
        private void FinalizePurchaseSuccess(ProductEntry product)
        {
            var resolutions = product.GetParsedBuyResolutions();
            var resolutionParams = product.GetParsedResolutionData();

            for (var i = 0; i < resolutions.Length; ++i)
            {
                var resolution = resolutions[i];
                var parameters = i >= resolutionParams.Length ? "" : resolutionParams[i];
                ResolveBuyResolution(product.Id, resolution, parameters);
            }

            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            accessor.WriteDataAsync();
            _currentIapEntry = null;
            _onComplete = null;
        }
        
        private void FinalizePurchaseSuccess(string id)
        {
            var product = GM.Instance.Get<GameDataManager>().GetProductEntry(id);

            if (product is null) return;
            FinalizePurchaseSuccess(product);
        }

        private static void ResolveBuyResolution(string productId, string resolution, string resolutionParam)
        {
            LogObj.Default.Info($"Resolving resolution \"{resolution}\" with params: \"{resolutionParam}\"");

            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            
            switch (resolution)
            {
                case Constants.BUY_RESOLUTION_SET_OWN:
                {
                    var tokens = resolutionParam.Split("&", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        accessor.SetInOwnerships(token, true, shouldOverride: true);
                        LogObj.Default.Info($"Player now own \"{token}\".");
                    }

                    break;
                }
                case Constants.BUY_RESOLUTION_ADD_RESOURCE:
                {
                    var tokens = resolutionParam.Split("&", StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var token in tokens)
                    {
                        var subTokens = token.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        if (subTokens.Length != 2)
                        {
                            LogObj.Default.Error($"Cannot resolve add resource \"{token}\"");
                            continue;
                        }

                        var success = int.TryParse(subTokens[0], out var count);
                        var resource = subTokens[1];

                        if (!success)
                        {
                            LogObj.Default.Error($"Cannot parse count of resource (\"{count}\")");
                            continue;
                        }
                        
                        LogObj.Default.Info($"Will add {count} of \'{resource}\".");
                        var current = accessor.GetFromResources(resource) ?? 0;
                        current += count;
                        accessor.SetInResources(resource, current, true);
                    }

                    break;
                }
                default:
                    LogObj.Default.Warn($"Unknown buy resolution: {resolution}. Please check.");
                    break;
            }
        }
    }
}