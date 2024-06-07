using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Unity.ATT;

namespace com.brg.Unity.Consents
{
    public class ConsentManager : ManagerBase
    {
        protected override async Task<bool> InitializeBehaviourAsync()
        {
            var progressGroup = new ProgressGroup(ProgressLeniency.REQUIRE_ALL_SUCCEEDED, new[]
            {
                GoogleCMP.Initialize(),
                ATTHelper.Request()
            });
            var result = await progressGroup.Task;
            return result;
        }
    }
}