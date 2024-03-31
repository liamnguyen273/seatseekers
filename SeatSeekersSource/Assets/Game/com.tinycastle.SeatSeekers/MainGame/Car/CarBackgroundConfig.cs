using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class CarBackgroundConfig : MonoBehaviour
    {
        [SerializeField] private float _width3Size;
        [SerializeField] private float _height4Size;
        [SerializeField] private float _expand;

        public float Width3Size => _width3Size;
        public float Height4Size => _height4Size;
        public float Expand => _expand;
    }
}