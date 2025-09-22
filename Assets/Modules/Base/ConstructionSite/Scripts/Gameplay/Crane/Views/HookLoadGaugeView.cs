using System.Globalization;
using R3;
using TMPro;
using UnityEngine;
using CodeBase.Core.UI.Widgets.ProgressBars;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views
{
    public class HookLoadGaugeView : MonoBehaviour
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
            hook.CurrentLoad
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnLoadChanged)
                .AddTo(this);
        }
        
        private async void OnLoadChanged(float currentLoad)
        {
            float normalizedProgress = Mathf.Clamp01(currentLoad / hook.CraneSpecification.MaxWireLoad);
            
            progressBar.SetDisplayValue(currentLoad.ToString("F1", CultureInfo.InvariantCulture));
            loadValueText.text = currentLoad.ToString("F1", CultureInfo.InvariantCulture);
            await progressBar.UpdateProgress(normalizedProgress);
        }
    }
}