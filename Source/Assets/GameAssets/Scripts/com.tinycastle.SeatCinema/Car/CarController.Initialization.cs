using System.Collections.Generic;
using com.brg.Common.Initialization;
using com.brg.Common.ProgressItem;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using UnityEngine;

namespace com.tinycastle.SeatCinema
{
    public partial class CarController : IInitializable
    {
        public InitializationState State { get; }
        public bool Usable { get; }
        public ReinitializationPolicy ReInitPolicy { get; }
        public IProgressItem GetInitializeProgressItem()
        {
            return new ImmediateProgressItem();
        }

        public void Initialize()
        {
            _seatPool = new(_seatHost.GameObject.GetDirectOrderedChildComponents<SeatController>());
            _obstaclePool = new (_obstacleHost.GameObject.GetDirectOrderedChildComponents<Obstacle>());

            foreach (var obstacle in _obstaclePool)
            {
                obstacle.Car = this;
                obstacle.SetGOActive(false);
            }
            
            foreach (var seat in _seatPool)
            {
                seat.Car = this;
                seat.SetGOActive(false);
            }

            _spawnedSeats = new();
            _spawnedObstacles = new();

            _occupyables = new();
        }
    }
}