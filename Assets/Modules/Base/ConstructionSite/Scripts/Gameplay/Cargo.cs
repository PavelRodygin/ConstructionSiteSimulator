using UnityEngine;

namespace Modules.Base.ThirdPersonMPModule.Scripts.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Cargo : MonoBehaviour
    {
        [SerializeField] private Transform attachPoint;
        
        private Rigidbody _rigidbody;

        public int Weight => (int)(_rigidbody.mass * 9.81f);

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}