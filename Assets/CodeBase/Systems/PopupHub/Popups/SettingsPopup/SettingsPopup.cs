using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using CodeBase.Core.Systems.PopupHub.Popups;
using CodeBase.Services;
using CodeBase.Services.Input;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace CodeBase.Systems.PopupHub.Popups.SettingsPopup
{
    public class SettingsPopup : BasePopup
    { 
        [Header("Volume Sliders")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundVolumeSlider;
        [SerializeField] private TMP_Text musicVolumeText;
        [SerializeField] private TMP_Text soundVolumeText;
        // [SerializeField] private Button rebindPopupButton;
        
        [Inject] private AudioSystem _audioSystem;
        [Inject] private InputSystemService _inputSystemService;
        [Inject] private IObjectResolver _objectResolver;
        
        private IPopupHub _popupHub;
        
        private readonly ReactiveCommand<Unit> _rebindPopupCommand = new();

        private void Start()
        {
            SetInitialSettings();
            SetupEventListeners();
            _rebindPopupCommand.Subscribe(_ => OnOpenRebindPopupButtonClicked());
            // rebindPopupButton.OnClickAsObservable()
            //     .Subscribe(_ => _rebindPopupCommand.Execute(default))
            //     .AddTo(this);
        }

        public async void OnEscapePressed(InputAction.CallbackContext callbackContext) => await Close();

        private void SetInitialSettings()
        {
            musicVolumeSlider.value = _audioSystem.MusicVolume;
            musicVolumeText.text = ((int)(_audioSystem.MusicVolume * 100)).ToString();
            soundVolumeSlider.value = _audioSystem.SoundsVolume;
            soundVolumeText.text = ((int)(_audioSystem.SoundsVolume * 100)).ToString();
        }

        private void SetupEventListeners()
        {
            musicVolumeSlider.onValueChanged.AddListener(v => _audioSystem.SetMusicVolume(v));
            musicVolumeSlider.onValueChanged.
                AddListener(v => musicVolumeText.text = ((int)(v * 100)).ToString());
            soundVolumeSlider.onValueChanged.AddListener(v => _audioSystem.SetSoundsVolume(v));
            soundVolumeSlider.onValueChanged.
                AddListener(v => soundVolumeText.text = ((int)(v * 100)).ToString());
            
            _inputSystemService.InputActions.UI.Cancel.performed -= OnEscapePressed;
        }

        private void RemoveEventListeners()
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            soundVolumeSlider.onValueChanged.RemoveAllListeners();
            _inputSystemService.InputActions.UI.Cancel.performed -= OnEscapePressed;
        }

        private void OnOpenRebindPopupButtonClicked()
        {
            if (_popupHub == null)
                _popupHub = _objectResolver.Resolve<IPopupHub>();
        }
    }
}