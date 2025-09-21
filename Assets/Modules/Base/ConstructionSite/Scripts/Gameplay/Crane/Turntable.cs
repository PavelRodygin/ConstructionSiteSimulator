using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Turntable : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CraneSpecificationSO craneSpecification;
        [SerializeField] private Trolley trolley;
        
        private float _currentRotationAngle;
        private float _currentCargoWeight;
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
        
        public float CurrentRotationSpeed => _currentRotationSpeed;
        
        private void Update()
        {
            UpdateCargoWeight();
        }

        private void FixedUpdate()
        {
            HandleRotation();
        }

        public void Rotate(float direction)
        {
            if (!craneSpecification) return;
            
            float absDir = Mathf.Abs(direction);
            if (absDir > 0.1f) // Threshold for ignore small jitter
                _targetRotationSpeed = GetAdjustedRotationSpeed() * Mathf.Sign(direction);
            else
                _targetRotationSpeed = 0f;
        }

        public void StopRotation() => _targetRotationSpeed = 0f;

        private void UpdateCargoWeight()
        {
            // Automatically update cargo weight from trolley's hook
            if (trolley) CurrentCargoWeight = trolley.CurrentHookLoad;
        }

        private void HandleRotation()
        {
            if (Mathf.Abs(_currentRotationSpeed) < 0.001f && Mathf.Abs(_targetRotationSpeed) < 0.001f)
                return;
            
            UpdateCurrentRotationSpeed();
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (Mathf.Abs(_currentRotationSpeed) > 0.001f)
            {
                float rotationAmount = _currentRotationSpeed * Time.fixedDeltaTime;
                CurrentRotationAngle += rotationAmount;
                transform.Rotate(0f, rotationAmount, 0f);
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
                acceleration * Time.fixedDeltaTime
            );
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

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}