# Unity-CSharp-Utilities

[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Package](https://img.shields.io/badge/UPM-dev.bxbstudio.utilities-blue)](https://github.com/BxB-Studio/Unity-CSharp-Utilities)

The Unity CSharp Utilities library provides a comprehensive collection of utility functions, extension methods, and data structures that are missing from the Unity Engine, helping developers write cleaner, more efficient code.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
- [Documentation](#documentation)
- [Core Components](#core-components)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Rich Extension Methods**: Extend Unity's built-in classes with useful methods
- **Math Utilities**: Additional math functions not available in Unity's Mathf
- **Serialization Helpers**: Easily serialize and deserialize data
- **Editor Tools**: Streamlined editor workflows and utilities
- **UI Components**: Helper methods for Unity UI components
- **Specialized Data Structures**: Custom serializable types
- **Transform Utilities**: Common transform operations made simple
- **Performance Optimizations**: Efficient alternatives to common operations
- **Cross-Platform Compatibility**: Works across all platforms Unity supports

## Installation

### Using Unity Package Manager (Recommended)

1. Open the Package Manager window in Unity (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/BxB-Studio/Unity-CSharp-Utilities.git`
5. Click "Add"

For detailed installation instructions, see the [Installation Guide](Documentation/Installation.md).

## Usage Examples

### Extension Methods

```csharp
// Find a child transform using various methods
Transform child = transform.Find("ChildName", caseSensitive: false);
Transform startsWith = transform.FindStartsWith("Child");
Transform endsWith = transform.FindEndsWith("Name");
Transform contains = transform.FindContains("ild");

// Clamp animation curves
AnimationCurve clampedCurve = originalCurve.Clamp01();

// String utilities
if (myString.IsNullOrEmpty()) {
    Debug.Log("String is null or empty");
}
```

### Data Serialization

```csharp
// Serialize and deserialize data
var dataUtil = new DataSerializationUtility<MyDataClass>("SavedData.dat", false);
var myData = new MyDataClass();

// Save data
dataUtil.SaveOrCreate(myData);

// Load data
var loadedData = dataUtil.Load();
```

### Math & Vector Operations

```csharp
// Useful math operations
float average = Utility.Average(1.0f, 2.0f, 3.0f, 4.0f);
Vector3 averagePosition = Utility.Average(position1, position2, position3);
float dist = Utility.Distance(transform1, transform2);

// Interval operations
var interval = new Interval(0f, 1f);
float lerpedValue = interval.Lerp(0.5f);
```

### Editor Utilities

```csharp
// Use in custom editors
EditorUtilities.AddScriptingDefineSymbol("MY_DEFINE");
bool hasSymbol = EditorUtilities.ScriptingDefineSymbolExists("MY_DEFINE");

// Use editor styles
GUIStyle buttonStyle = EditorUtilities.Styles.ButtonActive;
```

## Documentation

For detailed documentation, see:
- [Getting Started Guide](Documentation/GettingStarted.md)
- [API Reference](Documentation/APIReference.md)
- [Installation Guide](Documentation/Installation.md)

Each script in the library also contains comprehensive XML documentation comments that can be accessed through IntelliSense in your code editor.

## Core Components

### Utility Class
The main utility class provides hundreds of helper methods and extensions for common tasks.

### Data Serialization
`DataSerializationUtility<T>` simplifies saving and loading serializable data.

### Editor Utilities
Tools to enhance the Unity Editor workflow with custom UI styles, icons, and debugging helpers.

### Math Structures
Custom structures like `Interval`, `SimpleInterval`, `Interval2`, `Interval3` that provide mathematical operations not available in Unity.

### Serializable Types
Serializable versions of common Unity types like `SerializableVector2`, `SerializableColor`, etc.

## Contributing

We welcome contributions to this library! If you'd like to contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Developed by [BxB Studio](https://bxbstudio.dev)
