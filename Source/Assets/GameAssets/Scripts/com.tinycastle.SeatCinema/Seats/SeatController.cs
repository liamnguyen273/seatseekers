using System;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using CW.Common;
using Lean.Touch;
using UnityEngine;

namespace com.tinycastle.SeatCinema
{
    public class SeatController : MonoBehaviour, IOccupyable
    {
        [Header("Components")] 
        [SerializeField] private CompWrapper<BoxCollider> _collider = ".";
        [SerializeField] private GOWrapper _positionCaster = "./PositionCaster";
        [SerializeField] private CompWrapper<SpriteRenderer> _appearanceSingle = "./AppearanceSingle";
        [SerializeField] private CompWrapper<SpriteRenderer> _appearanceDouble = "./AppearanceDouble";
        [SerializeField] private GOWrapper _casterRed = "./PositionCaster/CasterRed";
        [SerializeField] private GOWrapper _casterGreen = "./PositionCaster/CasterGreen";
        [SerializeField] private GOWrapper _customerSeat1 = "./Seats/Seat1";
        [SerializeField] private GOWrapper _customerSeat2 = "./Seats/Seat2";

        [Header("Params")] 
        [SerializeField] private float _sensitivity = 1f;
        
        private SeatData _data;
        private Customer? _setCustomer1;
        private Customer? _setCustomer2;
        private bool _pickedUp;
        private LeanFinger? _finger = null;
        
        public CarController Car { get; set; }
        public SeatData Data => _data;
        public int X => _data.X;
        public int Y => _data.Y;
        public int SeatColor => _data.Color;
        public bool IsDoubleSeat => _data.IsDouble;

        public bool FullySeated => _setCustomer1 is not null && (!IsDoubleSeat || _setCustomer2 is not null);

        public bool Occupy(int x, int y)
        {
            return (X == x && Y == y) || (IsDoubleSeat && X + 1 == x && Y == y);
        }

        public bool CanEnterFrom(int fx, int fy)
        {
            var dx = fx - X;
            var dy = fy - Y;
            if (!IsDoubleSeat) return Mathf.Abs(dx) + Mathf.Abs(dy) == 1 && (dx != 0 && dy != 1);
            else
                return (_setCustomer1 is null && ((dx == -1 && dy == 0) || (dx == 0 && dy == -1))) ||
                       (_setCustomer2 is null && ((dx == 1 && dy == 0) || (dx == 2 && dy == 0)));
        }

        public void SetCoord(int x, int y)
        {
            _data.X = x;
            _data.Y = y;
            RefreshAppearance();
        }

        public void SetValue(int value)
        {
            _data.Value = value;
            RefreshAppearance();
        }

        public void SetData(SeatData newData)
        {
            _data = newData;
            RefreshAppearance();
        }

        public bool AssignCustomer1(Customer customer)
        {
            if (_setCustomer1 is not null || SeatColor != customer.Color) return false;
            _setCustomer1 = customer;

            return true;
        }

        public bool AssignCustomer2(Customer customer)
        {
            if (_setCustomer2 is not null || SeatColor != customer.Color) return false;
            _setCustomer2 = customer;

            return true;
        }

        public bool RemoveCustomers()
        {
            var result = false;
            result |= RemoveCustomer1();
            result |= RemoveCustomer2();
            return result;
        }
        
        public bool RemoveCustomer1()
        {
            if (_setCustomer1 is null) return false;

            var oldCustomer = _setCustomer1;
            _setCustomer1 = null;

            return true;
        }        
        
        public bool RemoveCustomer2()
        {
            if (_setCustomer2 is null) return false;

            var oldCustomer = _setCustomer1;
            _setCustomer2 = null;

            return true;
        }

        private Vector3 _movePosition;
        private Vector3 _cumulativeDelta;
        public void OnFingerDown(LeanSelectByFinger select, LeanFinger finger)
        {
            if (Car.SeatLock) return;
            _pickedUp = true;
            _finger = finger;
            _movePosition = transform.position;
            _cumulativeDelta = Vector3.zero;
            _positionCaster.GameObject.SetActive(true);
        }

        public void OnFingerUp(LeanSelectByFinger select, LeanFinger finger)
        {
            _pickedUp = false;
            _finger = null;
            _positionCaster.GameObject.SetActive(false);
            
            _cumulativeDelta = Vector3.zero;
            
            ResolveSeatDrop();
        }

        private void Update()
        {
            if (_pickedUp)
            {
                _casterRed.GameObject.SetActive(false);
                _casterGreen.GameObject.SetActive(true);
                if (CheckTouchInTransformRange(_finger!.ScreenPosition))
                {
                    var screenDelta = _finger!.ScreenDelta;
                    UpdateMovePosition(screenDelta);
                    transform.position = _movePosition;
                    SnapToGrid();
                }

            }
        }

        private bool CheckTouchInTransformRange(Vector2 screenPosition)
        {
            var worldTouchPoint = Camera.main.ScreenToWorldPoint(screenPosition);
            var posW = new Vector2(worldTouchPoint.x, worldTouchPoint.z);
            var pos = new Vector2(_movePosition.x, _movePosition.z);
            var distance = Vector2.Distance(pos, posW);
            return distance <= 1f;
        }

        private void UpdateMovePosition(Vector2 screenDelta)
        {
            var screenPoint = Camera.main.WorldToScreenPoint(_movePosition);

            const float distance = 0.9f;
            var hitLeft = RaycastWithDebugLines(new Vector3(-1f, 0f, 0f), distance);
            if (hitLeft) screenDelta.x = Mathf.Max(0, screenDelta.x);
            
            var hitRight = RaycastWithDebugLines(new Vector3(1f, 0f, 0f), distance);
            if (hitRight) screenDelta.x = Mathf.Min(0, screenDelta.x); 
            
            var hitUp = RaycastWithDebugLines(new Vector3(0f, 0f, 1f), distance);
            if (hitUp) screenDelta.y = Mathf.Min(0, screenDelta.y);            
            
            var hitDown = RaycastWithDebugLines(new Vector3(0f, 0f, -1f), distance);
            if (hitDown) screenDelta.y = Mathf.Max(0, screenDelta.y);
                
            var worldPoint = Camera.main.ScreenToWorldPoint(screenPoint + (Vector3)screenDelta * _sensitivity);
            _movePosition = worldPoint;
        }

        private bool RaycastWithDebugLines(Vector3 direction, float distance)
        {
            var pos = transform.position;
            pos.y += 0.5f;
            var result = Physics.Raycast(pos, direction, distance, LayerMask.GetMask("Seats"), QueryTriggerInteraction.Collide);
            if (result)
            {
                Debug.DrawRay(pos, direction * distance, Color.red);
            }
            else
            {
                Debug.DrawRay(pos, direction * distance, Color.green);
            }
            return result;
        }

        private void ResolveSeatDrop()
        {
            var success = Car.ResolveSeatDrop(this, transform.localPosition, out var newX, out var newY);
            if (success)
            {
                // TODO
            }
            else
            {
                // TODO
            }
            
            SetCoord(newX, newY);
        }

        private void SnapToGrid()
        {
            var success = Car.CheckSeatDrop(this, transform.localPosition, out var snappedPosition, out var newX, out var newY);

            var isSame = newX == X && newY == Y;
            _casterRed.GameObject.SetActive(!isSame && !success);
            _casterGreen.GameObject.SetActive(isSame || success);
            
            transform.localPosition = snappedPosition;
        }

        private void RefreshAppearance()
        {
            // Reset selection
            // TODO
            // OnFingerUp(null);

            var pos = Car.GetCellPosition(X, Y, false);
            
            _appearanceSingle.GameObject.SetActive(!_data.IsDouble);
            _appearanceDouble.GameObject.SetActive(_data.IsDouble);

            _collider.Comp.center = new Vector3(_data.IsDouble ? 0.5f : 0f, 0f, -0.5f);
            _collider.Comp.size = new Vector3(_data.IsDouble ? 1.9f : 0.9f, 0.9f, 1f);

            transform.localPosition = pos;
            
            _appearanceSingle.Comp.color = SeatColorUtils.GetColor(_data.Color);
            _appearanceDouble.Comp.color = SeatColorUtils.GetColor(_data.Color);
            
            _positionCaster.GameObject.SetActive(false);
        }
    }
}