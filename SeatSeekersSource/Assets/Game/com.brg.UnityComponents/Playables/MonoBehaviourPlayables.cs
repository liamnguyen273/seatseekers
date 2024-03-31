using System;
using com.brg.UnityCommon;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public abstract class CompPlayable : MonoBehaviour, IPlayable
    {
        public abstract void Play(Action completeCallback);
        public abstract void Kill();
    }    
    
    public abstract class CompInOutPlayable : MonoBehaviour, IInOutPlayable
    {
        public abstract void PlayIn(Action completeCallback);
        public abstract void PlayOut(Action completeCallback);
        public abstract void Kill();
    }

    public abstract class CompAnyPlayable : MonoBehaviour, IAnyPlayable
    {
        public abstract void Play(string animationName, Action completeCallback);
        public abstract void Kill();
    }
}