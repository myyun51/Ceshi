# ClassIsland Liquid Glass

A WPF UI library for **ClassIsland plugin components** that supplies the *adaptive-brightness glass* variant of the liquid-glass technique. It is designed to be referenced by a ClassIsland plugin project rather than to replace the host's plugin manifest or lifecycle entry point.

The implementation follows the practical parts of the cited Liquid Glass references for WPF:

- it renders a live crop of the component background, blurs it, then places a translucent tint and a specular rim over it;
- it measures sRGB relative luminance from that cropped background every frame (capped to 30 fps);
- it changes both the white/dark glass tint **and the content foreground** based on the measured brightness. This is the requested adaptive-brightness effect, not a fixed white frosted panel.

> WPF does not expose the framebuffer behind an arbitrary element or provide WebGL-style fragment shaders. Therefore the component explicitly receives the visual that paints the host background. This avoids screen capture, preserves per-window behavior, and makes the effect suitable for ClassIsland component pages.

## Add it to a ClassIsland plugin

1. Add a project reference to `src/ClassIsland.LiquidGlass/ClassIsland.LiquidGlass.csproj` from the ClassIsland plugin project (or package the generated DLL next to the plugin DLL).
2. In the component's view, name the visual that paints its background and pass it as `BackdropSource`. **Do not** pass a parent that also contains the `LiquidGlass` control: that would capture the control itself and cause visual feedback.
3. Bind all text/icon foregrounds to `AdaptiveForeground`. Use the control's content for the existing component layout.

```xml
<UserControl xmlns:glass="clr-namespace:ClassIsland.LiquidGlass.Controls;assembly=ClassIsland.LiquidGlass">
  <Grid>
    <!-- Existing component wallpaper / colored background. -->
    <Border x:Name="ComponentBackground" Background="#FF31425A" />

    <glass:LiquidGlass BackdropSource="{Binding ElementName=ComponentBackground}"
                       CornerRadius="18"
                       BlurRadius="14"
                       SurfaceOpacity="0.32">
      <StackPanel>
        <TextBlock Text="下一节：数学"
                   Foreground="{Binding AdaptiveForeground, RelativeSource={RelativeSource AncestorType=glass:LiquidGlass}}" />
        <TextBlock Text="08:20 – 09:00"
                   Foreground="{Binding AdaptiveForeground, RelativeSource={RelativeSource AncestorType=glass:LiquidGlass}}"
                   Opacity="0.78" />
      </StackPanel>
    </glass:LiquidGlass>
  </Grid>
</UserControl>
```

## Tuning

| Property | Default | Purpose |
| --- | ---: | --- |
| `CornerRadius` | `18` | Shape of the liquid lens and highlight rim. |
| `BlurRadius` | `14` | Softening of the refracted background crop. |
| `SurfaceOpacity` | `0.32` | Strength of the adaptive white/dark tint, `0`–`1`. |
| `BackdropSource` | required | Background-only WPF visual to sample. |

For image-heavy pages, use `BlurRadius` between 10 and 18 and leave `SurfaceOpacity` at 0.28–0.38; that keeps content legible without erasing background detail.

## Build

Requires the .NET 8 SDK with the Windows desktop workload:

```bash
dotnet build ClassIsland.LiquidGlass.sln -c Release
```

The project intentionally contains no dependency on a particular ClassIsland SDK version. That keeps it compatible with a plugin's existing SDK reference and avoids binding conflicts when ClassIsland updates.
