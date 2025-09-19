using System.Collections.Generic;
using Modules.Base.ThirdPersonMPModule.Scripts.Gameplay;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Hook : MonoBehaviour
    {
        [Header("Hook Configuration")]
        [SerializeField] private float attachmentRadius = 2f;
        
        [Header("Cargo Joint Configuration")]
        [SerializeField] private float cargoJointSpring = 10000f;
        [SerializeField] private float cargoJointDamper = 1000f;
        [SerializeField] private float cargoJointMaxForce = 100000f;

        private readonly List<Cargo> _nearbyCargoList = new();

        private ConfigurableJoint _cargoJoint;
        private SphereCollider _triggerCollider;
        
        [field: SerializeField] public ConfigurableJoint Joint { get; private set; }
        
        public Cargo CurrentCargo { get; private set; }
        public bool HasCargoAttached => CurrentCargo != null;
        public float CurrentLoad
        {
            get
            {
                if (!Joint || !Joint.connectedBody) return 0f;
                
                var force = Joint.currentForce;
                return force.magnitude;
            }
        }
        
        public float CurrentLoadKg
        {
            get
            {
                if (!HasCargoAttached) return 0f;
                return CurrentCargo.Weight;
            }
        }
        
        private void Awake()
        {
            SetupTriggerCollider();
            
            if (!Joint) 
                Debug.LogWarning($"Hook {name} is missing ConfigurableJoint reference!");
        }
        
        private void OnTriggerEnter(Collider collider)
        {
            collider.TryGetComponent<Cargo>(out var cargo);
            
            if (cargo && cargo != CurrentCargo && cargo.IsAttachable && !_nearbyCargoList.Contains(cargo))
                _nearbyCargoList.Add(cargo);
        }
        
        private void OnTriggerExit(Collider other)
        {
            var cargo = other.GetComponent<Cargo>();
            
            if (cargo) _nearbyCargoList.Remove(cargo);
        }
        
        private void SetupTriggerCollider()
        {
            _triggerCollider = GetComponent<SphereCollider>();
            
            if (!_triggerCollider) 
                _triggerCollider = gameObject.AddComponent<SphereCollider>();
            
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = attachmentRadius;
            
            _triggerCollider.center = Vector3.down * 0.5f; // Offset to hook point
        }
        
        public bool TryAttachCargo()
        {
            if (HasCargoAttached) return false;
            
            var nearestCargo = FindNearestCargo();
            
            return nearestCargo && AttachToSpecificCargo(nearestCargo);
        }
        
        public void TryDetachCargo()
        {
            if (!HasCargoAttached) return;
            
            var detachedCargo = CurrentCargo;
            CurrentCargo.OnDetached();
            
            if (_cargoJoint)
            {
                DestroyImmediate(_cargoJoint);
                _cargoJoint = null;
            }
            
            CurrentCargo = null;
            
            // Re-add to nearby list if still in trigger range
            if (_nearbyCargoList.Contains(detachedCargo) && detachedCargo.IsAttachable)
            {
                // Keep it in the list for potential re-attachment
            }
        }
        
        public void ToggleCargoAttachment()
        {
            if (HasCargoAttached)
                TryDetachCargo();
            else
                TryAttachCargo();
        }
        
        private bool AttachToSpecificCargo(Cargo cargo)
        {
            if (!cargo || HasCargoAttached || !cargo.IsAttachable) return false;
            
            _cargoJoint = gameObject.AddComponent<ConfigurableJoint>();
            _cargoJoint.connectedBody = cargo.Rigidbody;
            
            var localAttachPoint = cargo.Rigidbody.transform.InverseTransformPoint(cargo.AttachPoint.position);
            _cargoJoint.connectedAnchor = localAttachPoint;
            
            ConfigureCargoJoint(_cargoJoint);
            
            CurrentCargo = cargo;
            CurrentCargo.OnAttached();
            
            Debug.Log($"Hook attached to cargo: {cargo.name} (Weight: {cargo.Weight}kg)");
            return true;
        }
        
        private void ConfigureCargoJoint(ConfigurableJoint joint)
        {
            // Lock all motion except for slight swinging
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            // Allow limited angular motion for realistic swinging
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            
            // Set angular limits for swinging
            joint.lowAngularXLimit = new SoftJointLimit { limit = -15f };
            joint.highAngularXLimit = new SoftJointLimit { limit = 15f };
            joint.angularYLimit = new SoftJointLimit { limit = 15f };
            joint.angularZLimit = new SoftJointLimit { limit = 15f };
            
            // Configure joint drives
            var drive = new JointDrive
            {
                positionSpring = cargoJointSpring,
                positionDamper = cargoJointDamper,
                maximumForce = cargoJointMaxForce
            };
            
            joint.xDrive = drive;
            joint.yDrive = drive;
            joint.zDrive = drive;
            
            // Set anchor to bottom of hook
            joint.anchor = Vector3.down * 0.5f;
        }
        
        private Cargo FindNearestCargo()
        {
            if (_nearbyCargoList.Count == 0) return null;
            
            Cargo nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var cargo in _nearbyCargoList)
            {
                // Use attachment point for distance calculation
                float distance = Vector3.Distance(transform.position, cargo.AttachPoint.position);
                
                if (!(distance < nearestDistance)) continue;
                
                nearestDistance = distance;
                nearest = cargo;
            }
            
            return nearest;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw attachment range
            Gizmos.color = HasCargoAttached ? Color.green : Color.yellow;
            Vector3 triggerCenter = transform.position + Vector3.down * 0.5f;
            Gizmos.DrawWireSphere(triggerCenter, attachmentRadius);
 
            if (!HasCargoAttached || CurrentCargo == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, CurrentCargo.transform.position);
        }
    }
}