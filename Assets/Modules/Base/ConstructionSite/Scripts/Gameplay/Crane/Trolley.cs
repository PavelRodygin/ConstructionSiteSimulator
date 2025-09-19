using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Trolley : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CraneSpecificationSO craneSpecification;
        
        [Header("Components")]
        [SerializeField] private Hook hook;
        [SerializeField] private Transform cableAnchor;
        
        [Header("Movement Constraints")]
        [SerializeField] private Transform minPositionMarker;
        [SerializeField] private Transform maxPositionMarker;

        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _currentPosition; // [0; 1;]
        private bool _isMovingForward;
        private bool _isMovingBackward;

        private float _currentHookDepth; //[0; 1;]
        private bool _isHookMovingDown;
        private bool _isHookMovingUp;
        
        /// <summary>
        /// Current trolley position relative to the closest point to crane base (0-1 range)
        /// </summary>
        public float CurrentPosition 
        { 
            get => _currentPosition;
            private set => _currentPosition = Mathf.Clamp01(value);
        }
        
        /// <summary>
        /// Current hook depth relative to cable anchor (0-1 range)
        /// </summary>
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
                
                return hook.transform.position - cableAnchor.position;
            }
        }
        
        /// <summary>
        /// Current hook target position from Joint
        /// </summary>
        public Vector3 HookTargetPosition
        {
            get
            {
                if (!hook || !hook.Joint) return Vector3.zero;
                return hook.Joint.targetPosition;
            }
        }
        
        /// <summary>
        /// Current load on the hook in kg
        /// </summary>
        public float CurrentHookLoad => hook ? hook.CurrentLoadKg : 0f;
        
        /// <summary>
        /// Current load on the hook joint in Newtons
        /// </summary>
        public float CurrentHookLoadNewtons => hook ? hook.CurrentLoad : 0f;
        
        /// <summary>
        /// Whether the hook has cargo attached
        /// </summary>
        public bool HasCargoAttached => hook && hook.HasCargoAttached;
        
        /// <summary>
        /// Maximum trolley travel distance
        /// </summary>
        public float MaxTravelDistance => craneSpecification != null ? craneSpecification.TrolleyMaxDistance : 20f;
        
        private void Start()
        {
            InitializePositions();
            InitializeHook();
        }
        
        private void Update()
        {
            HandleMovement();
            HandleHookMovement();
        }
        
        private void InitializePositions()
        {
            // Calculate current position based on transform position
            UpdateCurrentPositionFromTransform();
        }
        
        private void UpdateMarkerPositions()
        {
            if (minPositionMarker && maxPositionMarker)
            {
                _startPosition = minPositionMarker.position;
                _endPosition = maxPositionMarker.position;
            }
            else
            {
                // Fallback: use current position as start and calculate end based on max distance
                _startPosition = transform.position;
                _endPosition = _startPosition + transform.forward * MaxTravelDistance;
            }
        }
        
        private void UpdateCurrentPositionFromTransform()
        {
            // Update marker positions first
            UpdateMarkerPositions();
            
            Vector3 totalDistance = _endPosition - _startPosition;
            Vector3 currentDistance = transform.position - _startPosition;

            if (!(totalDistance.magnitude > 0.01f)) return;
            
            _currentPosition = Vector3.Dot(currentDistance, totalDistance.normalized) / totalDistance.magnitude;
            _currentPosition = Mathf.Clamp01(_currentPosition);
        }
        
        private void InitializeHook()
        {
            if (!hook || !hook.Joint) return;
            
            // Set initial hook position to the anchor level (depth = 0)
            Vector3 initialTarget = hook.Joint.targetPosition;
            initialTarget.y = 0f;
            hook.Joint.targetPosition = initialTarget;
            _currentHookDepth = 0f;
        }
        
        private void HandleMovement()
        {
            if (craneSpecification == null) return;
            
            UpdateMarkerPositions();
            
            float moveSpeed = craneSpecification.TrolleyMoveSpeed;
            float deltaTime = Time.deltaTime;
            
            if (_isMovingForward && _currentPosition < 1f)
            {
                float moveAmount = moveSpeed * deltaTime / MaxTravelDistance;
                CurrentPosition += moveAmount;
                UpdateTransformPosition();
            }
            else if (_isMovingBackward && _currentPosition > 0f)
            {
                float moveAmount = moveSpeed * deltaTime / MaxTravelDistance;
                CurrentPosition -= moveAmount;
                UpdateTransformPosition();
            }
        }
        
        private void UpdateTransformPosition()
        {
            Vector3 targetPosition = Vector3.Lerp(_startPosition, _endPosition, _currentPosition);
            transform.position = targetPosition;
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
        
        /// <summary>
        /// Sets trolley position directly (0 = closest to crane base, 1 = farthest)
        /// </summary>
        public void SetPosition(float position)
        {
            CurrentPosition = position;
            UpdateMarkerPositions();
            UpdateTransformPosition();
        }
        
        private void HandleHookMovement()
        {
            if (!craneSpecification || !hook) return;
            
            float moveSpeed = craneSpecification.HookMoveSpeed;
            float maxDepth = craneSpecification.HookMaxDepth;
            float deltaTime = Time.deltaTime;
            
            if (_isHookMovingDown && _currentHookDepth < 1f)
            {
                float moveAmount = moveSpeed * deltaTime / maxDepth;
                CurrentHookDepth += moveAmount;
                UpdateHookPosition();
            }
            else if (_isHookMovingUp && _currentHookDepth > 0f)
            {
                float moveAmount = moveSpeed * deltaTime / maxDepth;
                CurrentHookDepth -= moveAmount;
                UpdateHookPosition();
            }
        }
        
        private void UpdateHookPosition()
        {
            if (!hook || !hook.Joint) return;
            
            float maxDepth = craneSpecification ? craneSpecification.HookMaxDepth : 15f;
            
            // Update the Y component of the joint's target position to control hook depth
            Vector3 currentTarget = hook.Joint.targetPosition;
            currentTarget.y = -maxDepth * _currentHookDepth;
            hook.Joint.targetPosition = currentTarget;
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
        
        /// <summary>
        /// Sets hook depth directly (0 = at anchor level, 1 = max depth)
        /// </summary>
        public void SetHookDepth(float depth)
        {
            CurrentHookDepth = depth;
            UpdateHookPosition();
        }
        
        /// <summary>
        /// Attempts to attach cargo to the hook
        /// </summary>
        public bool AttachCargo()
        {
            return hook && hook.AttachCargo();
        }
        
        /// <summary>
        /// Detaches cargo from the hook
        /// </summary>
        public void DetachCargo()
        {
            if (hook) hook.TryDetachCargo();
        }
        
        /// <summary>
        /// Toggles cargo attachment on the hook
        /// </summary>
        public void ToggleCargoAttachment()
        {
            if (hook) hook.ToggleCargoAttachment();
        }
    }
}
