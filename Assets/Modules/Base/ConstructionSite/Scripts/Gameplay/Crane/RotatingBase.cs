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
        
        /// <summary>
        /// Current rotation angle relative to the main crane support in degrees
        /// </summary>
        public float CurrentRotationAngle 
        { 
            get => _currentRotationAngle;
            private set => _currentRotationAngle = NormalizeAngle(value);
        }
        
        /// <summary>
        /// Current cargo weight that affects rotation speed (in kg)
        /// </summary>
        public float CurrentCargoWeight 
        { 
            get => _currentCargoWeight;
            set => _currentCargoWeight = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// Rated crane capacity in kg (10 tons)
        /// </summary>
        public float RatedCargoWeight => craneSpecification.RatedCargoWeight;
        
        /// <summary>
        /// Maximum crane load in kg (40 tons)
        /// </summary>
        public float MaxCargoWeight => craneSpecification.MaxCargoWeight;
        
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
            if (_isRotatingLeft)
            {
                float rotationAmount = GetAdjustedRotationSpeed() * Time.deltaTime;
                CurrentRotationAngle -= rotationAmount;
                transform.Rotate(0f, -rotationAmount, 0f);
            }
            else if (_isRotatingRight)
            {
                float rotationAmount = GetAdjustedRotationSpeed() * Time.deltaTime;
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