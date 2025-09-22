using R3;
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

        private Rigidbody _rb;
        private ConfigurableJoint _cargoJoint;
        private Cargo _currentCargo;

        [field: SerializeField] public CraneSpecificationSO CraneSpecification { get; private set; }
        [field: SerializeField] public ConfigurableJoint WireJoint { get; private set; }
        public ReactiveProperty<float> CurrentLoad { get; } = new(0f);
        public ReactiveProperty<float> CurrentCargoMass { get; } = new(0f);
        public ReactiveProperty<float> CurrentHeight { get; } = new(0f);
        public ReactiveProperty<float> VerticalSpeed { get; } = new(0f);
        public Cargo CurrentCargo { get; private set; }
        public bool HasCargoAttached => CurrentCargo;
        
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
            if (!WireJoint) 
                Debug.LogWarning($"Hook {name} is missing ConfigurableJoint reference!");
            
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            CurrentLoad.Value = WireJoint.currentForce.magnitude;
            CurrentHeight.Value = CalculateHeight();
            VerticalSpeed.Value = _rb.linearVelocity.y;
        }
        
        private float CalculateHeight()
        {
            Vector3 anchorPos = WireJoint.connectedBody 
                ? WireJoint.connectedBody.transform.TransformPoint(WireJoint.connectedAnchor) 
                : WireJoint.connectedAnchor;
            
            return anchorPos.y - transform.position.y;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_currentCargo) return;
            
            other.TryGetComponent<Cargo>(out var cargo);
            
            if (cargo && cargo != CurrentCargo && cargo.IsAttachable)
                _currentCargo = cargo;
        }
        
        private void OnTriggerExit(Collider other)
        {
            var cargo = other.GetComponent<Cargo>();
            
            if (cargo == _currentCargo)
                _currentCargo = null;
        }
        
        public bool TryAttachCargo()
        {
            if (HasCargoAttached || !_currentCargo) return false;
            
            return AttachToSpecificCargo(_currentCargo);
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
            CurrentCargoMass.Value = 0f;
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
            CurrentCargoMass.Value = cargo.Mass;
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