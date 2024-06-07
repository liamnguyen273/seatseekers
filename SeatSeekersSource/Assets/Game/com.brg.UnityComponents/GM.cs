using System.Collections.Generic;
using com.brg.Common;

namespace com.brg.Unity
{
    public class GM : GMBase
    {
        public static GM Instance { get; set; }

        public GM(IEnumerable<IGameComponent> managers) : base(managers)
        {
        }
    }
}