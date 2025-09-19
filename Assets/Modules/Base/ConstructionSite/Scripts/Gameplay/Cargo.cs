using UnityEngine;

namespace Modules.Base.ThirdPersonMPModule.Scripts.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Cargo : MonoBehaviour
    {
        [Header("Cargo Configuration")]
        [SerializeField] private Transform attachPoint;
        [SerializeField] private bool isAttachable = true;

        public Rigidbody Rigidbody { get; private set; }
        public Transform AttachPoint => attachPoint ? attachPoint : transform;
        public int Weight => (int)(Rigidbody.mass * 9.81f);
        public float Mass => Rigidbody.mass;
        public bool IsAttachable => isAttachable && !IsAttached;
        public bool IsAttached { get; internal set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            
            if (!Rigidbody) 
                Debug.LogError($"Cargo {name} is missing Rigidbody component!");
        }
        
        public virtual void OnAttached()
        {
            IsAttached = true;
            Debug.Log($"Cargo {name} attached (Weight: {Weight}kg)");
        }
        
        public virtual void OnDetached()
        {
            IsAttached = false;
            Debug.Log($"Cargo {name} detached");
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!attachPoint) return;
            
            Gizmos.color = IsAttached ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(attachPoint.position, 0.3f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, attachPoint.position);
        }
    }
}