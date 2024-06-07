using System.Threading.Tasks;
using com.brg.Common;

namespace com.brg.Unity.GameAnalytics
{
    public class GameAnalyticsManager : ManagerBase
    {
        private TaskCompletionSource<bool> _tcs;
        
        protected override Task<bool> InitializeBehaviourAsync()
        {
            _tcs = new TaskCompletionSource<bool>();
            GameAnalyticsSDK.GameAnalytics.onInitialize += OnInitialize;
            GameAnalyticsSDK.GameAnalytics.Initialize();
            return _tcs.Task;
        }

        private void OnInitialize(object sender, bool result)
        {
            _tcs.SetResult(result);
        }
    }
}