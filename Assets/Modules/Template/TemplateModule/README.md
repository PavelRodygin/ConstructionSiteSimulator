# Template Module

A foundational template module for the ShrimpOlympus project that demonstrates the MVP (Model-View-Presenter) architecture pattern and provides a solid foundation for creating new modules.

## Overview

The Template module serves as a starting point for developers to create new modules. It implements the MVP architectural pattern and includes basic UI navigation, sound management, and module lifecycle handling.

## Architecture

The module follows the MVP (Model-View-Presenter) pattern:

- **Model** (`TemplateScreenModel`): Contains business logic and data
- **View** (`TemplateScreenView`): Handles UI interactions and visual representation  
- **Presenter** (`TemplateScreenPresenter`): Coordinates between Model and View
- **Controller** (`TemplateModuleController`): Manages module lifecycle

## Features

- Main menu navigation
- Settings popup support
- Sound toggle functionality
- Keyboard navigation support (ESC key for main menu)
- Basic command throttling

## File Structure

```
TemplateModule/
├── Scripts/
│   ├── TemplateScreenModel.cs      # Business logic and data
│   ├── TemplateScreenView.cs       # UI interactions
│   ├── TemplateScreenPresenter.cs  # Business logic coordination
│   ├── TemplateModuleController.cs # Module lifecycle management
│   └── TemplateScreenInstaller.cs  # Dependency injection setup
├── Views/
│   └── TemplateScreenView.prefab   # UI prefab
├── Scenes/
│   └── TemplateScreen.unity        # Module scene
└── README.md                       # This file
```

## Usage

1. Copy the TemplateModule folder to your Modules directory
2. Rename all files and classes according to your module name
3. Update namespaces to match your module location
4. Customize UI elements and business logic
5. Update the installer for your specific dependencies

## Dependencies

- VContainer (Dependency Injection)
- R3 (Reactive Programming)
- UniTask (Async Operations)
- MediatR (Request/Response Pattern)

## Best Practices

- Follow the MVP pattern structure
- Use dependency injection for services
- Implement proper disposal patterns
- Add logging for debugging
- Validate UI element assignments

## License

This template is part of the ShrimpOlympus project and follows the same licensing terms.
