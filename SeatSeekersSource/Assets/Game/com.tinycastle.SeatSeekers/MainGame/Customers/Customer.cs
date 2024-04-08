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
        [SerializeField] private GOWrapper _appearance = "./Appearance";
        [SerializeField] private CompWrapper<Animator> _animator = "./Appearance";
        [SerializeField] private CompWrapper<SkinnedMeshRenderer> _meshRenderer = "./Appearance";

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
                _meshRenderer.Comp.material.color = SeatColorUtils.GetColor(value);
            }
        }

        private void Awake()
        {
            ResetLookTowards();
        }

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public void ResetAnim()
        {
            ResetLookTowards();
            _animator.Comp.Play("idle", -1, 0f);
        }

        private void ResetLookTowards()
        {
            _appearance.Transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        private void RotateTo(Vector3 point)
        {
            var offset = point - transform.position;
            var x = offset.z;
            var z = -offset.x;
            var value = Mathf.Atan2(z, x) * Mathf.Rad2Deg;
            _appearance.Transform.localRotation = Quaternion.Euler(0f, 0f, value);
        }

        private void LateUpdate()
        {
            _animator.Comp.transform.localPosition = Vector3.zero;
        }

        public void TeleportTo(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public Sequence MoveToSeatViaPath(List<Vector3> pathPoints, bool quickJump = false)
        {
            var currPos = transform.position;

            var sequence = DOTween.Sequence();
            var i = 0;
            sequence.AppendCallback(() =>
            {
                _animator.Comp.SetTrigger("walk");
            });
            
            foreach (var nextPoint in pathPoints)
            {
                var isLast = i == pathPoints.Count - 1;
                var dist = Vector3.Distance(currPos, nextPoint);
                var moveTime = ((isLast && !quickJump)? 3f : 1f) * dist / _moveSpeed;

                if (isLast)
                {
                    sequence.AppendCallback(() =>
                    {
                        ResetLookTowards();
                        _animator.Comp.SetTrigger("jump");
                    });
                }
                else
                {
                    sequence.AppendCallback(() => RotateTo(nextPoint));
                }
                
                sequence
                    .Append(transform
                    .DOMove(nextPoint, moveTime)
                    .SetEase(Ease.Linear));
                
                currPos = nextPoint;

                ++i;
            }

            sequence.AppendCallback(ResetLookTowards);

            return sequence;
        }

        public Tween MoveInQueue(Vector3 queuePos)
        {
            var currPos = transform.position;
            var dist = Vector3.Distance(currPos, queuePos);
            var moveTime = dist / _moveSpeed;

            if (moveTime < 0.05f) return null;
            
            return DOTween.Sequence()
                .AppendCallback(() => _animator.Comp.SetTrigger("walk"))
                .Append(transform.DOMove(queuePos, moveTime).SetEase(Ease.Linear))
                .AppendCallback(() => _animator.Comp.Play("idle"));
        }

        private void GetMoveTweenTo(Vector3 pos)
        {
            throw new NotImplementedException();
        }
    }
}