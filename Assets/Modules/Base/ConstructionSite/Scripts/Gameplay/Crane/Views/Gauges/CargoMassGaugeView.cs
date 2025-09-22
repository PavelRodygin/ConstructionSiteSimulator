using System.Globalization;
using R3;
using TMPro;
using UnityEngine;
using CodeBase.Core.UI.Widgets.ProgressBars;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views
{
    public class CargoMassGaugeView : MonoBehaviour
    {
        [SerializeField] private BaseProgressBar progressBar;
        [SerializeField] private TMP_Text loadValueText;
        [SerializeField] private Hook hook;
        
        private void Start()
        {
            if (!hook)
            {
                Debug.LogError("Hook reference is null in HookLoadGaugeView");
                return;
            }
            
            if (!progressBar)
            {
                Debug.LogError("ProgressBar reference is null in HookLoadGaugeView");
                return;
            }
            
            if (!loadValueText)
            {
                Debug.LogError("LoadValueText reference is null in HookLoadGaugeView");
                return;
            }
            
            SetupReactiveSubscriptions();
        }
        
        private void SetupReactiveSubscriptions()
        {
            hook.CurrentCargoMass
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnMassChanged)
                .AddTo(this);
        }
        
        private async void OnMassChanged(float currentMass)
        {
            float maxMass = hook.CraneSpecification.MaxWireLoad / Physics.gravity.magnitude;
            float normalizedProgress = Mathf.Clamp01(currentMass / maxMass);
            
            progressBar.SetDisplayValue(currentMass.ToString("F1", CultureInfo.InvariantCulture));
            loadValueText.text = currentMass.ToString("F1", CultureInfo.InvariantCulture);
            await progressBar.UpdateProgress(normalizedProgress);
        }
    }
}