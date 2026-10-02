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

A C# port of Google's [Material Color Utilities](https://github.com/material-foundation/material-color-utilities)
for Material 3 and Material 3 Expressive color schemes. Includes HCT,
tonal palettes, contrast, blending, pixel quantization, color scoring, and Phone/Watch schemes.
The optional Avalonia integration exposes generated colors as native resources.

This project is **pre-release**; APIs may change and it is not yet production-ready.
Upstream baseline: [ec7c4da](https://github.com/material-foundation/material-color-utilities/commit/ec7c4da3e0774264275377cd6b7687474bad577a).

## Installation

Both packages target **.NET 8 and .NET 10**. The Avalonia integration uses **Avalonia 12.1.3**.
[CI](.github/workflows/ci.yml) is configured to publish `main` builds to GitHub Packages.
Before installing, [configure an authenticated NuGet source](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry#authenticating-to-github-packages)
for `https://nuget.pkg.github.com/Shirasagi0012/index.json`.

```sh
# Core algorithms only
dotnet add package Shirasagi0012.MaterialColorUtilities --prerelease
# Avalonia integration (includes the core package)
dotnet add package Shirasagi0012.MaterialColorUtilities.Avalonia --prerelease
```

## Core usage

The core API follows upstream, using `ArgbColor` instead of a packed integer:

```csharp
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Scheme;
using MaterialColorUtilities.Utils;

var seed = Hct.From(new ArgbColor(0xFF6750A4u));
var scheme = new SchemeTonalSpot(seed, isDark: false, contrastLevel: 0);
ArgbColor primary = scheme.Primary;
ArgbColor onPrimary = scheme.OnPrimary;
```

The core quantizers and `Score.CalculateScore` can select seed colors from decoded
image pixels. Quantization runs synchronously on the calling thread; see the
[synchronous API and migration notes](docs/synchronous-quantization.md).
Image decoding and an Avalonia image-extraction adapter are not provided.

## Avalonia integration

### Register a scheme

Add `MaterialColorResources` to a resource dictionary's `MergedDictionaries` at
application, window, or control scope. Use the `mcu` XML namespace below throughout:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mcu="https://github.com/Shirasagi0012/MaterialColorUtilities.Avalonia">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <mcu:MaterialColorResources
                    Scheme="{mcu:TonalSpotScheme '#6750A4', SpecVersion=Spec2025}" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Defaults are `Spec2021`, `Phone`, and contrast `0`; `CmfScheme` requires `Spec2026`.
No provider or seed is registered automatically: a null scheme or unset seed supplies
no resources. The provider cannot replace `Resources` or be a `ThemeDictionaries` entry.

### Use colors

The markup extensions wrap native `DynamicResource`, so colors update with the
scheme and the target's theme. Generated values are Avalonia `Color`; Avalonia
converts them to brushes for properties such as `Background` and `Foreground`.

```xml
<Border Background="{mcu:MdSysColor PrimaryContainer}">
    <TextBlock Text="Material colors"
               Foreground="{mcu:MdSysColor OnPrimaryContainer}" />
</Border>
<Border Background="{mcu:MdRefPalette Primary, 60}" />
```

The keys are typed values, not strings:

- `MdSysColor Primary` uses `SysColorToken.Primary`
- `MdRefPalette Primary, 60` uses `new RefPaletteKey(RefPaletteToken.Primary, 60)`
- `MdRefPalette CustomName=Brand, Tone=60` uses `new RefPaletteKey("Brand", 60)`
- `MdCustomColor Brand, Container` uses `new CustomColorKey("Brand", CustomColorRole.Container)`

Native lookup is equivalent: `{DynamicResource {x:Static mcu:SysColorToken.Primary}}`.
Palette tones are integers from `0` through `100`. For a named palette, use
`CustomName` without also setting `Palette`.

### Add custom colors

Use the object-element form inside `MergedDictionaries` to register named colors:

```xml
<mcu:MaterialColorResources>
    <mcu:TonalSpotScheme Color="#6750A4" SpecVersion="Spec2025">
        <mcu:CustomColor Name="Brand" Color="#FF5722" Harmonize="True" />
    </mcu:TonalSpotScheme>
</mcu:MaterialColorResources>
```

```xml
<Border Background="{mcu:MdCustomColor Brand, Container}">
    <TextBlock Text="Brand" Foreground="{mcu:MdCustomColor Brand, OnContainer}" />
</Border>
<Border Background="{mcu:MdRefPalette CustomName=Brand, Tone=60}" />
```

Custom roles are `Color`, `OnColor`, `Container`, and `OnContainer`. Names are
case-insensitive, nonempty, and must not have surrounding whitespace. Harmonization
defaults to `True`; the last complete entry with the same name wins.

### Bind and update inputs

Schemes are mutable, bindable inputs. Bind the constructor's seed using a named root
whose view model exposes a `Color`-valued `Seed`, or use an application-supplied color resource:

```xml
<mcu:MaterialColorResources
    Scheme="{mcu:TonalSpotScheme {ReflectionBinding DataContext.Seed, ElementName=Root}, SpecVersion=Spec2025}" />
<mcu:MaterialColorResources
    Scheme="{mcu:TonalSpotScheme Color={DynamicResource SeedColor}, SpecVersion=Spec2025}" />
```

These are alternatives; `Root` must be named in the same XAML namescope.
Schemes and providers are not `StyledElement`s and do not themselves inherit
`DataContext`. Inline XAML bindings can use their declaration anchor; prefer an
explicit source or named root, especially across resource-dictionary boundaries.
A `StaticResource` source must be in an already available outer scope or earlier merged dictionary.

```csharp
using Avalonia.Controls;
using Avalonia.Media;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;

var scheme = new TonalSpotScheme(Color.Parse("#6750A4"));
border.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
scheme.Color = Color.Parse("#006A6A"); // Updates dynamic-resource consumers.
border.TryFindResource(SysColorToken.Primary, border.ActualThemeVariant, out var color);
```

C# lookup returns a snapshot; pass `ActualThemeVariant` explicitly. Use
`GetResourceObservable(key)` for ongoing observation and dispose subscriptions normally.

### Themes and overrides

Native Avalonia lookup determines scope and precedence. Use
`ThemeVariantScope RequestedThemeVariant="Dark"` for a dark subtree. One provider
can serve Light and Dark consumers simultaneously. To override a single role,
add a direct entry to the dictionary containing the merged provider:

```xml
<Color x:Key="{x:Static mcu:SysColorToken.Primary}">#B3261E</Color>
```

Overrides do not regenerate related colors or check contrast. Shared brushes resolve
in their declaration context; put the dynamic resource on the target, setter, or
template when each consumer needs its own scope.

## Examples and further reading

- [Gallery](MaterialColorUtilities.Gallery): editable schemes, custom colors, and theme scopes
- [Native-resource migration guide](docs/avalonia-native-resources-migration.md): previous API replacements, resource-key validation, binding setup, and detailed lookup behavior

## License

[Apache 2.0](LICENSE.txt). Original algorithms © Google LLC; C# port by Shirasagi0012.
