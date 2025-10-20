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

Material Color Ultilities (MCU) powers dynamic color with a set of color
libraries containing algorithms and utilities that make it easier for you to
develop color themes and schemes in your app.

<video autoplay muted loop src="https://user-images.githubusercontent.com/6655696/146014425-8e8e04bc-e646-4cc2-a3e7-97497a3e1b09.mp4" data-canonical-src="https://user-images.githubusercontent.com/6655696/146014425-8e8e04bc-e646-4cc2-a3e7-97497a3e1b09.mp4" class="d-block rounded-bottom-2 width-fit" style="max-width:640px;"></video>

## MaterialColorUtilities for .NET

This repository contains a C# port of Google's official `material-color-utilities` library.

### Motivation

An earlier C# port (albi005/MaterialColorUtilities) exists but seems no longer actively maintained and outdated. This project was initiated to provide an up-to-date implementation targeting the latest .NET platform.

### Current Status

This implementation is currently a **work-in-progress**. The API is mostly the same as the original, with minor adjustments to add some C# flavor.

Most parts of material-color-utilities (except CorePalette, which is labeled as deprecated) and all unit tests have been ported. All ported unit tests are passing. Feel free to try it out and give your feedback.

⚠️ This library is **not yet** ready for production use.

### APIs

Most API are identical to original implementation. There are some differences:

- For convenience, this library uses ArgbColor struct to represent a color, instead of an int as the original implementation does.
- Some `.From***(...)` method are replaced by constructor.

There are a separate project for Avalonia app to use, which give you ability to directly define a dynamic scheme in XAML with no C# code needed, and easy access to material color tokens as resources. Seed color can be bind to a property, or a dynamic resource, and the color scheme will auto update. Extended Colors and color harmonization are also supported.

You only need to put this in ResourceDictionary.MergedDictionaries

```xml
<mcu:DynamicMaterialColorScheme>
    <mcu:DynamicMaterialColorScheme.Scheme>
        <mcu:TonalSpotScheme Color="{DynamicResource SystemAccentColor}"/>
    </mcu:DynamicMaterialColorScheme.Scheme>
    <mcu:ExtendedPalette x:Key="Chartreuse" Color="Chartreuse" Harmonized="True" />
    <mcu:ExtendedPalette x:Key="Azure" Color="#007cf7" Harmonized="False" />
</mcu:DynamicMaterialColorScheme>
```

Thanks to Avalonia's ability to use any type for markup extensions, the XAML can be much simplified:

```xml
<mcu:DynamicMaterialColorScheme Scheme="{mcu:ExpressiveScheme {Binding SourceColor}}" />
```

Currently color extract XAML markup extension is not implemented, but it will be here after some time.
