using System;
using System.Collections.Generic;
using com.brg.UnityCommon.Editor;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class Customer : MonoBehaviour
    {
        [Header("Components")]
        // [SerializeField] private GOWrapper _appearance = "./Appearance";
        [SerializeField] private CompWrapper<SpriteRenderer> _debugAppearance = "./Appearance";

        [Header("Params")] 
        [SerializeField] private float _moveSpeed = 1.25f;

        private int _index = -1;
        private int _color;

        
        public SeatController? Seat { get; set; }

        public bool Seated => Seat is not null;

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

        public Sequence MoveToSeatViaPath(List<Vector3> pathPoints)
        {
            var currPos = transform.position;

            var sequence = DOTween.Sequence();
            foreach (var nextPoint in pathPoints)
            {
                var dist = Vector3.Distance(currPos, nextPoint);
                var moveTime = dist / _moveSpeed;
                sequence.Append(transform.DOMove(nextPoint, moveTime).SetEase(Ease.Linear));
                currPos = nextPoint;
            }

            return sequence;
        }

        public Tween MoveInQueue(Vector3 queuePos)
        {
            var currPos = transform.position;
            var dist = Vector3.Distance(currPos, queuePos);
            var moveTime = dist / _moveSpeed;
            return transform.DOMove(queuePos, moveTime).SetEase(Ease.Linear);
        }

        private void GetMoveTweenTo(Vector3 pos)
        {
            throw new NotImplementedException();
        }
    }
}