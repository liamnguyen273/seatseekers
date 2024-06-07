using System;
using com.brg.Common;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public class PopupBehaviour : MonoBehaviour, IInitializable
    {
        [SerializeField] private EventWrapper _showStartEvent;
        [SerializeField] private EventWrapper _showEndEvent;
        [SerializeField] private EventWrapper _hideStartEvent;
        [SerializeField] private EventWrapper _hideEndEvent;

        public EventWrapper ShowStartEvent => _showStartEvent;
        public EventWrapper ShowEndEvent => _showEndEvent;
        public EventWrapper HideStartEvent => _hideStartEvent;
        public EventWrapper HideEndEvent => _hideEndEvent;

        private Action _oneTimeShowStart;
        private Action _oneTimeShowEnd;
        private Action _oneTimeHideStart;
        private Action _oneTimeHideEnd;

        public Popup Popup { get; internal set; }

        public virtual IProgress Initialize()
        {
            if (Initialized) return InitializationProgress;
            
            _showStartEvent ??= new EventWrapper();
            _showEndEvent ??= new EventWrapper();
            _hideStartEvent ??= new EventWrapper();
            _hideEndEvent ??= new EventWrapper();
            
            _showStartEvent += InnateOnShowStart;
            _showEndEvent += InnateOnShowEnd;
            _hideStartEvent += InnateOnHideStart;
            _hideEndEvent += InnateOnHideEnd;

            Initialized = true;

            return InitializationProgress;
        }

        public bool Initialized { get; private set; }

        private IProgress _initializationProgress;
        public IProgress InitializationProgress => _initializationProgress ??= new ImmediateProgress(true, 1f);

        public PopupBehaviour OnShowStart(Action action)
        {
            _oneTimeShowStart = action;
            _showStartEvent += _oneTimeShowStart;
            return this;
        }
        
        public PopupBehaviour OnShowEnd(Action action)
        {
            _oneTimeShowEnd = action;
            _showEndEvent += _oneTimeShowEnd;
            return this;
        }
        
        public PopupBehaviour OnHideStart(Action action)
        {
            _oneTimeHideStart = action;
            _hideStartEvent += _oneTimeHideStart;
            return this;
        }
        
        public PopupBehaviour OnHideEnd(Action action)
        {
            _oneTimeHideEnd = action;
            _hideEndEvent += _oneTimeHideEnd;
            return this;
        }

        internal void CleanUpOnShowSessionCompleted()
        {
            _showStartEvent -= _oneTimeShowStart;
            _showEndEvent -= _oneTimeShowEnd;
            _hideStartEvent -= _oneTimeHideStart;
            _hideEndEvent -= _oneTimeHideEnd;
        }

        protected virtual void InnateOnShowStart() { }
        protected virtual void InnateOnShowEnd() { }
        protected virtual void InnateOnHideStart() { }
        protected virtual void InnateOnHideEnd() { }
    }
}