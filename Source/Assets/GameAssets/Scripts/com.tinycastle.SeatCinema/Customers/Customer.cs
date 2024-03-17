using System;
using System.Collections.Generic;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.SeatCinema
{
    public class Customer : MonoBehaviour
    {
        [Header("Components")]
        // [SerializeField] private GOWrapper _appearance = "./Appearance";
        [SerializeField] private CompWrapper<SpriteRenderer> _debugAppearance = "./Appearance";

        [Header("Params")] 
        [SerializeField] private float _moveSpeed = 1f;

        private int _index = -1;
        private int _color;

        public int Color
        {
            get => _color;
            set
            {
                _color = value % SeatData.DOUBLE_SEAT_LEFT;
                _debugAppearance.Comp.color = SeatColorUtils.GetColor(_color);
            }
        }

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public void TeleportTo(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public void MoveToSeatViaPath(List<Vector3> pathPoints)
        {
            throw new NotImplementedException();
        }

        public void MoveInQueue(Vector3 queuePos)
        {
            throw new NotImplementedException();
        }

        private void GetMoveTweenTo(Vector3 pos)
        {
            throw new NotImplementedException();
        }
    }
}