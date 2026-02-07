# Liquid Glass Tab Bar for .NET MAUI Shell (Android)

This implementation adds a custom liquid glass tab bar effect to your .NET MAUI Shell application, mimicking the newest iOS liquid glass tab bar with margins, blur effect, and rounded corners. This implementation is Android-only.

## Features

- **Liquid Glass Effect**: Semi-transparent background (alpha 180) that shows content underneath
- **Rounded Corners**: 25dp radius for modern look
- **Floating Margins**: 16dp margins on left, right, and bottom
- **Elevation**: 8dp subtle elevation for floating appearance
- **Custom Tab Colors**: Purple (#512BD4) for active tabs, gray for inactive
- **Subtle Border**: 1px stroke (alpha 80) for enhanced glass effect
- **Content Visibility**: Content is visible beneath the tab bar through the semi-transparent glass effect

## Implementation Details

### Files Created/Modified

1. **`Platforms/Android/Helpers/LiquidGlassTabBarHelper.cs`**
   - Static helper class that applies the liquid glass styling
   - Automatically finds the BottomNavigationView in the view hierarchy
   - Handles cleanup to prevent memory leaks

2. **`Platforms/Android/MainActivity.cs`**
   - Modified to call `LiquidGlassTabBarHelper.ApplyLiquidGlassEffect()` on startup
   - Includes proper cleanup in `OnDestroy()`

### How It Works

1. When the app starts, `MainActivity.OnCreate()` is called
2. After a 500ms delay (to ensure Shell is fully loaded), the helper applies the styling
3. The helper recursively searches the view hierarchy to find the `BottomNavigationView`
4. Once found, it applies all liquid glass styling:
   - Rounded corners with GradientDrawable
   - Semi-transparent background (ARGB 200, 255, 255, 255)
   - Margins for floating effect
   - Elevation for shadow
   - Blur effect (Android 12+ only)
   - Custom tab colors

### Customization

You can customize the following parameters in `LiquidGlassTabBarHelper.cs`:

```csharp
// Corner radius (currently 25dp)
var cornerRadius = (int)(25 * context.Resources.DisplayMetrics.Density);

// Margins (currently 16dp)
var marginInPixels = (int)(16 * context.Resources.DisplayMetrics.Density);

        // Background color (currently ARGB 180, 255, 255, 255)
        // Alpha 180 provides good transparency to show content underneath
        var glassColor = global::Android.Graphics.Color.Argb(180, 255, 255, 255);

        // Border color (currently ARGB 80, 255, 255, 255)
        _backgroundDrawable.SetStroke(strokeWidth, global::Android.Graphics.Color.Argb(80, 255, 255, 255));

        // Elevation (currently 8f)
        ViewCompat.SetElevation(bottomNav, 8f);

        // Note: No blur effect is applied to the tab bar itself
        // The glass effect comes from the semi-transparent background

// Active tab color (currently #512BD4)
global::Android.Graphics.Color.ParseColor("#512BD4")
```

### Compatibility

- **All Android versions**: Semi-transparent glass effect works on all versions
- **Other platforms**: No effect applied (Android-only implementation)

### Important Notes

- **No Blur Effect**: The implementation does NOT use `SetRenderEffect` because it would blur the tab bar itself rather than what's behind it. For true background blur in Android, you would need a more complex implementation with BackdropBlurRenderer or custom rendering.
- **Content Visibility**: Content is visible beneath the tab bar through the semi-transparent glass background, creating the desired liquid glass effect.

## Notes

- The helper uses a recursive search to find the `BottomNavigationView`, which may add minimal overhead on app startup
- The effect is only applied once to avoid unnecessary updates
- Proper cleanup is implemented in `OnDestroy()` to prevent memory leaks
- The delay (500ms) ensures the Shell view hierarchy is fully created before applying styling

## Testing

To test the implementation:

1. Run the app on an Android device or emulator
2. Navigate to the tab bar (bottom of the screen)
3. Verify the tab bar has:
   - Rounded corners (25dp)
   - Semi-transparent glass-like appearance (alpha 180)
   - Floating effect with margins (16dp)
   - Subtle elevation (8dp)
   - Purple color (#512BD4) for active tab
   - Content visible beneath the tab bar

## Future Enhancements

Potential improvements:

1. Make the effect configurable through XAML/MAUI properties
2. Add animation support for tab transitions
3. Implement similar effect for iOS platform
4. Add support for custom blur radius and intensity
5. Implement dynamic color support for light/dark themes