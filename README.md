# Material Color Utilities

Color is a powerful design tool and part of the Material system along with
styles like typography and shape. In products, colors and the way they are used
can be vast and varied. An app’s color scheme can express brand and style.
Semantic colors can communicate meaning. And color contrast control supports
visual accessibility.

In many design systems of the past, designers manually picked app colors to
support the necessary range of color applications and use cases. Material 3
introduces a dynamic color system, which does not rely on hand-picked colors.
Instead, it uses color algorithms to generate beautiful, accessible color
schemes based on dynamic inputs like a user’s wallpaper. This enables greater
flexibility, personalization, and expression, all while streamlining work for
designers and teams.

Material Color Utilities (MCU) powers dynamic color with a set of color
libraries containing algorithms and utilities that make it easier for you to
develop color themes and schemes in your app.

<video autoplay muted loop src="https://user-images.githubusercontent.com/6655696/146014425-8e8e04bc-e646-4cc2-a3e7-97497a3e1b09.mp4" data-canonical-src="https://user-images.githubusercontent.com/6655696/146014425-8e8e04bc-e646-4cc2-a3e7-97497a3e1b09.mp4" class="d-block rounded-bottom-2 width-fit" style="max-width:640px;"></video>

# MaterialColorUtilities for .NET

This repository contains a C# port of Google's official `material-color-utilities` library.

## Current Status

This implementation is currently a **work-in-progress**. The API is mostly the same as the original, with minor adjustments to add some C# flavor.

Most parts of material-color-utilities (except CorePalette, which is labeled as deprecated) and all unit tests have been ported. All ported unit tests are passing. Feel free to try it out and give your feedback.

This project support all color schemes from original Material Design 3 and Material 3 Expressive. Watch variant is also ported.

Synced with upstream commit:
[ec7c4da](https://github.com/material-foundation/material-color-utilities/commit/ec7c4da3e0774264275377cd6b7687474bad577a)

⚠️ This library is **not yet** ready for production use.

## APIs

### Core libs

Most APIs are identical to the original implementation. There are some differences:

- For convenience, this library uses ArgbColor struct to represent a color, instead of an int as the original implementation does.

### Avalonia integration

The Avalonia package exposes Material colors through Avalonia's native resource system.
`ColorScheme` is the mutable, bindable input; `MaterialColorResources` is a resource
provider. All generated values are Avalonia `Color` values, addressed by typed keys.

**Breaking change:** the old attached-property/snapshot API and MCU color markup
extensions have been removed. See [Migrating to native resources](docs/avalonia-native-resources-migration.md)
for the complete migration and behavior changes. Ergonomic markup extensions are
intentionally deferred in this initial API.

#### Register a scheme

Add a provider to a resource dictionary's `MergedDictionaries`, at application,
window, or control scope. Use the same XML namespace for the schemes and token keys:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mcu="https://github.com/Shirasagi0012/MaterialColorUtilities.Avalonia"
             RequestedThemeVariant="Default">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <mcu:MaterialColorResources>
                    <mcu:TonalSpotScheme Color="#6750A4" SpecVersion="Spec2025" />
                </mcu:MaterialColorResources>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

`Scheme` is the provider's content property and a `StyledProperty`. The provider is
not a dictionary: do not assign it directly to `Resources` or use it as a
`ThemeDictionaries` entry. Put it in an ordinary dictionary's `MergedDictionaries`.

The seed above is an explicit application choice. MCU does not choose a default
seed or automatically register global resources. A null scheme or unset/null
primary seed supplies no resources. `Color="{DynamicResource SystemAccentColor}"`
also works when your application supplies that resource; it is not guaranteed by
MCU on every platform. The defaults remain `Spec2021`, `Phone`, and contrast 0.
CMF requires `Spec2026` and supports its existing secondary seed.

#### Consume system colors

`SysColorToken` enum values are the resource keys. Strings such as `"Primary"`
are not aliases.

```xml
<Border Background="{DynamicResource {x:Static mcu:SysColorToken.PrimaryContainer}}">
    <TextBlock Text="Material colors"
               Foreground="{DynamicResource {x:Static mcu:SysColorToken.OnPrimaryContainer}}" />
</Border>
```

Use `DynamicResource` for live scheme and theme updates. Avalonia converts a `Color`
to an immutable brush when the target property type is `IBrush`, such as
`Background` or `Foreground`. The provider itself always returns `Color`. For a
concrete `SolidColorBrush`, create it explicitly and bind its `Color`:

```xml
<SolidColorBrush x:Key="MaterialSurfaceBrush"
                 Color="{DynamicResource {x:Static mcu:SysColorToken.Surface}}" />
```

A shared brush resolves in its declaration/owner context. It is not reinterpreted
against each consumer's local scheme. Put `DynamicResource` directly on the target,
style setter, or template when each target should use its own resource scope.

#### Palettes and named custom colors

Multi-argument keys are immutable values:

- `new RefPaletteKey(RefPaletteToken.Primary, 60)` selects a standard palette tone
- `new RefPaletteKey("Brand", 60)` selects a named custom palette tone
- `new CustomColorKey("Brand", CustomColorRole.Container)` selects a custom role

Avalonia 12.1 cannot construct these structs with `x:Arguments`. For native XAML,
expose application-owned key values as static properties, then use `x:Static`.
These are ordinary keys in your application, not MCU markup extensions or string
aliases:

```csharp
using MaterialColorUtilities.Avalonia.Tokens;

namespace YourApp;

public static class ThemeKeys
{
    public static RefPaletteKey Primary60 { get; } = new(RefPaletteToken.Primary, 60);
    public static RefPaletteKey Brand60 { get; } = new("Brand", 60);
    public static CustomColorKey BrandContainer { get; } = new("Brand", CustomColorRole.Container);
}
```

```xml
<Border xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:mcu="https://github.com/Shirasagi0012/MaterialColorUtilities.Avalonia"
        xmlns:local="using:YourApp">
    <Border.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <mcu:MaterialColorResources>
                    <mcu:TonalSpotScheme Color="#6750A4">
                        <mcu:CustomColor Name="Brand" Color="#FF5722" Harmonize="True" />
                    </mcu:TonalSpotScheme>
                </mcu:MaterialColorResources>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Border.Resources>
    <StackPanel Spacing="8">
        <Border Height="40" Background="{DynamicResource {x:Static local:ThemeKeys.Primary60}}" />
        <Border Height="40" Background="{DynamicResource {x:Static local:ThemeKeys.Brand60}}" />
        <Border Height="40" Background="{DynamicResource {x:Static local:ThemeKeys.BrandContainer}}" />
    </StackPanel>
</Border>
```

Tones are bytes in the range 0–100. The standard constructor does not accept
`RefPaletteToken.Custom`; use the named constructor.

Custom roles are `Color`, `OnColor`, `Container`, and `OnContainer`. Names use
`StringComparer.OrdinalIgnoreCase` for key equality and hashing, so `Brand` and
`brAND` address the same resource, including ordinary dictionary overrides. Names
must be nonempty and have no leading/trailing whitespace; they are not trimmed.
Later complete custom inputs with the same name win. Incomplete custom inputs (null
name or seed) are skipped. Harmonization is optional and defaults to true. Custom
roles retain fixed light/dark tones; standard palette lookup now uses the requested
theme, which can change Spec2025 results compared with the old API.

#### Scope, overrides, and themes

Native Avalonia resource lookup handles scope and precedence. To override one
role, add a direct entry to the dictionary that contains the merged provider:

```xml
<Color x:Key="{x:Static mcu:SysColorToken.Primary}">#B3261E</Color>
```

This overrides only that role; it does not regenerate related colors or validate
the resulting contrast. A local provider missing a custom name allows that key to
fall back to outer resources. Missing resources follow native miss/unset behavior,
not an automatic transparent color.

Use `ThemeVariantScope RequestedThemeVariant="Dark"` or native Light/Dark theme
dictionaries for fixed-theme scopes. There is no per-expression theme pin.
Light/Dark subtrees can use one provider concurrently. Custom variants follow their
`InheritVariant` chain; null, `Default`, and variants without a Light/Dark ancestor
resolve to light at the provider level.

In Avalonia 12.1, a brush's declaration context matters even inside theme dictionaries:
inline Light/Dark dictionaries under a control use that declaring control's theme
for the brush's nested `DynamicResource`. For brushes pinned to their dictionary's
Light/Dark variant, put the theme dictionaries in a standalone `ResourceDictionary`
loaded through `ResourceInclude`, as the Gallery does. These native anchor behaviors
are not changed by MCU.

#### C# lookup and binding

```csharp
using Avalonia.Controls;
using Avalonia.Media;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;

var scheme = new TonalSpotScheme(Color.Parse("#6750A4"));
border.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
scheme.Color = Color.Parse("#006A6A");

if (border.TryFindResource(SysColorToken.Primary, border.ActualThemeVariant, out var value)
    && value is Color color)
{
    var brush = new SolidColorBrush(color);
}

border.Resources[new CustomColorKey("Brand", CustomColorRole.Container)] =
    Color.Parse("#FFDBCF");
```

Pass `ActualThemeVariant` explicitly for a themed C# lookup. The overload without a
theme passes null; it does not automatically use the caller's actual theme. Use
host resource lookup rather than `Resources[key]`, which only reads direct entries.
A lookup is a snapshot of the value; use Avalonia's `GetResourceObservable(key)` for
continued observation and dispose its subscription normally.

For scheme/input bindings, use an explicit source or element name. A resource
provider is not a `StyledElement` and does not automatically inherit the owner's
`DataContext`. The Gallery uses a named root as the binding source and demonstrates
system colors, both palette key constructors, custom roles, and theme dictionaries.

For `Source={StaticResource ThemeSettings}`, use `ReflectionBinding` or an explicitly
typed binding path. Put the settings resource in an earlier merged dictionary or an
already available outer scope. A direct entry in the same dictionary is not yet
available during construction of its merged provider, even if it appears first in
XAML. The named-root binding in the Gallery avoids this construction-order issue.

Image-based color extraction is not provided by the integration.
