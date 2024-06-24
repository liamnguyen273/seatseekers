using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Purchasing;

namespace com.brg.Unity.Singular
{
    public static class SingularHelper
    {
        public static void SendSingularIAPEvent(Product product, bool restored)
        {
            var attr = new Dictionary<string, object>()
            {
                ["amt"] = product.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture),
                ["r"] = product.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture)
            };

#if UNITY_IOS
            SingularSDK.InAppPurchase("mn_iap", product, attr, restored);
#else
            SingularSDK.InAppPurchase("mn_iap", product, null, restored);
#endif
        }

    }
}