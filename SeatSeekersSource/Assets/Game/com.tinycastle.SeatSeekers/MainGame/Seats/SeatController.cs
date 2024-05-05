using System;
using com.brg.Common;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using JSAM;
using Lean.Touch;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
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
        [SerializeField] private CompWrapper<SpriteRenderer> _jumpModeSelectGudie;
        [SerializeField] private float _sensitivity = 1f;

        [SerializeField] private Sprite[] _seatSingleSprites;
        [SerializeField] private Sprite[] _seatDoubleSprites;
        
        private SeatData _data;
        private Customer _setCustomer1;
        private Customer _setCustomer2;
        private bool _pickedUp;
        private LeanFinger _finger = null;
        
        private bool _seatInJumpMode = false;
        private bool _droppedQueued;
        
        public bool Locked { get; set; }
        
        public CarController Car { get; set; }
        public SeatData Data => _data;
        public int X => _data.X;
        public int Y => _data.Y;
        public int SeatColor => _data.Color;
        public bool IsDoubleSeat => _data.IsDouble;

        public bool FullySeated => _setCustomer1 is not null && (!IsDoubleSeat || _setCustomer2 is not null);

        public int SortValue => X + Y + 1000 * SeatColor;
        
        public bool SeatInJumpMode
        {
            get => !FullySeated && _seatInJumpMode;
            set
            {
                if (FullySeated)
                {
                    _seatInJumpMode = false;
                }
                else
                {
                    _seatInJumpMode = value;
                }
                
                _jumpModeSelectGudie.GameObject.SetActive(_seatInJumpMode);
            }
        }

        public Transform Transform => transform;

        public bool Occupy(int x, int y)
        {
            return (X == x && Y == y) || (IsDoubleSeat && X + 1 == x && Y == y);
        }

        public bool CanEnterFrom(int fx, int fy)
        {
            var dx = fx - X;
            var dy = fy - Y;
            if (FullySeated) return false;
            if (!IsDoubleSeat) return (dx == -1 && dy == 0) || (dx == 1 && dy == 0) || (dx == 0 && dy == -1);
            else
                return (_setCustomer1 is null && ((dx == -1 && dy == 0) || (dx == 0 && dy == -1))) ||
                       (_setCustomer2 is null && ((dx == 2 && dy == 0) || (dx == 1 && dy == -1)));
        }

        public void SetCoord(int x, int y, bool performMoveImmediately = true)
        {
            _data.X = x;
            _data.Y = y;
            RefreshAppearance(performMoveImmediately);
        }

        public void SetValue(int value, bool performMoveImmediately = true)
        {
            _data.Value = value;
            RefreshAppearance(performMoveImmediately);
        }

        public void SetData(SeatData newData, bool performMoveImmediately = true)
        {
            _data = newData;
            _droppedQueued = false;
            Locked = false;
            RefreshAppearance(performMoveImmediately);
        }

        public bool CanAssign1(Customer customer)
        {
            return _setCustomer1 is null && (SeatColor == (int)SeatEnum.ANY || SeatColor == customer.Color);
        }

        public bool CanAssign2(Customer customer)
        {
            return IsDoubleSeat && _setCustomer2 is null && (SeatColor == (int)SeatEnum.ANY || SeatColor == customer.Color);
        }

        public void AssignCustomer1(Customer customer, bool jumpImmediately = true)
        {
            if (_setCustomer1 is not null)
            {
                LogObj.Default.Error("AssignCustomer1 is called, but seat 1 already has a customer");
                return;
            }
            
            _setCustomer1 = customer;
            customer.Seat = this;

            if (jumpImmediately)
            {
                customer.transform.SetParent(_customerSeat1.Transform);
                customer.transform.localPosition = Vector3.zero;
            }
        }

        public void AssignCustomer2(Customer customer, bool jumpImmediately = true)
        {
            if (_setCustomer2 is not null)
            {
                LogObj.Default.Error("AssignCustomer2 is called, but seat 2 already has a customer");
                return;
            }
            
            _setCustomer2 = customer;
            customer.Seat = this;
            if (jumpImmediately)
            {
                customer.transform.SetParent(_customerSeat2.Transform);
                customer.transform.localPosition = Vector3.zero;
            }
        }

        public bool RemoveCustomers()
        {
            var result = false;
            result |= RemoveCustomer1();
            result |= RemoveCustomer2();
            return result;
        }

        public void ResetPickup()
        {
            _pickedUp = false;
        }
        
        public bool RemoveCustomer1()
        {
            if (_setCustomer1 is null) return false;

            var oldCustomer = _setCustomer1;
            _setCustomer1 = null;
            oldCustomer.Seat = null;

            return true;
        }        
        
        public bool RemoveCustomer2()
        {
            if (_setCustomer2 is null) return false;

            var oldCustomer = _setCustomer2;
            _setCustomer2 = null;
            oldCustomer.Seat = null;

            return true;
        }

        private Vector3 _currentMousePosition;
        private Vector3 _currentMoveVelocity;
        public void OnFingerDown(LeanSelectByFinger select, LeanFinger finger)
        {
            AudioManager.PlaySound(AudioLibrarySounds.sfx_shift, transform);
            
            if (!Car.AllowPickUpSeat || Locked || _pickedUp) return;

            _droppedQueued = false;
            
            if (SeatInJumpMode && !FullySeated)
            {
                GM.Instance.Get<MainGameManager>().OnSeatWantCustomerToJumpTo(this);
                return;
            }
            
            _pickedUp = true;
            _finger = finger;
            _currentMoveVelocity = Vector3.zero;
            _currentMousePosition = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
            _positionCaster.GameObject.SetActive(true);
        }
        
        public void OnFingerUp(LeanSelectByFinger select, LeanFinger finger)
        {
            AudioManager.PlaySound(AudioLibrarySounds.sfx_shift, transform);
            
            if (!_pickedUp) return;
            _pickedUp = false;
            _finger = null;

            _droppedQueued = true;
        }

        private void Update()
        {
            if (_pickedUp)
            {
                _casterRed.GameObject.SetActive(false);
                _casterGreen.GameObject.SetActive(true);
                
                // var screenDelta = _finger!.ScreenDelta;
                _currentMousePosition = Camera.main.ScreenToWorldPoint(_finger!.ScreenPosition);
                var currPos = transform.position;
                var calculatedPos = Vector3.MoveTowards(currPos, _currentMousePosition, 4f);
                calculatedPos.y = 0f;
                var offset = calculatedPos - currPos;
                PositionOffset(ref offset);
                transform.position = currPos + offset;
                SnapToGrid();
            }

            if (_droppedQueued)
            {
                if (!GM.Instance.Get<MainGameManager>().ResolvingPlayerAction)
                {
                    ResolveSeatDrop();
                }
            }
        }

        // private bool CheckTouchInTransformRange(Vector2 screenPosition)
        // {
        //     var worldTouchPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        //     var posW = new Vector2(worldTouchPoint.x, worldTouchPoint.z);
        //     var pos = new Vector2(_movePosition.x, _movePosition.z);
        //     var distance = Vector2.Distance(pos, posW);
        //     return distance <= 1f;
        // }

        private void PositionOffset(ref Vector3 offset)
        {
            offset.y = 0f;
            var distance = 0.4f;
            var hitLeft = RaycastWithDebugLines(new Vector3(-1f, 0f, 0f), distance);
            if (hitLeft) offset.x = Mathf.Max(0, offset.x);
            
            var hitRight = RaycastWithDebugLines(new Vector3(1f, 0f, 0f), distance);
            if (hitRight) offset.x = Mathf.Min(0, offset.x); 
            
            var hitUp = RaycastWithDebugLines(new Vector3(0f, 0f, 1f), distance);
            if (hitUp) offset.z = Mathf.Min(0, offset.z);            
            
            var hitDown = RaycastWithDebugLines(new Vector3(0f, 0f, -1f), distance);
            if (hitDown) offset.z = Mathf.Max(0, offset.z);
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
            _droppedQueued = false;
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
            
            _positionCaster.GameObject.SetActive(false);

            Car.MainGame.OnSeatDroppedByPlayer();
        }

        private void SnapToGrid()
        {
            var success = Car.CheckSeatDrop(this, transform.localPosition, out var snappedPosition, out var newX, out var newY);
            
            var isSame = newX == X && newY == Y;
            _casterRed.GameObject.SetActive(!isSame && !success);
            _casterGreen.GameObject.SetActive(isSame || success);

            var pos = Car.transform.TransformPoint(snappedPosition);
            _positionCaster.Transform.position = pos;
            // transform.localPosition = snappedPosition;
        }

        private void RefreshAppearance(bool performMoveImmediately)
        {
            // Reset selection
            // TODO
            // OnFingerUp(null);

            if (performMoveImmediately)
            {
                var pos = Car.GetCellPosition(X, Y, false);
                transform.localPosition = pos;
            }
            
            _appearanceSingle.GameObject.SetActive(!_data.IsDouble);
            _appearanceDouble.GameObject.SetActive(_data.IsDouble);

            _collider.Comp.center = new Vector3(_data.IsDouble ? 0.5f : 0f, 0f, -0.5f);
            _collider.Comp.size = new Vector3(_data.IsDouble ? 1.9f : 0.9f, 0.9f, 1f);

            var colorIndex = SeatColor;

            if (colorIndex >= (int)SeatEnum.ANY)
            {
                _appearanceSingle.Comp.sprite = _seatSingleSprites[colorIndex - 1];
                _appearanceDouble.Comp.sprite = _seatDoubleSprites[colorIndex - 1];
            }
            else
            {
                _appearanceSingle.Comp.sprite = null;
                _appearanceDouble.Comp.sprite = null;
            }
            
            _positionCaster.GameObject.SetActive(false);
            _jumpModeSelectGudie.GameObject.SetActive(false);
        }
    }
}