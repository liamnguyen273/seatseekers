using com.brg.Common.Initialization;

namespace com.brg.Common.AnalyticsEvents
{
    public interface IAnalyticsServiceAdapter: IInitializable, IInitializableObservable
    {
        public void SendEvent(AnalyticsEventBuilder eventBuilder);
        public bool TranslateGameEventName(string name, out string translatedName);
    }
}