using UnityEngine;
using VContainer;

namespace Modules.Base.ThirdPersonMPModule.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerGfx : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        public AudioClip landingAudioClip;
        public AudioClip[] footstepAudioClips;
        [Range(0, 1)] public float footstepAudioVolume = 0.5f;

        private Animator _animator;
        private CharacterController _characterController;
        private PlayerMoveController _moveController;
        private bool _animationsEnabled = true;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        [Inject]
        private void Construct(PlayerMoveController moveController)
        {
            _moveController = moveController;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();

            AssignAnimationIDs();
        }

        private void Update()
        {
            if (_animationsEnabled) UpdateAnimations();
        }
        
        public void OnTowTruckEntered()
        {
            if (!_animationsEnabled) return; // Avoid redundant calls

            _animationsEnabled = false;
            _animator.enabled = false; // Disable animator to freeze animations
        }

        public void OnTowTruckExited()
        {
            if (_animationsEnabled) return; // Avoid redundant calls

            _animationsEnabled = true;
            _animator.enabled = true; // Re-enable animator to resume animations
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void UpdateAnimations()
        {
            if (_animator == null || _moveController == null) return;
            
            _animator.SetFloat(_animIDSpeed, _moveController.CurrentSpeed);
            _animator.SetFloat(_animIDMotionSpeed, _moveController.InputMagnitude);
            _animator.SetBool(_animIDGrounded, _moveController.IsGrounded);

            if (_moveController.IsJumping)
            {
                _animator.SetBool(_animIDJump, true);
            }

            if (_moveController.IsFalling)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }
            else
            {
                _animator.SetBool(_animIDFreeFall, false);
            }
        }


        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (!(animationEvent.animatorClipInfo.weight > 0.5f) || footstepAudioClips.Length <= 0) return;
            
            int index = Random.Range(0, footstepAudioClips.Length);
            if (audioSource != null) 
                audioSource.PlayOneShot(footstepAudioClips[index], footstepAudioVolume);
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (audioSource != null) 
                    audioSource.PlayOneShot(landingAudioClip, footstepAudioVolume);
            }
            
            _animator.SetBool(_animIDJump, false);
        }
    }
}