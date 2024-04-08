using System;
using com.brg.Common;
using com.brg.UnityComponents;

namespace com.tinycastle.SeatSeekers
{
    public class PurchaseManager
    {
        public static bool Purchase(ProductEntry product)
        {
            if (product.IsIAP)
            {
                // TODO
                return false;
            }

            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;

            var resource = product.Currency;
            var price = product.Price;
            var currentResourceCount = accessor.GetFromResources(resource) ?? 0;

            if (price > currentResourceCount)
            {
                return false;
            }

            currentResourceCount -= price;
            accessor.SetInResources(resource, currentResourceCount, true);
            
            var resolutions = product.GetParsedBuyResolutions();
            var resolutionParams = product.GetParsedResolutionData();

            for (var i = 0; i < resolutions.Length; ++i)
            {
                var resolution = resolutions[i];
                var parameters = i >= resolutionParams.Length ? "" : resolutionParams[i];
                ResolveBuyResolution(product.Id, resolution, parameters);
            }

            accessor.WriteDataAsync();
            
            return true;
        }

        public static void ResolveBuyResolution(string productId, string resolution, string resolutionParam)
        {
            LogObj.Default.Info($"Resolving resolution \"{resolution}\" with params: \"{resolutionParam}\"");

            var accessor = GM.Instance.Get<GameSaveManager>().PlayerData;
            
            switch (resolution)
            {
                case GlobalConstants.BUY_RESOLUTION_SET_OWN:
                {
                    var tokens = resolutionParam.Split("&", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        accessor.SetInOwnerships(token, true, shouldOverride: true);
                        LogObj.Default.Info($"Player now own \"{token}\".");
                    }

                    break;
                }
                case GlobalConstants.BUY_RESOLUTION_ADD_RESOURCE:
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