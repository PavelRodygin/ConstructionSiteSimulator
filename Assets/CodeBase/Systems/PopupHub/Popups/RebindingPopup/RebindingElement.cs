using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CodeBase.Systems.PopupHub.Popups.RebindingPopup
{[Serializable]
    public class UpdateBindingUIEvent : UnityEvent<RebindingElement, string, string, string> { }
    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<RebindingElement, InputActionRebindingExtensions.RebindingOperation> { }
    public class RebindingElement : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button rebindButton;
        [SerializeField] private Button resetButton;
        [Tooltip("Reference to action that is to be rebound from the UI")]
        [SerializeField] private InputActionReference actionReference;

        [Tooltip("Text label that will receive the name of the action")]
        [SerializeField] private TextMeshPro actionLabel;

        [Tooltip("Text label that will be updated with prompt for user input")]
        [SerializeField] private TextMeshPro rebindText;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField] private TextMeshPro bindingText;
        
        private InputAction _action;
        private InputSystem_Actions _actionAsset;
        
        
        private void OnEnable()
        {
            UpdateBindingText();
            _actionAsset = new InputSystem_Actions();
            
            
        }

        private void UpdateBindingText() => bindingText.text = actionReference.action.bindings.ToString();

        // [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
        //          + "bindings in custom ways, e.g. using images instead of text.")]
        // [SerializeField] private UpdateBindingUIEvent updateBindingUIEvent;
        //
        // [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
        //          + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
        //          + "customize the rebind.")]
        // [SerializeField] private InteractiveRebindEvent rebindStartEvent;
        //
        // [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        // [SerializeField]
        // private InteractiveRebindEvent rebindStopEvent;
        //
        // [SerializeField] private string bindingIdString;
        //
        // [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

        //private static List<RebindingElement> _rebindActionUIs;
        
        // protected void OnEnable()
        // {
        //     _rebindActionUIs ??= new List<RebindingElement>();
        //     _rebindActionUIs.Add(this);
        //     
        //     if (_rebindActionUIs.Count == 1)
        //         UnityEngine.InputSystem.InputSystem.onActionChange += OnActionChange;
        //     
        //     rebindButton.OnClickAsObservable().Subscribe(_ => StartInteractiveRebind()).AddTo(this);
        //     resetButton.OnClickAsObservable().Subscribe(_ => ResetToDefault()).AddTo(this);
        // }
        //
        // private static void OnActionChange(object obj, InputActionChange change)
        // {
        //     if (change != InputActionChange.BoundControlsChanged)
        //         return;
        //
        //     var action = obj as InputAction;
        //     var actionMap = action?.actionMap ?? obj as InputActionMap;
        //     var actionAsset = actionMap?.asset ?? obj as InputActionAsset;
        //
        //     for (var i = 0; i < _rebindActionUIs.Count; ++i)
        //     {
        //         var component = _rebindActionUIs[i];
        //         var referencedAction = component.actionReference?.action;
        //         if (referencedAction == null)
        //             continue;
        //
        //         if (referencedAction == action ||
        //             referencedAction.actionMap == actionMap ||
        //             referencedAction.actionMap?.asset == actionAsset)
        //             component.UpdateBindingDisplay();
        //     }
        // }
        //
        // public void StartInteractiveRebind()
        // {
        //     if (!ResolveActionAndBinding(out var action, out var bindingIndex))
        //         return;
        //
        //     // If the binding is a composite, we need to rebind each part in turn.
        //     if (action.bindings[bindingIndex].isComposite)
        //     {
        //         var firstPartIndex = bindingIndex + 1;
        //         if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
        //             PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
        //     }
        //     else
        //     {
        //         PerformInteractiveRebind(action, bindingIndex);
        //     }
        // }
        //
        // private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        // {
        //     _rebindOperation?.Cancel(); // Will null out m_RebindOperation.
        //
        //     void CleanUp()
        //     {
        //         _rebindOperation?.Dispose();
        //         _rebindOperation = null;
        //         action.Enable();
        //     }
        //
        //     //Fixes the "InvalidOperationException: Cannot rebind action x while it is enabled" error
        //     action.Disable();
        //
        //     // Configure the rebind.
        //     _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
        //         .OnCancel(
        //             operation =>
        //             {
        //                 rebindStopEvent?.Invoke(this, operation);
        //                 if (m_RebindOverlay != null)
        //                     m_RebindOverlay.SetActive(false);
        //                 UpdateBindingDisplay();
        //                 CleanUp();
        //             })
        //         .OnComplete(
        //             operation =>
        //             {
        //                 if (m_RebindOverlay != null)
        //                     m_RebindOverlay.SetActive(false);
        //                 rebindStopEvent?.Invoke(this, operation);
        //                 UpdateBindingDisplay();
        //                 CleanUp();
        //
        //                 // If there's more composite parts we should bind, initiate a rebind
        //                 // for the next part.
        //                 if (allCompositeParts)
        //                 {
        //                     var nextBindingIndex = bindingIndex + 1;
        //                     if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
        //                         PerformInteractiveRebind(action, nextBindingIndex, true);
        //                 }
        //             });
        //
        //     // If it's a part binding, show the name of the part in the UI.
        //     var partName = default(string);
        //     if (action.bindings[bindingIndex].isPartOfComposite)
        //         partName = $"Binding '{action.bindings[bindingIndex].name}'. ";
        //
        //     // Bring up rebind overlay, if we have one.
        //     //m_RebindOverlay?.SetActive(true);
        //     if (rebindText != null)
        //     {
        //         var text = !string.IsNullOrEmpty(_rebindOperation.expectedControlType)
        //             ? $"{partName}Waiting for {_rebindOperation.expectedControlType} input..."
        //             : $"{partName}Waiting for input...";
        //         rebindText.text = text;
        //     }
        //
        //     // If we have no rebind overlay and no callback but we have a binding text label,
        //     // temporarily set the binding text label to "<Waiting>".
        //     if (m_RebindOverlay == null && rebindText == null && rebindStartEvent == null && bindingText != null)
        //         bindingText.text = "<Waiting...>";
        //
        //     // Give listeners a chance to act on the rebind starting.
        //     rebindStartEvent?.Invoke(this, _rebindOperation);
        //
        //     _rebindOperation.Start();
        // }
        //
        // private bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        // {
        //     bindingIndex = -1;
        //
        //     action = actionReference?.action;
        //     if (action == null)
        //         return false;
        //
        //     if (string.IsNullOrEmpty(bindingIdString))
        //         return false;
        //
        //     // Look up binding index.
        //     var bindingId = new Guid(bindingIdString);
        //     bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
        //     if (bindingIndex == -1)
        //     {
        //         Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
        //         return false;
        //     }
        //
        //     return true;
        // }
        //
        // public void ResetToDefault()
        // {
        //     if (!ResolveActionAndBinding(out var action, out var bindingIndex))
        //         return;
        //
        //     if (action.bindings[bindingIndex].isComposite)
        //     {
        //         // It's a composite. Remove overrides from part bindings.
        //         for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
        //             action.RemoveBindingOverride(i);
        //     }
        //     else
        //     {
        //         action.RemoveBindingOverride(bindingIndex);
        //     }
        //     UpdateBindingDisplay();
        // }
        //
        // public void UpdateBindingDisplay()
        // {
        //     var displayString = string.Empty;
        //     var deviceLayoutName = default(string);
        //     var controlPath = default(string);
        //
        //     // Get display string from action.
        //     var action = actionReference?.action;
        //     if (action != null)
        //     {
        //         var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == bindingIdString);
        //         if (bindingIndex != -1)
        //             displayString = action.GetBindingDisplayString(bindingIndex,
        //                 out deviceLayoutName, out controlPath, displayStringOptions);
        //     }
        //
        //     // Set on label (if any).
        //     if (bindingText != null)
        //         bindingText.text = displayString;
        //
        //     // Give listeners a chance to configure UI in response.
        //     updateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        // }
        //
        // protected void OnDisable()
        // {
        //     _rebindOperation?.Dispose();
        //     _rebindOperation = null;
        //
        //     _rebindActionUIs.Remove(this);
        //     if (_rebindActionUIs.Count == 0)
        //     {
        //         _rebindActionUIs = null;
        //         UnityEngine.InputSystem.InputSystem.onActionChange -= OnActionChange;
        //     }
        // }
    }
}