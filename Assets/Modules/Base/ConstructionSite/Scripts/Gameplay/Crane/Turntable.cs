using UnityEngine;
using R3;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Turntable : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CraneSpecificationSO craneSpecification;
        [SerializeField] private Trolley trolley;
        
        private float _targetRotationSpeed;
        
        public ReactiveProperty<float> CurrentRotationAngle { get; } = new(0f);
        public ReactiveProperty<float> CurrentRotationSpeed { get; } = new(0f);

        private void FixedUpdate()
        {
            HandleRotation();
        }

        public void Rotate(float direction)
        {
            if (!craneSpecification) return;
            
            float absDir = Mathf.Abs(direction);
            if (absDir > 0.1f) // Threshold for ignore small jitter
                _targetRotationSpeed = craneSpecification.BaseRotationSpeed * Mathf.Sign(direction);
            else
                _targetRotationSpeed = 0f;
        }

        public void StopRotation() => _targetRotationSpeed = 0f;

        private void HandleRotation()
        {
            if (Mathf.Abs(CurrentRotationSpeed.Value) < 0.001f && Mathf.Abs(_targetRotationSpeed) < 0.001f)
                return;
            
            UpdateCurrentRotationSpeed();
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (Mathf.Abs(CurrentRotationSpeed.Value) > 0.001f)
            {
                float rotationAmount = CurrentRotationSpeed.Value * Time.fixedDeltaTime;
                CurrentRotationAngle.Value = NormalizeAngle(CurrentRotationAngle.Value + rotationAmount);
                transform.Rotate(0f, rotationAmount, 0f);
            }
        }

        private void UpdateCurrentRotationSpeed()
        {
            if (!craneSpecification) return;
            
            float acceleration = _targetRotationSpeed == 0f ? 
                craneSpecification.RotationDeceleration : 
                craneSpecification.RotationAcceleration;
            
            CurrentRotationSpeed.Value = Mathf.MoveTowards(
                CurrentRotationSpeed.Value, 
                _targetRotationSpeed, 
                acceleration * Time.fixedDeltaTime
            );
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}