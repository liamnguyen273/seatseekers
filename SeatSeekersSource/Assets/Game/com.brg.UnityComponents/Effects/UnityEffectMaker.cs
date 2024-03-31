using System.Collections.Generic;
using com.brg.UnityCommon;

namespace com.brg.UnityComponents
{
    public partial class EffectMaker : EffectMakerBase
    {
        public EffectMaker(IEnumerable<EffectHelper> effectHelpers) : base(effectHelpers)
        {
            
        }
    }

    public class UnityEffectMaker : UnityComp<EffectMaker>
    {
        private void Awake()
        {

        }

        public void AttachComps()
        {
            var effectMaker = new EffectMaker(GetEffects());
            Comp = effectMaker;
        }

        private IEnumerable<EffectHelper> GetEffects()
        {
            var helpers = gameObject.GetDirectOrderedChildComponents<EffectHelper>();
            return helpers;
        }
    }
}