using System.Linq;
using com.brg.Common.Initialization;
using com.brg.UnityCommon;

namespace com.tinycastle.SeatCinema
{
    public partial class MainGameManager
    {
        private Customer GetCustomer()
        {
            var customer = _customerPool.First();
            _customerPool.Remove(customer);
            _spawnedCustomers.Add(customer);
            return customer;
        }

        private void ReturnCustomer(Customer customer)
        {
            _spawnedCustomers.Remove(customer);
            customer.transform.parent = _customerHost.Transform;
            customer.SetGOActive(false);
            _customerPool.Add(customer);
        }
    }
}