using com.brg.Common;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public class ParticleCallbackHelper : MonoBehaviour
    {
        [SerializeField] private EventWrapper _event;

        public EventWrapper Event => _event;
        
        private void OnParticleSystemStopped()
        {
            _event?.Invoke();
        }
    }
}

