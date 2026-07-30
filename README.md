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

#### Setup

There is a separate Avalonia integration project that lets you define a dynamic scheme in XAML without C# code and access Material color tokens with markup extensions. The seed color can be bound to a property or a dynamic resource. The integration uses its own attached-property and immutable-snapshot binding layer; it does not depend on `DesignTokens.Avalonia`.

To set up a dynamic scheme, set the attached `MaterialColor.Scheme` property to your preferred scheme. The scheme is an authoring input and is not inherited itself. MCU creates a `ResolvedColorScheme` snapshot and publishes it through the inherited `MaterialColor.ResolvedScheme` property; descendants and color bindings consume that snapshot. Setting the scheme on `Application` provides an application-level fallback for elements that do not have a nearer scheme.

```xml
<Application>
    <mcu:MaterialColor.Scheme>
        <mcu:TonalSpotScheme Color="{DynamicResource SystemAccentColor}" />
    </mcu:MaterialColor.Scheme>
</Application>
```

Thanks to Avalonia's ability to use any type for markup extensions, the XAML can be much simplified:

```xml
<Border mcu:MaterialColor.Scheme="{mcu:TonalSpotScheme {Binding ThemeColor}}" />
```

`ColorScheme` is the mutable, bindable authoring object. When it changes, MCU replaces the
immutable `ResolvedColorScheme` snapshot exposed through the inherited
`MaterialColor.ResolvedScheme` attached property. Applications that integrate MCU into another
theme system can publish a snapshot directly with `MaterialColor.SetResolvedScheme(...)` and can
read resolved roles or palettes through `ResolvedColorScheme.GetColor`, `GetBrush`, and
`GetPaletteColor`.

#### Color Extraction

Currently color extract XAML markup extension is not implemented, but it will be here after some time.

#### Color Token with Markup Extension

Access Material 3 design tokens using strongly-typed markup extensions. These extensions automatically resolve to either a `SolidColorBrush` or a `Color` binding depending on the target property. The binding will automatically update when theme/scheme/seed color change.

- `MdSysColor (SysColorToken token)`
- `MdRefPalette (RefPaletteToken palette, byte tone)`

`MdSysColor` follows the target's `ActualThemeVariant` by default and supports the optional
`Theme` property to pin light or dark resolution. `MdRefPalette` is seed-based and therefore does
not have a theme parameter.

```xml
<Border Background="{mcu:MdRefPalette Primary, 60}" Grid.Row="0">
    <TextBlock Classes="label-medium" Foreground="{mcu:MdSysColor OnPrimary}">Primary</TextBlock>
</Border>
```

#### Custom Colors

Named custom colors are supported. Each `CustomColor` contributes a tonal palette and the four
Material custom roles (`Custom`, `OnCustom`, `CustomContainer`, and `OnCustomContainer`). Custom
colors may be harmonized toward the scheme's source color (the default) or kept exact.

```xml
<Panel>
    <mcu:MaterialColor.Scheme>
        <mcu:TonalSpotScheme Color="#6750A4">
            <mcu:CustomColor Name="Brand" Color="#FF5722" />
        </mcu:TonalSpotScheme>
    </mcu:MaterialColor.Scheme>

    <Border Background="{mcu:MdSysColor CustomContainer, CustomKey=Brand}">
        <TextBlock Foreground="{mcu:MdSysColor OnCustomContainer, CustomKey=Brand}"
                   Text="Brand" />
    </Border>
</Panel>
```

Use `MdRefPalette Custom` with `CustomKey` to read a tone from the named custom palette.

The integration does not currently provide image-based color extraction.

#### Theme Override

By default, the `MdSysColor` markup extension reacts to the current `ActualThemeVariant`. However, you can explicitly set the `Theme` parameter to ensure a resource always resolves to a specific theme, which is particularly useful when defining `ThemeDictionaries`.

```xml
<ResourceDictionary>
    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Light">
            <SolidColorBrush x:Key="SystemControlBackgroundBrush" 
                             Color="{mcu:MdSysColor Surface, Theme=Light}" />
        </ResourceDictionary>

        <ResourceDictionary x:Key="Dark">
            <SolidColorBrush x:Key="SystemControlBackgroundBrush" 
                             Color="{mcu:MdSysColor Surface, Theme=Dark}" />
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```
