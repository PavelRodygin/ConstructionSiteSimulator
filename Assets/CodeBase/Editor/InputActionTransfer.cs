using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Editor
{
    public class InputActionTransfer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset sourceAsset;
        [SerializeField] private InputActionAsset targetAsset;
        [SerializeField] private string actionMapNameToCopy;

        [ContextMenu("Transfer Action Map")]
        public void TransferActionMap()
        {
            if (sourceAsset == null || targetAsset == null || string.IsNullOrEmpty(actionMapNameToCopy))
            {
                Debug.LogError("Source Asset, Target Asset, or Action Map Name is not set.");
                return;
            }

            // Find the source Action Map
            InputActionMap sourceMap = sourceAsset.FindActionMap(actionMapNameToCopy);
            if (sourceMap == null)
            {
                Debug.LogError($"Action Map '{actionMapNameToCopy}' not found in source asset.");
                return;
            }

            // Create a new Action Map in the target asset
            InputActionMap targetMap = targetAsset.AddActionMap(sourceMap.name);

            // Copy each action and its bindings
            foreach (InputAction sourceAction in sourceMap.actions)
            {
                // Add a new action to the target map
                InputAction targetAction = targetMap.AddAction(sourceAction.name, sourceAction.type, sourceAction.expectedControlType);

                // Copy bindings
                foreach (InputBinding sourceBinding in sourceAction.bindings)
                {
                    targetAction.AddBinding(sourceBinding);
                }
            }

            Debug.Log($"Transferred Action Map '{actionMapNameToCopy}' to target asset.");
        }
    }
}