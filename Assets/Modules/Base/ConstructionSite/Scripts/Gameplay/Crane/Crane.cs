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
            // craneDashboard.Initialize();
        }

        private void Start()
        {
            if (_inputSystemService != null && !_isControlEnabled) EnableCraneControls();
        }

        public void EnableCraneControls()
        {
            if (_inputSystemService == null)
            {
                Debug.LogWarning("InputSystemService is null! Cannot enable crane controls.");
                return;
            }
            
            if (_isControlEnabled) return;
            
            _inputSystemService.SwitchToCrane();
            _isControlEnabled = true;
            
            SetupReactiveInput();
        }

        public void DisableCraneControls()
        {
            if (_inputSystemService == null) return;
            
            _inputSystemService.SwitchToUI();
            _isControlEnabled = false;
            
            _inputDisposables.Clear();
            
            turntable.StopRotation();
            trolley.StopMovement();
            trolley.StopHookMovement();
        }

        private void SetupReactiveInput()
        {
            var actionMap = _inputSystemService.InputActions.Crane;
            
            Observable.EveryUpdate()
                .Where(_ => _isControlEnabled)
                .Select(_ => actionMap.TurntableRotate.ReadValue<Vector2>())
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    if (Mathf.Abs(value.x) > 0.1f) // Threshold для jitter
                    {
                        turntable.Rotate(value.x);
                    }
                    else
                    {
                        turntable.Rotate(0f);
                    }
                })
                .AddTo(_inputDisposables);
            
            // Trolley movement: Polling states (forward/backward)
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
            
            // Hook movement: Polling states (up/down)
            Observable.EveryUpdate()
                .Where(_ => _isControlEnabled)
                .Select(_ => (up: actionMap.HookUp.IsPressed(), down: actionMap.HookDown.IsPressed()))
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case { up: true, down: false }:
                            trolley.MoveHookUp();
                            break;
                        case { down: true, up: false }:
                            trolley.MoveHookDown();
                            break;
                        default:
                            trolley.StopHookMovement();
                            break;
                    }
                })
                .AddTo(_inputDisposables);
            
            // Cargo attachment (discrete action, event-driven)
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