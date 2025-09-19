# Construction Site Simulator

A Unity-based construction site simulation project featuring a realistic tower crane with modular, extensible architecture. The project demonstrates advanced physics simulation, reactive programming patterns, and component-based design principles.

> **Note**: This project was developed in a single day as a rapid prototype. While the core architecture and crane system are fully functional, there may be minor issues that are being actively addressed. The focus was on demonstrating the modular design and reactive programming capabilities rather than production-ready polish.

## 🏗️ Project Architecture

The project follows a modular architecture where each module is isolated and independent, represented by a `ModuleController` class that runs the main `ModuleStateMachine`. Each module uses the MVP (Model-View-Presenter) pattern, with optional state implementation using the Stateless library.

### Key Architectural Principles
- **Modular Design**: Each module is self-contained and can be developed independently
- **MVP Pattern**: Clear separation between business logic (Model), UI (View), and coordination (Presenter)
- **Dependency Injection**: Using VContainer for clean dependency management
- **Reactive Programming**: R3 library for event-driven and data flow programming

## 🏗️ Crane System - The Heart of the Project

The crane system is the centerpiece of this simulation, featuring a sophisticated physics-based implementation with realistic behavior and extensive customization options.

### Crane Hierarchy & Components

The crane consists of three main components, each with specific responsibilities:

#### 1. **RotatingBase** - The Foundation
- **Purpose**: Controls the crane's rotation around its vertical axis
- **Key Features**:
  - Realistic rotation with acceleration/deceleration curves
  - Load-dependent speed reduction (heavier cargo = slower rotation)
  - Smooth interpolation between rotation states
  - Current rotation angle tracking (normalized to 0-360°)

#### 2. **Trolley** - The Horizontal Movement
- **Purpose**: Manages horizontal movement along the crane's boom
- **Key Features**:
  - Smooth forward/backward movement with constraints
  - Hook depth control (vertical cable length)
  - Position tracking (0-1 normalized range)
  - Real-time load monitoring from attached cargo

#### 3. **Hook** - The Cargo Handler
- **Purpose**: Handles cargo attachment, physics simulation, and load management
- **Key Features**:
  - ConfigurableJoint-based physics simulation
  - Realistic cargo swinging behavior
  - Load weight calculation and monitoring
  - Automatic cargo detection and attachment

### 🔧 ConfigurableJoint Configuration

The Hook component uses Unity's `ConfigurableJoint` for realistic physics simulation. The configuration in the `CraneHook` prefab is carefully tuned for optimal behavior:

#### Motion Constraints
- **Linear Motion**: Y-axis only (vertical movement)
- **Angular Motion**: Limited rotation on all axes for realistic swinging
- **Angular Limits**: ±10° on X and Z axes, ±10° on Y axis

#### Joint Drives
- **Y-Drive** (Primary): 
  - Spring: 20,000 (strong vertical positioning)
  - Damper: 7,500 (smooth movement)
  - Max Force: 500,000 (handles heavy loads)
- **X/Z-Drives** (Stabilization):
  - Spring: 10,000 (prevents excessive swinging)
  - Damper: 500 (natural damping)
  - Max Force: 50,000 (controlled movement)

#### Angular Drives
- **Spring**: 2,500 (natural swinging motion)
- **Damper**: 1,000 (realistic damping)
- **Max Force**: Unlimited (allows natural physics)

### 🎯 Component Isolation & Extensibility

Each crane component is designed to be completely isolated and focused on its specific task:

- **RotatingBase**: Only handles rotation logic and state
- **Trolley**: Manages horizontal movement and hook positioning
- **Hook**: Deals with cargo physics and attachment

This isolation makes the system highly extensible:

#### Easy Sensor Integration
```csharp
// Each component can easily expose reactive data for sensors and instruments
public ReadOnlyReactiveProperty<float> CurrentRotationAngle => 
    _rotationAngle.ToReadOnlyReactiveProperty();
public ReadOnlyReactiveProperty<float> CurrentLoad => 
    _currentLoad.ToReadOnlyReactiveProperty();
public ReadOnlyReactiveProperty<float> HookHeight => 
    _hookHeight.ToReadOnlyReactiveProperty();
public ReadOnlyReactiveProperty<float> TrolleyDistance => 
    _trolleyDistance.ToReadOnlyReactiveProperty();

// Reactive commands for sensor updates
private readonly ReactiveCommand<SensorData> _sensorUpdateCommand = new();

// Sensors can subscribe to reactive events with filtering
_rotationAngle
    .Where(angle => Mathf.Abs(angle) > 0.1f)
    .Subscribe(angle => UpdateRotationSensor(angle));
```

#### Real-time Sensor Data Streaming
The reactive programming architecture (R3) makes it trivial to create various sensors and instruments for the operator dashboard:

```csharp
// Crane rotation sensor (degrees relative to base)
public ReadOnlyReactiveProperty<float> RotationSensor => 
    _rotationAngle.ToReadOnlyReactiveProperty();

// Load sensor (current cargo weight)
public ReadOnlyReactiveProperty<float> LoadSensor => 
    _currentLoad.ToReadOnlyReactiveProperty();

// Hook height sensor (vertical position)
public ReadOnlyReactiveProperty<float> HookHeightSensor => 
    _hookHeight.ToReadOnlyReactiveProperty();

// Trolley distance sensor (horizontal position from base)
public ReadOnlyReactiveProperty<float> TrolleyDistanceSensor => 
    _trolleyDistance.ToReadOnlyReactiveProperty();

// Reactive commands for sensor data updates
private readonly ReactiveCommand<SensorData> _sensorDataCommand = new();

// Sensors automatically update when data changes
_rotationAngle
    .DistinctUntilChanged()
    .Subscribe(angle => _sensorDataCommand.Execute(new RotationSensorData(angle)));
```

### 🚀 Reactive Programming Integration

The project leverages R3 (Reactive Extensions for Unity) for clean, event-driven programming with a focus on **reactive events and commands**. This powerful combination with modular architecture makes it incredibly easy to create various sensors and measuring instruments:

#### Easy Sensor Creation
Thanks to component decomposition and R3, you can easily create:

- **🔄 Rotation Sensor**: Shows crane rotation angle in degrees relative to base
- **⚖️ Load Sensor**: Displays current cargo weight and load status  
- **📏 Height Sensor**: Measures hook vertical position and depth
- **📐 Distance Sensor**: Shows trolley horizontal position from crane base

#### Reactive Events & Commands
- **ReactiveCommand**: Used for sensor data updates and UI interactions
- **ReactiveProperty**: Real-time sensor data with automatic change notifications
- **Subject/Observable**: Event publishing and subscription patterns for sensor streams
- **CompositeDisposable**: Proper resource management for reactive subscriptions

#### Key Reactive Patterns for Sensors
```csharp
// Reactive commands for sensor data
private readonly ReactiveCommand<SensorData> _sensorUpdateCommand = new();
private readonly ReactiveCommand<LoadAlert> _loadAlertCommand = new();

// Reactive properties for real-time sensor readings
private readonly ReactiveProperty<float> _rotationAngle = new(0f);
private readonly ReactiveProperty<float> _currentLoad = new(0f);
private readonly ReactiveProperty<float> _hookHeight = new(0f);

// Event publishing with Subjects
private readonly Subject<CraneTelemetry> _telemetrySubject = new();
```

#### Sensor Data Streaming
- **Real-time Updates**: Sensors automatically update when component data changes
- **Data Filtering**: Use reactive operators to filter and process sensor data
- **Throttling**: Prevent excessive sensor updates for performance
- **Clean Disposal**: All sensor subscriptions are properly managed

### 🎮 Control System

The crane features a sophisticated control system with:
- **Smooth Input Handling**: State-based input with proper acceleration/deceleration
- **Load-Dependent Behavior**: Heavier loads reduce maximum speed
- **Safety Constraints**: Movement limits and load capacity checks
- **Real-time Feedback**: Immediate response to operator input

### 🔧 Configuration System

The crane behavior is controlled through `CraneSpecificationSO` ScriptableObjects:

- **Rotation Settings**: Speed, acceleration, deceleration curves
- **Load Capacity**: Rated and maximum cargo weights
- **Performance**: Speed reduction based on load
- **Movement**: Trolley and hook speed settings

### 🎯 Future Extensibility

The modular design makes it easy to add:

1. **Sensor Systems**: Various measuring instruments (rotation, load, height, distance sensors)
2. **Sound Systems**: Audio sources can be easily placed on trolley, rotation mechanism, or hook
3. **Visual Effects**: Particle systems, animations, and visual feedback
4. **Dashboard Integration**: Real-time sensor data streaming to operator interfaces
5. **Telemetry**: Data logging and analysis systems
6. **AI Integration**: Automated crane operation based on sensor data

### 🛠️ Technical Highlights

- **Physics-Based Simulation**: Realistic cargo behavior using Unity's physics system
- **Performance Optimized**: Efficient update loops and state management
- **Memory Safe**: Proper disposal patterns and resource management
- **Testable**: Clean separation of concerns enables easy unit testing
- **Scalable**: Component-based architecture supports complex crane configurations

## 🚀 Getting Started

1. Open the project in Unity 2022.3 or later
2. Navigate to the ConstructionSite scene
3. Use the crane controls to operate the simulation
4. Experiment with different cargo weights and observe realistic physics behavior

## 📁 Project Structure

```
Assets/
├── Modules/Base/ConstructionSite/
│   ├── Scripts/Gameplay/Crane/
│   │   ├── Crane.cs                 # Main crane controller
│   │   ├── RotatingBase.cs         # Rotation component
│   │   ├── Trolley.cs              # Horizontal movement
│   │   ├── Hook.cs                 # Cargo handling
│   │   └── CraneSpecificationSO.cs # Configuration
│   └── Prefabs/
│       ├── Crane.prefab            # Main crane assembly
│       └── CraneHook.prefab        # Hook with ConfigurableJoint
└── CodeBase/
    ├── Core/Infrastructure/        # Module system
    └── Implementation/             # Concrete implementations
```

This project demonstrates how proper architecture, component isolation, and reactive programming can create a highly maintainable and extensible simulation system. The crane implementation serves as an excellent example of how to build complex, physics-based systems in Unity while maintaining clean, testable code.

## 🚀 Development Timeline

This project was developed as a **24-hour rapid prototype** to demonstrate:
- Modular architecture principles
- Reactive programming with R3
- Physics-based crane simulation
- Component isolation and extensibility

While the core functionality is complete and working, some refinements and optimizations are ongoing. The focus was on showcasing the architectural approach and extensibility potential rather than production-ready polish.