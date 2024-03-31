using System.Collections.Generic;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;

namespace com.brg.UnityComponents
{
    public class GM : GMBase
    {
        public static GM Instance { get; set; }

        public GM(IEnumerable<IGameComponent> managers) : base(managers)
        {
            
        }
    }
}