using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CodeBase.Services.Input
{
    /// <summary>
    /// A service responsible for creating and managing the InputSystem_Actions instance and controlling Action Maps.
    /// </summary>
    public class InputSystemService : IStartable, IDisposable
    {
        private const string EventSystemObjectName = "EventSystem";

        private static InputSystemUIInputModule _uiInputModule;
        private static EventSystem _eventSystem;

        public InputSystem_Actions InputActions { get; private set; }
        
        public event Action OnSwitchToUI;
        public event Action OnSwitchToPlayerHumanoid;
        public event Action OnSwitchToCrane;
        // public event Action OnSwitchToWhatDoYouWant;
        // Also may be replaced by Action<ActionMap>
        
        public void Start()
        {
            CreateInputSystemActions();
            InitializeEventSystem();
            InputActions.UI.Enable(); 
        }
        
        /// <summary>
        /// Enables only the UI Action Map, keeping it always enabled.
        /// </summary>
        public void SwitchToUI()
        {
            InputActions.PlayerHumanoid.Disable();
            InputActions.Crane.Disable();
            InputActions.UI.Enable(); // Убеждаемся, что UI всегда включён
            Debug.Log("Switched to UI mode.");
            OnSwitchToUI?.Invoke();
        }

        /// <summary>
        /// Enables PlayerHumanoid Action Map, keeping UI enabled.
        /// </summary>
        public void SwitchToPlayerHumanoid()
        {
            InputActions.Crane.Disable();
            InputActions.PlayerHumanoid.Enable();
            InputActions.UI.Enable(); // UI остаётся включённым
            Debug.Log("Switched to PlayerHumanoid mode.");
            OnSwitchToPlayerHumanoid?.Invoke();
        }

        /// <summary>
        /// Enables Crane Action Map, keeping UI enabled.
        /// </summary>
        public void SwitchToCrane()
        {
            InputActions.PlayerHumanoid.Disable();
            InputActions.Crane.Enable();
            InputActions.UI.Enable(); // UI остаётся включённым
            Debug.Log("Switched to Crane mode.");
            OnSwitchToCrane?.Invoke();
        }

        /// <summary>
        /// Enables the UI Action Map without affecting other Action Maps (always enabled by default).
        /// </summary>
        public void EnableUI()
        {
            InputActions.UI.Enable();
            Debug.Log("UI Action Map ensured to be enabled.");
        }

        /// <summary>
        /// Disables the UI Action Map (now overridden to do nothing to keep UI always enabled).
        /// </summary>
        public void DisableUI()
        {
            Debug.LogWarning("UI Action Map cannot be disabled as per design.");
            InputActions.UI.Enable(); // Игнорируем попытку отключения
        }

        /// <summary>
        /// Checks if the UI Action Map is enabled.
        /// </summary>
        public bool IsUIInputEnabled() => InputActions.UI.enabled;

        /// <summary>
        /// Checks if the PlayerHumanoid Action Map is enabled.
        /// </summary>
        public bool IsPlayerHumanoidInputEnabled() => InputActions.PlayerHumanoid.enabled;

        /// <summary>
        /// Checks if the Crane Action Map is enabled.
        /// </summary>
        public bool IsCraneInputEnabled() => InputActions.Crane.enabled;
        
        /// <summary>
        /// Sets the first selected object for UI navigation.
        /// </summary>
        /// <param name="selectedObject">The object to be set as the first selected.</param>
        public void SetFirstSelectedObject(Selectable selectedObject)
        {
            if (!_eventSystem)
            {
                Debug.LogWarning("EventSystem is not initialized. Cannot set first selected object.");
                return;
            }

            if (!selectedObject)
            {
                Debug.LogWarning("Selected object is null. Cannot set first selected object.");
                return;
            }

            _eventSystem.SetSelectedGameObject(selectedObject.gameObject);
        }
        
        public string GetStringActionPath(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot get full path.");
                return string.Empty;
            }

            string mapName = action.actionMap?.name ?? "UnknownMap";
            string actionName = action.name;
            return $"{mapName}/{actionName}";
        }
        
        public Observable<Unit> GetStartedObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Started Observable.");
                return Observable.Empty<Unit>();
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.started += h,
                h => action.started -= h
            ).Select(_ => Unit.Default);
        }
        
        public Observable<Unit> GetPerformedObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Performed Observable.");
                return Observable.Empty<Unit>();
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.performed += h,
                h => action.performed -= h
            ).Select(_ => Unit.Default);
        }
        
        public Observable<Unit> GetCanceledObservable(InputAction action)
        {
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot create Canceled Observable.");
                return Observable.Empty<Unit>();
            }

            return Observable.FromEvent(
                (Action<InputAction.CallbackContext> h) => action.canceled += h,
                h => action.canceled -= h
            ).Select(_ => Unit.Default);
        }

        //The reference of getting observables:
        // public static Observable<Unit> AsObservable(this UnityEngine.Events.UnityEvent unityEvent, CancellationToken cancellationToken = default)
        // {
        //     return Observable.FromEvent(h => new UnityAction(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        // }
        //
        // public static Observable<T> AsObservable<T>(this UnityEngine.Events.UnityEvent<T> unityEvent, CancellationToken cancellationToken = default)
        // {
        //     return Observable.FromEvent<UnityAction<T>, T>(h => new UnityAction<T>(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        // }
        
        public void Dispose()
        {
            if (InputActions == null) return;

            InputActions.PlayerHumanoid.Disable();
            InputActions.Crane.Disable();
            InputActions.UI.Disable();
            
            InputActions.Disable();
            InputActions.Dispose();
        }

        /// <summary>
        /// Initializes the EventSystem, creating a new one if it doesn't exist.
        /// </summary>
        private void InitializeEventSystem()    
        {
            _eventSystem = Object.FindObjectOfType<EventSystem>();
            if (!_eventSystem)
            {
                _eventSystem = CreateEventSystem();
                _uiInputModule.actionsAsset = InputActions.asset;
                Object.DontDestroyOnLoad(_eventSystem.gameObject);
            }
            else
                Debug.Log("Found existing EventSystem.");
        }

        /// <summary>
        /// Initializes the InputSystem_Actions.
        /// </summary>
        private void CreateInputSystemActions() => InputActions = new InputSystem_Actions();

        /// <summary>
        /// Creates a new EventSystem with an InputSystemUIInputModule.
        /// </summary>
        /// <returns>The created EventSystem.</returns>
        private static EventSystem CreateEventSystem()
        {
            var eventSystem = new GameObject(EventSystemObjectName).AddComponent<EventSystem>();
            _uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            return eventSystem;
        }
    }
}