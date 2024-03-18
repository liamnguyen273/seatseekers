using com.brg.Common.Initialization;
using com.brg.Common.ProgressItem;
using com.brg.UnityCommon.Data;
using DG.Tweening;

namespace com.tinycastle.SeatCinema
{
    public partial class MainGameManager : IActivatable
    {
        public void Activate()
        {
            StartGame();
        }

        public void Deactivate()
        {
            _playerMoveTween?.Kill();
            _playerMoveTween = null;
        }

        public void PrepareActivate()
        {
            throw new System.NotImplementedException();
        }

        public void PrepareDeactivate()
        {
            throw new System.NotImplementedException();
        }

        public IProgressItem GetPrepareActivateProgressItem()
        {
            // TODO
            LoadLevel(new LevelEntry());
            return new ImmediateProgressItem();
        }

        public IProgressItem GetPrepareDeactivateProgressItem()
        {
            throw new System.NotImplementedException();
        }
    }
}