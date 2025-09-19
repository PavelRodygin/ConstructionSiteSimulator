using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Hook : MonoBehaviour
    {
        [Header("Cargo Joint Configuration")]
        [SerializeField] private float cargoJointSpring = 10000f;
        [SerializeField] private float cargoJointDamper = 1000f;
        [SerializeField] private float cargoJointMaxForce = 100000f;

        private ConfigurableJoint _cargoJoint;
        private Cargo _nearbyCargo;
        
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
            if (!Joint) 
                Debug.LogWarning($"Hook {name} is missing ConfigurableJoint reference!");
        }
        
        private void OnTriggerEnter(Collider collider)
        {
            if (_nearbyCargo != null) return; // Already have cargo in range
            
            collider.TryGetComponent<Cargo>(out var cargo);
            
            if (cargo && cargo != CurrentCargo && cargo.IsAttachable)
                _nearbyCargo = cargo;
        }
        
        private void OnTriggerExit(Collider other)
        {
            var cargo = other.GetComponent<Cargo>();
            
            if (cargo == _nearbyCargo)
                _nearbyCargo = null;
        }
        
        public bool TryAttachCargo()
        {
            if (HasCargoAttached || _nearbyCargo == null) return false;
            
            return AttachToSpecificCargo(_nearbyCargo);
        }
        
        public void TryDetachCargo()
        {
            if (!HasCargoAttached) return;
            
            CurrentCargo.OnDetached();
            
            if (_cargoJoint)
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
            
            joint.anchor = Vector3.down * 0.5f;
        }
        
        
    }
}