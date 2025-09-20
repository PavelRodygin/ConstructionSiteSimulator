using CodeBase.Services.Input;
using R3;
using UnityEngine;
using VContainer;
using System;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Crane : MonoBehaviour
    {
        [SerializeField] private Turntable turntable;
        [SerializeField] private Trolley trolley;
        [SerializeField] private CraneDashboard craneDashboard;
        
        private InputSystemService _inputSystemService;
        private Camera _moduleCamera;
        
        private bool _isControlEnabled;
        private readonly CompositeDisposable _inputDisposables = new();
        
        [Inject]
        private void Construct(InputSystemService inputSystemService, Camera moduleCamera)
        {
            _inputSystemService = inputSystemService;
            
            // Initialize controls after injection if the object is already active
            if (gameObject.activeInHierarchy && enabled)
            {
                EnableCraneControls();
            }
        }

        private void Start()
        {
            // Only enable if not already enabled in Construct
            if (_inputSystemService != null && !_isControlEnabled)
            {
                EnableCraneControls();
            }
        }

        public void EnableCraneControls()
        {
            if (_inputSystemService == null)
            {
                Debug.LogWarning("InputSystemService is null! Cannot enable crane controls.");
                return;
            }
            
            if (_isControlEnabled) return; // Already enabled
            
            _inputSystemService.SwitchToCrane();
            _isControlEnabled = true;
            
            SetupReactiveInput();
        }

        public void DisableCraneControls()
        {
            if (_inputSystemService == null) return;
            
            _inputSystemService.SwitchToUI();
            _isControlEnabled = false;
            
            // Dispose all input subscriptions
            _inputDisposables.Clear();
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
                            turntable.RotateLeft();
                            break;
                        case { right: true, left: false }:
                            turntable.RotateRight();
                            break;
                        default:
                            turntable.StopRotation();
                            break;
                    }
                })
                .AddTo(_inputDisposables);
            
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
                .AddTo(_inputDisposables);
            
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
                .AddTo(_inputDisposables);
            
            // Cargo attachment (discrete action)
            _inputSystemService.GetPerformedObservable(actionMap.AttachCargo)
                .Where(_ => _isControlEnabled)
                .ThrottleFirst(TimeSpan.FromMilliseconds(200))
                .Subscribe(_ => trolley.ToggleCargoAttachment())
                .AddTo(_inputDisposables);
        }
        
        private void OnDestroy()
        {
            _inputDisposables?.Dispose();
        }
    }
}
