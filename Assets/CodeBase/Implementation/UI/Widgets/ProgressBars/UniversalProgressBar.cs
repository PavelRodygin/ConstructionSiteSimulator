using CodeBase.Core.UI.Widgets.ProgressBars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Implementation.UI.Widgets.ProgressBars
{
    public class UniversalProgressBar : BaseProgressBar
    {
        [SerializeField] private TMP_Text progressValueText;
        [SerializeField] private Image progressBarFillImage;
        
        private float _currentProgress = 0f;
        
        protected override void UpdateProgressVisual(float progress)
        {
            _currentProgress = progress;
            var exponentialProgress = CalculateExponentialProgress(progress);
            
            if (progressBarFillImage != null)
            {
                progressBarFillImage.fillAmount = exponentialProgress;
            }

            if (progressValueText != null)
            {
                var percentage = (int)(exponentialProgress * 100);
                progressValueText.text = $"{percentage}%";
                // Debug log to verify text updates
                Debug.Log($"[UniversalProgressBar] Progress: {progress:F2} -> Exponential: {exponentialProgress:F2} -> Text: {percentage}%");
            }
            else
            {
                Debug.LogWarning("[UniversalProgressBar] progressValueText is null! Check Unity Inspector assignment.");
            }
        }
        
        protected override float GetCurrentProgress() => _currentProgress;

        public override void SetProgress(float value)
        {
            _currentProgress = value;
            var exponentialProgress = CalculateExponentialProgress(value);
            if (progressBarFillImage) progressBarFillImage.fillAmount = exponentialProgress;
            if (progressValueText) progressValueText.text = $"{(int)(exponentialProgress * 100)}%";
        }

        public override void SetDisplayValue(string text)
        {
            if (progressValueText) progressValueText.text = text;
        }
    }
}