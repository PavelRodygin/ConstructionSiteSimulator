using R3;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Trolley : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CraneSpecificationSO craneSpecification;
        
        [SerializeField] private Hook hook;
        [SerializeField] private Transform cableAnchor;
        
        [Header("Movement Constraints")]
        [SerializeField] private Transform mastTowerBorderPoint;
        [SerializeField] private Transform jibEndBorderPoint;

        private Vector3 _localStartPosition;
        private Vector3 _localEndPosition;  
        private float _maxTravelDistance;
        private float _currentPosition; // [0; 1]
        private float _movementDirection; // -1 backward, 0 stop, 1 forward

        private float _currentHookDepth; // [0; 1]
        private float _hookDirection; // -1 up, 0 stop, 1 down
        
        public ReactiveProperty<float> RelativeZPosition { get; } = new(0f);
        
        public float CurrentPosition 
        { 
            get => _currentPosition;
            private set => _currentPosition = Mathf.Clamp01(value);
        }
        
        public float CurrentHookDepth
        {
            get => _currentHookDepth;
            private set => _currentHookDepth = Mathf.Clamp01(value);
        }
        
        public float CurrentHookLoad => hook ? hook.CurrentLoadKg : 0f;
        
        private void Start()
        {
            SetupLocalMarkerPositions();
            UpdateCurrentPositionFromTransform();
            InitializeHook();
            
            RelativeZPosition.Value = _currentPosition * _maxTravelDistance;
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
            HandleHookMovement();
        }
        
        private void InitializeHook()
        {
            if (!hook || !hook.WireJoint) return;
            
            Vector3 initialTarget = hook.WireJoint.targetPosition;
            initialTarget.y = 0f;
            hook.WireJoint.targetPosition = initialTarget;
            _currentHookDepth = 0f;
        }

        public void MoveForward() => _movementDirection = 1f;

        public void MoveBackward() => _movementDirection = -1f;

        public void StopMovement() => _movementDirection = 0f;

        public void MoveHookDown() => _hookDirection = 1f;

        public void MoveHookUp() => _hookDirection = -1f;

        public void StopHookMovement() => _hookDirection = 0f;

        public void ToggleCargoAttachment() => hook.ToggleCargoAttachment();

        private void SetupLocalMarkerPositions()
        {
            if (mastTowerBorderPoint && jibEndBorderPoint && transform.parent)
            {
                _localStartPosition = transform.parent.InverseTransformPoint(mastTowerBorderPoint.position);
                _localEndPosition = transform.parent.InverseTransformPoint(jibEndBorderPoint.position);
            }
            else
            {
                // Fallback: from 0 to 30m
                _localStartPosition = Vector3.zero;
                _localEndPosition = new Vector3(0, 0, 30f);
            }
            
            _maxTravelDistance = Vector3.Distance(_localStartPosition, _localEndPosition);
            if (_maxTravelDistance < 0.01f) _maxTravelDistance = 30f; // protection for zero distance
        }

        private void UpdateCurrentPositionFromTransform()
        {
            if (transform.parent == null) return;
            
            Vector3 localPos = transform.localPosition;
            
            Vector3 totalLocalDistance = _localEndPosition - _localStartPosition;
            Vector3 currentLocalDistance = localPos - _localStartPosition;

            if (totalLocalDistance.magnitude > 0.01f)
            {
                _currentPosition = Vector3.Dot(currentLocalDistance, totalLocalDistance.normalized) / totalLocalDistance.magnitude;
                _currentPosition = Mathf.Clamp01(_currentPosition);
            }
        }

        private void HandleMovement()
        {
            if (!craneSpecification || !transform.parent || _movementDirection == 0f) return;
            
            float moveSpeed = craneSpecification.TrolleyMoveSpeed;
            float deltaTime = Time.fixedDeltaTime;
            float normalizedSpeed = moveSpeed * deltaTime / _maxTravelDistance * _movementDirection;
            
            float newPosition = CurrentPosition + normalizedSpeed;
            
            if ((_movementDirection > 0 && newPosition <= 1f) || (_movementDirection < 0 && newPosition >= 0f))
            {
                CurrentPosition = newPosition;
                UpdateTransformLocalPosition();
                
                // Update reactive
                RelativeZPosition.Value = _currentPosition * _maxTravelDistance;
            }
        }

        private void UpdateTransformLocalPosition()
        {
            Vector3 targetLocalPosition = Vector3.Lerp(_localStartPosition, _localEndPosition, _currentPosition);
            transform.localPosition = targetLocalPosition;
        }

        private void HandleHookMovement()
        {
            if (!craneSpecification || !hook || _hookDirection == 0f) return;
            
            float moveSpeed = craneSpecification.HookMoveSpeed;
            float maxDepth = craneSpecification.HookMaxDepth;
            float deltaTime = Time.fixedDeltaTime;
            float normalizedSpeed = moveSpeed * deltaTime / maxDepth * _hookDirection;
            
            float newDepth = CurrentHookDepth + normalizedSpeed;
            
            if ((_hookDirection > 0 && newDepth <= 1f) || (_hookDirection < 0 && newDepth >= 0f))
            {
                CurrentHookDepth = newDepth;
                UpdateHookPosition();
            }
        }

        private void UpdateHookPosition()
        {
            if (!hook || !hook.WireJoint) return;
            
            float maxDepth = craneSpecification ? craneSpecification.HookMaxDepth : 15f;
            
            Vector3 currentTarget = hook.WireJoint.targetPosition;
            currentTarget.y = -maxDepth * _currentHookDepth;
            hook.WireJoint.targetPosition = currentTarget;
        }
    }
}