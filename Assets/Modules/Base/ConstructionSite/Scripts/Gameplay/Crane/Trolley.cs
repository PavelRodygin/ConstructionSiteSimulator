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
        private float _currentPosition; // [0; 1]
        private bool _isMovingForward;
        private bool _isMovingBackward;

        private float _currentHookDepth; // [0; 1]
        private bool _isHookMovingDown;
        private bool _isHookMovingUp;
        
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
        
        public Vector3 HookPositionRelativeToAnchor
        {
            get
            {
                if (!hook || !cableAnchor) return Vector3.zero;
                return hook.transform.localPosition - cableAnchor.localPosition;
            }
        }
        
        public Vector3 HookTargetPosition
        {
            get
            {
                if (!hook || !hook.Joint) return Vector3.zero;
                return hook.Joint.targetPosition;
            }
        }
        
        public float CurrentHookLoad => hook ? hook.CurrentLoadKg : 0f;
        
        public float CurrentHookLoadNewtons => hook ? hook.CurrentLoad : 0f;
        
        public bool HasCargoAttached => hook && hook.HasCargoAttached;
        
        public float MaxTravelDistance => craneSpecification != null ? craneSpecification.TrolleyMaxDistance : 20f;
        
        private void Start()
        {
            UpdateLocalMarkerPositions();
            UpdateCurrentPositionFromTransform();
            InitializeHook();
        }
        
        private void Update()
        {
            HandleMovement();
            HandleHookMovement();
        }
        
        private void InitializeHook()
        {
            if (!hook || !hook.Joint) return;
            
            Vector3 initialTarget = hook.Joint.targetPosition;
            initialTarget.y = 0f;
            hook.Joint.targetPosition = initialTarget;
            _currentHookDepth = 0f;
        }

        public void MoveForward()
        {
            _isMovingForward = true;
            _isMovingBackward = false;
        }

        public void MoveBackward()
        {
            _isMovingForward = false;
            _isMovingBackward = true;
        }

        public void StopMovement()
        {
            _isMovingForward = false;
            _isMovingBackward = false;
        }

        public void MoveHookDown()
        {
            _isHookMovingDown = true;
            _isHookMovingUp = false;
        }

        public void MoveHookUp()
        {
            _isHookMovingDown = false;
            _isHookMovingUp = true;
        }

        public void StopHookMovement()
        {
            _isHookMovingDown = false;
            _isHookMovingUp = false;
        }
        
        public void ToggleCargoAttachment()
        {
            if (hook) hook.ToggleCargoAttachment();
        }

        private void UpdateLocalMarkerPositions()
        {
            if (mastTowerBorderPoint && jibEndBorderPoint && transform.parent)
            {
                _localStartPosition = transform.parent.InverseTransformPoint(mastTowerBorderPoint.position);
                _localEndPosition = transform.parent.InverseTransformPoint(jibEndBorderPoint.position);
            }
            else
            {
                _localStartPosition = Vector3.zero;
                _localEndPosition = new Vector3(0, 0, MaxTravelDistance);
            }
        }

        private void UpdateCurrentPositionFromTransform()
        {
            UpdateLocalMarkerPositions();
            
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
            if (!craneSpecification || transform.parent == null) return;
            
            UpdateLocalMarkerPositions();
            
            float moveSpeed = craneSpecification.TrolleyMoveSpeed;
            float deltaTime = Time.deltaTime;
            float normalizedSpeed = moveSpeed * deltaTime / MaxTravelDistance;
            
            if (_isMovingForward && _currentPosition < 1f)
            {
                CurrentPosition += normalizedSpeed;
                UpdateTransformLocalPosition();
            }
            else if (_isMovingBackward && _currentPosition > 0f)
            {
                CurrentPosition -= normalizedSpeed;
                UpdateTransformLocalPosition();
            }
        }

        private void UpdateTransformLocalPosition()
        {
            Vector3 targetLocalPosition = Vector3.Lerp(_localStartPosition, _localEndPosition, _currentPosition);
            transform.localPosition = targetLocalPosition;
        }

        private void HandleHookMovement()
        {
            if (!craneSpecification || !hook) return;
            
            float moveSpeed = craneSpecification.HookMoveSpeed;
            float maxDepth = craneSpecification.HookMaxDepth;
            float deltaTime = Time.deltaTime;
            float normalizedSpeed = moveSpeed * deltaTime / maxDepth;
            
            if (_isHookMovingDown && _currentHookDepth < 1f)
            {
                CurrentHookDepth += normalizedSpeed;
                UpdateHookPosition();
            }
            else if (_isHookMovingUp && _currentHookDepth > 0f)
            {
                CurrentHookDepth -= normalizedSpeed;
                UpdateHookPosition();
            }
        }

        private void UpdateHookPosition()
        {
            if (!hook || !hook.Joint) return;
            
            float maxDepth = craneSpecification ? craneSpecification.HookMaxDepth : 15f;
            
            Vector3 currentTarget = hook.Joint.targetPosition;
            currentTarget.y = -maxDepth * _currentHookDepth;
            hook.Joint.targetPosition = currentTarget;
        }
    }
}