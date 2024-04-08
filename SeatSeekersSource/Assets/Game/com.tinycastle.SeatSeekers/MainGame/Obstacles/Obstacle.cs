using System;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class Obstacle : MonoBehaviour, IOccupyable
    {
        private SeatData _data;
        
        public CarController Car { get; set; }
        public SeatData Data => _data;
        public int X => _data.X;
        public int Y => _data.Y;
        public int SeatColor => _data.Color;
        public bool IsDoubleSeat => _data.IsDouble;
        
        public Transform Transform => transform;
        
        public bool Occupy(int x, int y)
        {
            return (X == x && Y == y) || (IsDoubleSeat && X + 1 == x && Y == y);
        }

        public bool CanEnterFrom(int fx, int fy)
        {
            return false;
        }

        public void SetData(SeatData data, bool performMoveImmediately = true)
        {
            _data = data;
            RefreshAppearance();
        }

        private void RefreshAppearance()
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Ouch");
        }
    }
}