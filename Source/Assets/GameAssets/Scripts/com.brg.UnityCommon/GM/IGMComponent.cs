using com.brg.Common.Initialization;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public interface IGMComponent : IInitializable
    {
        public void OnFoundByGM();
    }
}