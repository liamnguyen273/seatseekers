using System;
using UnityEngine;

namespace com.brg.UnityCommon.Editor
{
    [Serializable]
    public class CompWrapper<T> where T : Component
    {
        [SerializeField] private T _comp;
        [SerializeField] private string _path;
        
        public Transform Transform => Comp.transform;
        public GameObject GameObject => Comp.gameObject;

        public T Comp => _comp;

        public CompWrapper(string path)
        {
            _path = path;
        }

        public CompWrapper()
        {
            
        }

        public void Set(T comp, string path)
        {
            _comp = comp;
            _path = path;
        }
        
        public static implicit operator CompWrapper<T>(string path)
        {
            return new CompWrapper<T>(path);
        }

        public static implicit operator T(CompWrapper<T> wrapper)
        {
            return wrapper.Comp;
        }
    }
}