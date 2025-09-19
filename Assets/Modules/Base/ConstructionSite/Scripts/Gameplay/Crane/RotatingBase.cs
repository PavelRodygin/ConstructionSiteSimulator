using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class RotatingBase : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CraneSpecificationSO craneSpecification;
        [SerializeField] private Trolley trolley;
        
        private float _currentRotationAngle;
        private float _currentCargoWeight;
        private bool _isRotatingLeft;
        private bool _isRotatingRight;
        private float _currentRotationSpeed;
        private float _targetRotationSpeed;
        
        /// <summary>
        /// Current rotation angle relative to the main crane support in degrees
        /// </summary>
        public float CurrentRotationAngle 
        { 
            get => _currentRotationAngle;
            private set => _currentRotationAngle = NormalizeAngle(value);
        }
        
        public float CurrentCargoWeight 
        { 
            get => _currentCargoWeight;
            set => _currentCargoWeight = Mathf.Max(0f, value);
        }
        
        public float RatedCargoWeight => craneSpecification.RatedCargoWeight;
        
        public float MaxCargoWeight => craneSpecification.MaxCargoWeight;
        
        public float CurrentRotationSpeed => _currentRotationSpeed;
        
        private void Update()
        {
            UpdateCargoWeight();
            HandleRotation();
        }
        
        private void UpdateCargoWeight()
        {
            // Automatically update cargo weight from trolley's hook
            if (trolley) CurrentCargoWeight = trolley.CurrentHookLoad;
        }
        
        private void HandleRotation()
        {
            UpdateTargetRotationSpeed();
            UpdateCurrentRotationSpeed();
            ApplyRotation();
        }
        
        private void UpdateTargetRotationSpeed()
        {
            if (_isRotatingLeft)
            {
                _targetRotationSpeed = -GetAdjustedRotationSpeed();
            }
            else if (_isRotatingRight)
            {
                _targetRotationSpeed = GetAdjustedRotationSpeed();
            }
            else
            {
                _targetRotationSpeed = 0f;
            }
        }
        
        private void UpdateCurrentRotationSpeed()
        {
            if (!craneSpecification) return;
            
            float acceleration = _targetRotationSpeed == 0f ? 
                craneSpecification.RotationDeceleration : 
                craneSpecification.RotationAcceleration;
            
            _currentRotationSpeed = Mathf.MoveTowards(
                _currentRotationSpeed, 
                _targetRotationSpeed, 
                acceleration * Time.deltaTime
            );
        }
        
        private void ApplyRotation()
        {
            if (Mathf.Abs(_currentRotationSpeed) > 0.001f)
            {
                float rotationAmount = _currentRotationSpeed * Time.deltaTime;
                CurrentRotationAngle += rotationAmount;
                transform.Rotate(0f, rotationAmount, 0f);
            }
        }
        
        private float GetAdjustedRotationSpeed()
        {
            if (!craneSpecification) return 0f;
            
            // No speed reduction up to rated capacity (10 tons)
            if (_currentCargoWeight <= craneSpecification.RatedCargoWeight)
            {
                return craneSpecification.BaseRotationSpeed;
            }
            
            // Progressive speed reduction from rated capacity to maximum load
            float excessWeight = _currentCargoWeight - craneSpecification.RatedCargoWeight;
            float excessCapacity = craneSpecification.MaxCargoWeight - craneSpecification.RatedCargoWeight;
            float overloadFactor = excessWeight / excessCapacity;
            
            // Speed reduction based on configuration
            float speedReduction = Mathf.Lerp(0f, craneSpecification.MaxSpeedReduction, overloadFactor);
            float adjustedSpeed = craneSpecification.BaseRotationSpeed * (1f - speedReduction);
            
            return Mathf.Max(craneSpecification.MinRotationSpeed, adjustedSpeed);
        }
        
        public void RotateLeft()
        {
            _isRotatingLeft = true;
            _isRotatingRight = false;
        }
        
        public void RotateRight()
        {
            _isRotatingLeft = false;
            _isRotatingRight = true;
        }
        
        public void StopRotation()
        {
            _isRotatingLeft = false;
            _isRotatingRight = false;
        }
        
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}