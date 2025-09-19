using System.Collections.Generic;
using Modules.Base.ThirdPersonMPModule.Scripts.Gameplay;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hook : MonoBehaviour
    {
        [Header("Hook Configuration")]
        [SerializeField] private float attachmentDistance = 2f;
        [SerializeField] private LayerMask cargoLayerMask = -1;
        
        [Header("Joint Configuration")]
        [SerializeField] private float cargoJointSpring = 10000f;
        [SerializeField] private float cargoJointDamper = 1000f;
        [SerializeField] private float cargoJointMaxForce = 100000f;

        private ConfigurableJoint _cargoJoint;
        private List<Cargo> _nearbyCargoList = new();
        private Rigidbody _rigidbody;
        
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
            _rigidbody = GetComponent<Rigidbody>();
            
            if (!Joint) 
                Debug.LogWarning($"Hook {name} is missing ConfigurableJoint reference!");
        }
        
        private void Update()
        {
            FindNearbyCargoObjects();
        }
        
        /// <summary>
        /// Attempts to attach to the nearest cargo object
        /// </summary>
        public bool AttachCargo()
        {
            if (HasCargoAttached) return false;
            
            Cargo nearestCargo = FindNearestCargo();
            if (nearestCargo == null) return false;
            
            return AttachToSpecificCargo(nearestCargo);
        }
        
        public void TryDetachCargo()
        {
            if (!HasCargoAttached) return;
            
            CurrentCargo.OnDetached();
            
            if (_cargoJoint != null)
            {
                DestroyImmediate(_cargoJoint);
                _cargoJoint = null;
            }
            
            CurrentCargo = null;
        }
        
        public void ToggleCargoAttachment()
        {
            if (HasCargoAttached)
                TryDetachCargo();
            else
                AttachCargo();
        }
        
        private bool AttachToSpecificCargo(Cargo cargo)
        {
            if (!cargo || HasCargoAttached || !cargo.IsAttachable) return false;
            
            _cargoJoint = gameObject.AddComponent<ConfigurableJoint>();
            _cargoJoint.connectedBody = cargo.Rigidbody;
            
            Vector3 localAttachPoint = cargo.Rigidbody.transform.InverseTransformPoint(cargo.AttachPoint.position);
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
        
        private void FindNearbyCargoObjects()
        {
            _nearbyCargoList.Clear();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, attachmentDistance, cargoLayerMask);
            
            foreach (var collider in colliders)
            {
                var cargo = collider.GetComponent<Cargo>();
                
                if (cargo && cargo != CurrentCargo && cargo.IsAttachable) _nearbyCargoList.Add(cargo);
            }
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
            Gizmos.DrawWireSphere(transform.position, attachmentDistance);

            if (!HasCargoAttached || CurrentCargo == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, CurrentCargo.transform.position);
        }
    }
}