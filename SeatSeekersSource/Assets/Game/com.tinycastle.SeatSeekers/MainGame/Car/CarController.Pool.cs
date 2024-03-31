using System.Linq;
using com.brg.UnityCommon;

namespace com.tinycastle.SeatSeekers
{
    public partial class CarController
    {
        private SeatController GetSeat()
        {
            var seat = _seatPool.First();
            _seatPool.Remove(seat);
            _spawnedSeats.Add(seat);
            return seat;
        }

        private Obstacle GetObstacle()
        {
            var obstacle = _obstaclePool.First();
            _obstaclePool.Remove(obstacle);
            _spawnedObstacles.Add(obstacle);
            return obstacle;
        }

        private void ReturnSeat(SeatController seat)
        {
            _spawnedSeats.Remove(seat);
            seat.SetGOActive(false);
            _seatPool.Add(seat);
        }
        
        private void ReturnObstacle(Obstacle obstacle)
        {
            _spawnedObstacles.Remove(obstacle);
            obstacle.SetGOActive(false);
            _obstaclePool.Add(obstacle);
        }
    }
}