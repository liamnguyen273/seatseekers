using UnityEngine;

namespace com.brg.Unity.AdMob
{
    [CreateAssetMenu(menuName = "BRG/Extras/AdMob/Config", fileName = "AdMobConfig", order = 1)]
    public class AdMobConfig : ScriptableObject
    {
        // [SerializeField] private string _androidSDKKey;
        // [SerializeField] private string _iosSDKKey;
        
        [SerializeField] private AdMobAdUnitPack _android;
        [SerializeField] private AdMobAdUnitPack _ios;

        // public string AndroidSDKKey => _androidSDKKey;
        //
        // public string IOSSDKKey => _iosSDKKey;

        public AdMobAdUnitPack Android => _android;

        public AdMobAdUnitPack IOS => _ios;
    }
}