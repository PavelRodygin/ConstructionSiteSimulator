using UnityEngine;
using CodeBase.Services;
using CodeBase.Services.Input;
// using Modules.Base.GameModule.Scripts.Gameplay.Systems; // Removed - not needed for basic movement
using VContainer;

namespace Modules.Base.ThirdPersonMPModule.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInteractionController))]
    [RequireComponent(typeof(PlayerMoveController))]
    [RequireComponent(typeof(PlayerGfx))]
    public class Player : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private PlayerMoveController playerMoveController;
        [SerializeField] private PlayerInteractionController playerInteractionController;
        [SerializeField] private PlayerGfx playerGfx;
        [Space]
        // [SerializeField] private CinemachineVirtualCamera playerVirtualCamera; // Removed - using static camera
        [SerializeField] private Transform playerCameraTransform;
        [SerializeField] private CharacterController characterController;

        private InputSystemService _inputSystemService;
        private Transform _gameWorldTransform;
        private Vector3 _originalPosition;
        
        public bool IsInVehicle => playerInteractionController.IsPlayerInVehicle;

        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
            
            playerInteractionController.OnEnterTowTruck += EnterTowTruck;
            playerInteractionController.OnExitTowTruck += ExitTowTruck;
        }

        // Method to set GameWorldTransform from external source (e.g., GameManager)
        public void SetGameWorldTransform(Transform gameWorldTransform)
        {
            _gameWorldTransform = gameWorldTransform;
        }

        public void Initialize(bool isPlayerInVehicle)
        {
            playerInteractionController.Initialize(playerCameraTransform.transform, isPlayerInVehicle);
            playerMoveController.Initialize(playerCameraTransform, isPlayerInVehicle);
        }

        private void Awake()
        {
            if (playerMoveController == null)
                playerMoveController = GetComponent<PlayerMoveController>();

            if (playerInteractionController == null)
                playerInteractionController = GetComponent<PlayerInteractionController>();

            if (playerGfx == null)
                playerGfx = GetComponent<PlayerGfx>();
        }

        private void Update()
        {
            if (!playerInteractionController.IsPlayerInVehicle) playerMoveController.UpdateController();
        }

        private void LateUpdate()
        {
            if (!playerInteractionController.IsPlayerInVehicle) playerMoveController.LateUpdateController();
        }

        private void EnterTowTruck()
        {
            // Mock vehicle entry - not needed for basic movement
            Debug.Log("[Mock] Player entered vehicle");
        }

        private void ExitTowTruck()
        {
            // Mock vehicle exit - not needed for basic movement  
            Debug.Log("[Mock] Player exited vehicle");
        }

        private void OnDestroy()
        {
            playerInteractionController.OnEnterTowTruck -= EnterTowTruck;
            playerInteractionController.OnExitTowTruck -= ExitTowTruck;
        }
    }
}