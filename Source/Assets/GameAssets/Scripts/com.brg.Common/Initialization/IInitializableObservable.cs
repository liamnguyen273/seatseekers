using System;

namespace com.brg.Common.Initialization
{
    public delegate InitializationState InitializationDelegate();
    
    /// <summary>
    /// Interface for an initializable that exposes events for initializations
    /// </summary>
    public interface IInitializableObservable
    {
        public event Action OnInitializationSuccessfulEvent;
        public event Action OnInitializationFailedEvent;
    }
}
