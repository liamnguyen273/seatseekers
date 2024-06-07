using System;
using com.brg.UnityCommon;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public abstract class CompPlayable : MonoBehaviour, IPlayable
    {
        public abstract void Play(Action completeCallback);
        public abstract void Complete();
        public abstract void Kill();
        public abstract bool Playing { get; }
    }

    public abstract class CompInOutPlayable : MonoBehaviour, IInOutPlayable
    {
        public abstract void PlayIn(Action completeCallback);
        public abstract void PlayOut(Action completeCallback);
        public abstract void CompleteIn();
        public abstract void CompleteOut();
        public abstract void Kill();
        public abstract bool Playing { get; }
        public abstract bool PlayingIn { get; }
        public abstract bool PlayingOut { get; }
    }

    public abstract class CompAnyPlayable : MonoBehaviour, IAnyPlayable
    {
        public abstract void Play(string stateName, Action completeCallback);
        public abstract void CompleteCurrent();
        public abstract void Complete(string name);
        public abstract void Kill();
        public abstract bool Playing { get; }
    }
}