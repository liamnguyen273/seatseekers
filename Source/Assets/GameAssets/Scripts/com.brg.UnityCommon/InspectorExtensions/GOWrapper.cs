using System;
using UnityEngine;

namespace com.brg.UnityCommon.Editor
{
    [Serializable]
    public class GOWrapper
    {
        [SerializeField] private GameObject _comp;
        [SerializeField] private string _path;
        
        public Transform Transform => Comp.transform;
        public GameObject GameObject => Comp.gameObject;

        public GameObject Comp => _comp;

        public GOWrapper(string path)
        {
            _path = path;
        }

        public GOWrapper()
        {
            
        }

        public void Set(GameObject comp, string path)
        {
            _comp = comp;
            _path = path;
        }
        
        public static implicit operator GOWrapper(string path)
        {
            return new GOWrapper(path);
        }
        
        public static implicit operator GameObject(GOWrapper wrapper)
        {
            return wrapper.Comp;
        }
    }
}