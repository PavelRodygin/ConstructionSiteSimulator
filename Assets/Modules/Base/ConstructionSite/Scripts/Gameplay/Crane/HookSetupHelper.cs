using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    /// <summary>
    /// Helper component to automatically set up hook collider in editor
    /// </summary>
    [System.Serializable]
    public class HookSetupHelper : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private bool showSetupInfo = true;
        
        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupHookCollider();
            }
        }
        
        [ContextMenu("Setup Hook Collider")]
        public void SetupHookCollider()
        {
            var hook = GetComponent<Hook>();
            if (!hook)
            {
                if (showSetupInfo)
                    Debug.LogWarning($"No Hook component found on {name}");
                return;
            }
            
            var sphereCollider = GetComponent<SphereCollider>();
            if (!sphereCollider)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
                if (showSetupInfo)
                    Debug.Log($"Added SphereCollider to hook {name}");
            }
            
            sphereCollider.isTrigger = true;
            sphereCollider.center = Vector3.down * 0.5f;
            sphereCollider.radius = 2f; // Default attachment radius
            
            if (showSetupInfo)
                Debug.Log($"Hook {name} collider setup complete!");
        }
        
        private void OnValidate()
        {
            if (autoSetupOnAwake && Application.isPlaying)
            {
                SetupHookCollider();
            }
        }
    }
}
