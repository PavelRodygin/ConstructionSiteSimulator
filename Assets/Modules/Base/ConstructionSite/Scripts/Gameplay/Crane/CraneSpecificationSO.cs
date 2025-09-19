using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    [CreateAssetMenu(fileName = "CraneSpecification", menuName = "ConstructionSite/Crane Specification")]
    public class CraneSpecificationSO : ScriptableObject
    {
        [Header("Rotation Settings")]
        [field: SerializeField] public float BaseRotationSpeed { get; private set; } = 30f; // degrees per second
        [field: SerializeField] public float MinRotationSpeed { get; private set; } = 10f;
        [field: SerializeField] public float RotationAcceleration { get; private set; } = 60f; // degrees per second squared
        [field: SerializeField] public float RotationDeceleration { get; private set; } = 90f; // degrees per second squared
        
        [Header("Load Capacity")]
        [field: SerializeField] public float RatedCargoWeight { get; private set; } = 10000f;
        [field: SerializeField] public float MaxCargoWeight { get; private set; } = 40000f;
        
        [Header("Crane Performance")]
        [field: SerializeField, Range(0f, 1f)] public float MaxSpeedReduction { get; private set; } = 0.75f;
        
        [Header("Trolley Settings")]
        [field: SerializeField] public float TrolleyMoveSpeed { get; private set; } = 5f; // m/s
        [field: SerializeField] public float TrolleyMaxDistance { get; private set; } = 20f; // meters
        
        [Header("Hook Settings")]
        [field: SerializeField] public float HookMoveSpeed { get; private set; } = 3f; // m/s
        [field: SerializeField] public float HookMaxDepth { get; private set; } = 15f; // meters
    }
}
