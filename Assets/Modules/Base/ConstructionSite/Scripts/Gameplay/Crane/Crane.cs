using CodeBase.Services.Input;
using R3;
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

        private void Start()
        {
            EnableCraneControls();
        }

        public void EnableCraneControls()
        {
            if (_inputSystemService == null) return;
            
            _inputSystemService.SwitchToCrane();
            _isControlEnabled = true;
            
            SetupReactiveInput();
        }

        public void DisableCraneControls()
        {
            if (_inputSystemService == null) return;
            
            _inputSystemService.SwitchToUI();
            _isControlEnabled = false;
        }

        private void SetupReactiveInput()
        {
            var actionMap = _inputSystemService.InputActions.Crane;
            
            // Rotation input with state caching
            Observable.EveryUpdate()
                .Where(_ => _isControlEnabled)
                .Select(_ => (left: actionMap.TurnLeft.IsPressed(), right: actionMap.TurnRight.IsPressed()))
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case { left: true, right: false }:
                            rotatingBase.RotateLeft();
                            break;
                        case { right: true, left: false }:
                            rotatingBase.RotateRight();
                            break;
                        default:
                            rotatingBase.StopRotation();
                            break;
                    }
                })
                .AddTo(this);
            
            // Trolley input with state caching
            Observable.EveryUpdate()
                .Where(_ => _isControlEnabled)
                .Select(_ => (forward: actionMap.TrolleyForward.IsPressed(), backward: actionMap.TrolleyBackward.IsPressed()))
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case { forward: true, backward: false }:
                            trolley.MoveForward();
                            break;
                        case { backward: true, forward: false }:
                            trolley.MoveBackward();
                            break;
                        default:
                            trolley.StopMovement();
                            break;
                    }
                })
                .AddTo(this);
            
            // Hook movement input with state caching
            Observable.EveryUpdate()
                .Where(_ => _isControlEnabled)
                .Select(_ => (down: actionMap.HookDown.IsPressed(), up: actionMap.HookUp.IsPressed()))
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case { down: true, up: false }:
                            trolley.MoveHookDown();
                            break;
                        case { up: true, down: false }:
                            trolley.MoveHookUp();
                            break;
                        default:
                            trolley.StopHookMovement();
                            break;
                    }
                })
                .AddTo(this);
            
            // Cargo attachment (discrete action)
            Observable.EveryUpdate()
                .Where(_ => _isControlEnabled && actionMap.AttachCargo.WasPressedThisFrame())
                .Subscribe(_ => trolley.ToggleCargoAttachment())
                .AddTo(this);
        }
    }
}
