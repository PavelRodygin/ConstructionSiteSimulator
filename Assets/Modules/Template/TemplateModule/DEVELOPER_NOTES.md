# Developer Notes for Template Module

This document provides detailed guidance for developers using the Template module as a foundation for new modules.

## Quick Start

1. Copy the `TemplateModule` folder
2. Rename it to your module name (e.g., `MyNewModule`)
3. Update all class names and namespaces
4. Customize UI elements and business logic
5. Update the installer for your dependencies

## Class Renaming Guide

### 1. Update Class Names

- `TemplateScreenModel` → `MyNewModuleModel`
- `TemplateScreenView` → `MyNewModuleView`
- `TemplateScreenPresenter` → `MyNewModulePresenter`
- `TemplateModuleController` → `MyNewModuleController`
- `TemplateScreenInstaller` → `MyNewModuleInstaller`

### 2. Update Namespaces

Change all namespace references from:
```csharp
namespace Modules.Template.TemplateScreen.Scripts
```

To:
```csharp
namespace Modules.MyNewModule.MyNewModuleScreen.Scripts
```

## Code Examples

### Model Example

```csharp
public class MyNewModuleModel : IModel
{
    public int CommandThrottleDelay => 300;
    
    public void Dispose() { }
}
```

### View Example

```csharp
public class MyNewModuleView : BaseView
{
    [Header("Navigation")]
    [SerializeField] private Button mainMenuButton;
    
    [Header("UI Elements")]
    [SerializeField] private Button settingsPopupButton;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private TMP_Text titleText;
    
    protected override void Awake()
    {
        base.Awake();
        
        #if UNITY_EDITOR
        ValidateUIElements();
        #endif
    }
    
    private void ValidateUIElements()
    {
        if (mainMenuButton == null) Debug.LogError($"{nameof(mainMenuButton)} is not assigned");
        if (settingsPopupButton == null) Debug.LogError($"{nameof(settingsPopupButton)} is not assigned");
        if (musicToggle == null) Debug.LogError($"{nameof(musicToggle)} is not assigned");
        if (titleText == null) Debug.LogError($"{nameof(titleText)} is not assigned");
    }
}
```

### Commands Structure Example

```csharp
public readonly struct MyNewModuleCommands
{
    public readonly ReactiveCommand<Unit> OpenMainMenuCommand;
    public readonly ReactiveCommand<Unit> SettingsPopupCommand;
    public readonly ReactiveCommand<bool> SoundToggleCommand;

    public MyNewModuleCommands(
        ReactiveCommand<Unit> openMainMenuCommand,
        ReactiveCommand<Unit> settingsPopupCommand,
        ReactiveCommand<bool> soundToggleCommand)
    {
        OpenMainMenuCommand = openMainMenuCommand;
        SettingsPopupCommand = settingsPopupCommand;
        SoundToggleCommand = soundToggleCommand;
    }
}
```

### Presenter Example

```csharp
public class MyNewModulePresenter : IDisposable
{
    private readonly MyNewModuleModel _model;
    private readonly MyNewModuleView _view;
    private readonly IAudioSystem _audioSystem;
    private readonly IPopupHub _popupHub;
    
    private readonly CompositeDisposable _disposables = new();
    private ReactiveCommand<ModulesMap> _openNewModuleCommand;
    private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
    private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
    private readonly ReactiveCommand<bool> _toggleSoundCommand = new();
    
    public MyNewModulePresenter(
        MyNewModuleModel model,
        MyNewModuleView view,
        IAudioSystem audioSystem,
        IPopupHub popupHub)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
        _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));
    }
    
    public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
    {
        _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
        
        _view.HideInstantly();

        var commands = new MyNewModuleCommands(
            _openMainMenuCommand,
            _settingsPopupCommand,
            _toggleSoundCommand
        );

        _view.SetupEventListeners(commands);
        await _view.Show();
    }
    
    public async UniTask Exit()
    {
        await _view.Hide();
    }
    
    public void HideInstantly() => _view.HideInstantly();
    
    public void Dispose()
    {
        _disposables.Dispose();
    }
    
    private void SubscribeToUIUpdates()
    {
        _openMainMenuCommand
            .ThrottleFirst(TimeSpan.FromMilliseconds(_model.CommandThrottleDelay))
            .Subscribe(_ => OnMainMenuButtonClicked())
            .AddTo(_disposables);

        _settingsPopupCommand
            .ThrottleFirst(TimeSpan.FromMilliseconds(_model.CommandThrottleDelay))
            .Subscribe(_ => OnSettingsPopupButtonClicked())
            .AddTo(_disposables);

        _toggleSoundCommand
            .ThrottleFirst(TimeSpan.FromMilliseconds(_model.CommandThrottleDelay))
            .Subscribe(OnSoundToggled)
            .AddTo(_disposables);
    }
    
    private void OnMainMenuButtonClicked()
    {
        _openNewModuleCommand.Execute(ModulesMap.MainMenu);
    }
    
    private void OnSettingsPopupButtonClicked()
    {
        _popupHub.ShowSettingsPopup();
    }
    
    private void OnSoundToggled(bool isOn)
    {
        if (isOn)
        {
            _audioSystem.SetMusicVolume(1f);
        }
        else
        {
            _audioSystem.SetMusicVolume(0f);
        }
    }
}
```

### Module Controller Example

```csharp
public class MyNewModuleController : IModuleController
{
    private readonly MyNewModuleModel _model;
    private readonly MyNewModulePresenter _presenter;
    
    public MyNewModuleController(MyNewModuleModel model, MyNewModulePresenter presenter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
    }
    
    public async UniTask Enter(object param)
    {
        await _presenter.Enter();
    }
    
    public async UniTask Execute() => await UniTask.CompletedTask;
    
    public async UniTask Exit()
    {
        await _presenter.Exit();
    }
    
    public void Dispose()
    {
        _presenter.Dispose();
        _model.Dispose();
    }
}
```

## Best Practices

### 1. Error Handling
- Always validate dependencies in constructors
- Use try-catch blocks for async operations
- Log errors with module prefixes for easy identification

### 2. UI Validation
- Validate UI elements in Awake() method
- Use descriptive error messages
- Check for null references before use

### 3. Logging
- Use consistent module prefixes in all log messages
- Enable debug logging for development
- Log important state changes and errors

### 4. Performance
- Use throttling for rapid user interactions
- Implement proper disposal patterns
- Avoid memory leaks with reactive subscriptions

## Troubleshooting

### Common Issues

1. **Missing UI Elements**: Check that all UI elements are assigned in the Inspector
2. **Dependency Injection Errors**: Verify that all dependencies are registered in the installer
3. **Compilation Errors**: Ensure all required assemblies are referenced in the asmdef file

### Debug Tips

- Use the validation methods in the View for UI element checking
- Check the console for error messages with module prefixes
- Verify that the installer is properly configured
- Enable debug logging in the Model for detailed operation tracking

## Migration from Previous Versions

### New Features
- Simplified MVP architecture
- Basic error handling and validation
- Consistent logging patterns
- UI element validation

### Breaking Changes
- View now requires command structure
- Presenter handles business logic
- Controller focuses on module lifecycle

### Migration Steps
1. Update class names and namespaces
2. Implement basic validation methods
3. Add error handling where needed
4. Test all functionality after migration
