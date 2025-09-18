using System.Threading;
using CodeBase.Core.UI.Views;
using CodeBase.Core.UI.Views.Animations;
using CodeBase.Core.UI.Widgets.ProgressBars;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.Bootstrap.Scripts
{
    public class BootstrapView : BaseView
    {
        [Header("Progress Bar")]
        [SerializeField] private BaseProgressBar progressBar;
        
        [Header("UI Components")]
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text progressStatusText;
        [SerializeField] private TMP_Text tooltipsText;
        [SerializeField] private TMP_Text versionText;
        
        [Header("Visual Effects")]
        [SerializeField] private Image progressIndicatorImage;
        [SerializeField] private Image flashingImage;

        // private FlickerAnimation _flickerAnimation; // Removed to prevent CanvasGroup blinking
        private Sequence _loadCompletedSequence;

        private const string TapToContinueText = "Tap to continue";
        private const float ProgressBarAnimDuration = 0.5f;

        
        private void Start()
        {
            tooltipsText.transform.parent.gameObject.SetActive(true);
            // Removed FlickerAnimation initialization to prevent CanvasGroup blinking
            // _flickerAnimation = new FlickerAnimation(CanvasGroup, flashingImage);
        }

        public void SetupEventListeners(ReactiveCommand<Unit> startCommand,
            ReadOnlyReactiveProperty<string> progressStatus,
            ReadOnlyReactiveProperty<float> exponentialProgress)
        {
            continueButton.OnClickAsObservable()
                .Subscribe(_ => startCommand.Execute(default))
                .AddTo(this);

            Observable.CombineLatest(exponentialProgress, progressStatus,
                    (progress, status) => new { progress, status })
                .Subscribe(data => ReportProgress(data.progress, data.status).Forget())
                .AddTo(this);
        }
        
        public void SetVersionText(string version) => versionText.text = version;

        public void SetTooltipText(string text) => tooltipsText.text = text;

        public void ShowAnimations(CancellationToken cancellationToken)
        {
            // Set status text and animate it
            if (progressStatusText)
            {
                progressStatusText.text = TapToContinueText;
                
                _loadCompletedSequence = DOTween.Sequence();
                _loadCompletedSequence.Append(progressStatusText.transform.DOScale(1.2f, ProgressBarAnimDuration))
                         .SetLoops(-1, LoopType.Yoyo);
            }

            // Removed flickering animation to prevent CanvasGroup blinking
            // _flickerAnimation.StartFlickering(cancellationToken).Forget();
        }

        public override UniTask Show()
        {
            base.Show().Forget();
            SetActive(true);
            
            // Initialize progress bar
            if (progressBar) 
                progressBar.ResetProgress();
            
            // Reset visual effects
            UpdateVisualEffects(0f);
            
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            StopAnimation();
            base.Dispose();
        }

        private void StopAnimation()
        {
            if (_loadCompletedSequence == null || !_loadCompletedSequence.IsActive()) return;
            
            _loadCompletedSequence.Kill();
            _loadCompletedSequence = null;
        }

        private void UpdateVisualEffects(float expProgress)
        {
            // Update overlay alpha (fade out as progress increases)
            // if (flashingImage) 
            //     flashingImage.color = new Color(0, 0, 0, 1 - expProgress);

            // Keep CanvasGroup alpha fixed to prevent blinking
            if (CanvasGroup) 
                CanvasGroup.alpha = 1f;

            // Update stuff image alpha (fade in as progress increases, with minimum value)
            if (progressIndicatorImage) 
                progressIndicatorImage.color = new Color(1, 1, 1, Mathf.Max(0.1f, expProgress));
        }
        
        private async UniTask ReportProgress(float expProgress, string progressStatus)
        {
            // Update progress bar
            if (progressBar) 
                await progressBar.UpdateProgress(expProgress);
            
            // Handle status text separately in BootstrapView
            if (progressStatusText) 
                progressStatusText.text = progressStatus;
            
            // Update visual effects separately (BootstrapView responsibility)
            UpdateVisualEffects(expProgress);
        }
    }
}
