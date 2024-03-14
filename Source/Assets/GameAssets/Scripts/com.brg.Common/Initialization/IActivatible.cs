using com.brg.Common.ProgressItem;

namespace com.brg.Common.Initialization
{
    public interface IActivatable
    {
        public void Activate();
        public void Deactivate();  
        public void PrepareActivate();
        public void PrepareDeactivate();
        public IProgressItem GetPrepareActivateProgressItem();
        public IProgressItem GetPrepareDeactivateProgressItem();
    }
}