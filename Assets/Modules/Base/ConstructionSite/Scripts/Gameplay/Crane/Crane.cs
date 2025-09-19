using System;
using CodeBase.Services.Input;
using UnityEngine;
using VContainer;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Crane : MonoBehaviour
    {
        [SerializeField] private RotatingBase rotatingBase;
        [SerializeField] private Trolley trolley;
        
        private InputSystemService _inputSystemService;
        private bool _isControlEnabled = true;
        
        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }

        private void Awake()
        {
            EnableCraneControls();
        }

        private void Update()
        {
            if (!_isControlEnabled || _inputSystemService == null) return;
            
            HandleRotationInput();
            HandleTrolleyInput();
            HandleHookInput();
        }

        public void EnableCraneControls()
        {
            if (_inputSystemService == null) return;
            
            _inputSystemService.SwitchToCrane();
            _isControlEnabled = true;
        }

        public void DisableCraneControls()
        {
            if (_inputSystemService == null) return;
            
            _inputSystemService.SwitchToUI();
            _isControlEnabled = false;
        }

        private void HandleRotationInput()
        {
            bool turnLeft = _inputSystemService.InputActions.Crane.TurnLeft.IsPressed();
            bool turnRight = _inputSystemService.InputActions.Crane.TurnRight.IsPressed();
            
            if (turnLeft && !turnRight)
                rotatingBase.RotateLeft();
            else if (turnRight && !turnLeft)
                rotatingBase.RotateRight();
            else
                rotatingBase.StopRotation();
        }
        
        private void HandleTrolleyInput()
        {
            bool trolleyForward = _inputSystemService.InputActions.Crane.TrolleyForward.IsPressed();
            bool trolleyBackward = _inputSystemService.InputActions.Crane.TrolleyBackward.IsPressed();
            
            if (trolleyForward && !trolleyBackward)
                trolley.MoveForward();
            else if (trolleyBackward && !trolleyForward)
                trolley.MoveBackward();
            else
                trolley.StopMovement();
        }
        
        private void HandleHookInput()
        {
            bool hookDown = _inputSystemService.InputActions.Crane.HookDown.IsPressed();
            bool hookUp = _inputSystemService.InputActions.Crane.HookUp.IsPressed();
            
            if (hookDown && !hookUp)
                trolley.MoveHookDown();
            else if (hookUp && !hookDown)
                trolley.MoveHookUp();
            else
                trolley.StopHookMovement();
        }

        private void OnDestroy()
        {
            DisableCraneControls();
        }
    }
}
