using com.brg.Common.Initialization;
using com.brg.UnityCommon;

namespace com.tinycastle.SeatCinema
{
    public partial class MainGameManager
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.ALLOW_ON_FAILED;
        
        public void OnFoundByGM()
        {
            // Do nothing
        }

        protected override void StartInitializationBehaviour()
        {
            _car.Comp.MainGame = this;
            _car.Comp.Initialize();

            _customerPool = new(_customerHost.GameObject.GetDirectOrderedChildComponents<Customer>());
            foreach (var customer in _customerPool)
            {
                customer.SetGOActive(false);
            }

            _spawnedCustomers = new();

            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            
        }
    }
}