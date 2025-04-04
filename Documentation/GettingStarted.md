# Getting Started with Unity-CSharp-Utilities

This guide will help you get started with the Unity-CSharp-Utilities package and make the most of its features.

## Installation

### Using Unity Package Manager (Recommended)

1. Open the Package Manager window in Unity (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/BxB-Studio/Unity-CSharp-Utilities.git`
5. Click "Add"

### Alternative Installation (Manual)

1. Download the latest release from the [releases page](https://github.com/BxB-Studio/Unity-CSharp-Utilities/releases)
2. Extract the contents into your project's Assets folder
3. Or use as an embedded package by extracting to `Packages/dev.bxbstudio.utilities`

## Importing the Namespace

To use the utilities in your scripts, add the following using directive:

```csharp
using Utilities;
```

For editor utilities, also add:

```csharp
using Utilities.Editor;
```

## Basic Usage Examples

### Transform Extensions

```csharp
// Find a child by name (case insensitive)
Transform child = transform.Find("ChildName", caseSensitive: false);

// Find a child with a name that starts with, ends with, or contains a string
Transform childStartsWith = transform.FindStartsWith("Child");
Transform childEndsWith = transform.FindEndsWith("Name");
Transform childContains = transform.FindContains("ild");
```

### Animation Curve Extensions

```csharp
// Clamp a curve's time and value to [0,1]
AnimationCurve clampedCurve = myAnimationCurve.Clamp01();

// Clone a curve
AnimationCurve clonedCurve = myAnimationCurve.Clone();

// Clamp a curve to custom ranges
AnimationCurve customClampedCurve = myAnimationCurve.Clamp(0f, 10f, -1f, 1f);
```

### String Extensions

```csharp
// Check if a string is null or empty
if (myString.IsNullOrEmpty()) {
    Debug.Log("String is null or empty");
}

// Check if a string is null, empty, or consists only of white space
if (myString.IsNullOrWhiteSpace()) {
    Debug.Log("String is null, empty, or whitespace");
}

// Join an array of strings
string[] parts = new[] { "Hello", "World" };
string joined = parts.Join(" "); // "Hello World"

// Split a string
string text = "Hello,World";
string[] splitResult = text.Split(","); // ["Hello", "World"]
```

### Utility Class Functions

```csharp
// Get the average of multiple values
float avgFloat = Utility.Average(1.0f, 2.0f, 3.0f); // 2.0f
Vector3 avgVector = Utility.Average(Vector3.zero, Vector3.one); // (0.5, 0.5, 0.5)

// Distance calculations
float distance = Utility.Distance(transform1, transform2);
float distance2 = Utility.Distance(Vector3.zero, Vector3.one);

// Direction calculations
Vector3 direction = Utility.Direction(sourcePosition, targetPosition);

// Lerp and InverseLerp with automatic clamping
float lerpResult = Utility.Lerp(0f, 10f, 0.5f); // 5.0f
float inverseLerp = Utility.InverseLerp(0f, 10f, 5f); // 0.5f
```

### Interval Structures

```csharp
// Create an interval
var interval = new Interval(0f, 10f);

// Lerp within the interval
float value = interval.Lerp(0.5f); // 5.0f

// Check if a value is in range
bool inRange = interval.InRange(5f); // true

// Clamp a value to the interval
float clampedValue = Math.Clamp(15f, interval.Min, interval.Max); // 10.0f
```

### Data Serialization

```csharp
// Create a serialization utility for your data class
var dataUtil = new DataSerializationUtility<PlayerData>("SaveData/PlayerProgress.dat", false);

// Save data
var playerData = new PlayerData();
playerData.PlayerName = "Player1";
playerData.Score = 1000;
dataUtil.SaveOrCreate(playerData);

// Load data
var loadedData = dataUtil.Load();
Debug.Log($"Loaded player: {loadedData.PlayerName}, Score: {loadedData.Score}");

// Delete saved data
dataUtil.Delete();
```

### Editor Utilities

```csharp
// Add custom menu items
[MenuItem("Tools/My Custom Tool")]
static void MyCustomTool() {
    // Use editor utilities here
    EditorUtilities.AddScriptingDefineSymbol("MY_FEATURE_ENABLED");
}

// Use custom styles
void OnGUI() {
    if (GUILayout.Button("Custom Button", EditorUtilities.Styles.ButtonActive)) {
        // Handle button click
    }
}

// Use built-in icons
EditorGUILayout.LabelField(new GUIContent("Warning", EditorUtilities.Icons.Warning));
```

## Further Reading

For more detailed information about all available utilities and their usage, check out:
- The [API Reference](APIReference.md) document
- The [Installation Guide](Installation.md) for additional setup options
- XML documentation comments directly in the source code
- IntelliSense tooltips when using the library in your code editor
