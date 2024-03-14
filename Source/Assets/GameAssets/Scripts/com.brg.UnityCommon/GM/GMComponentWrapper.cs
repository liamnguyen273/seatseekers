using com.brg.Common.Initialization;
using com.brg.Common.ProgressItem;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public abstract class GMComponentWrapper<T> : MonoBehaviour, IGMComponent where T : IInitializable, new()
    {
        protected T _wrappedObject;

        public T WrappedObject => _wrappedObject;

        public InitializationState State => _wrappedObject.State;
        public bool Usable => _wrappedObject.Usable;
        public ReinitializationPolicy ReInitPolicy => _wrappedObject.ReInitPolicy;
        public IProgressItem GetInitializeProgressItem()
        {
            return _wrappedObject.GetInitializeProgressItem();
        }

        public virtual void Initialize()
        {
            _wrappedObject.Initialize();
        }

        public void OnFoundByGM()
        {
            _wrappedObject = new T();
        }

        public static implicit operator T(GMComponentWrapper<T> wrapper)
        {
            return wrapper._wrappedObject;
        }
    }
}