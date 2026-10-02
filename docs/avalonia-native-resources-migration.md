# Migrating Avalonia integration to native resources

This is a **breaking API and behavior change**. There is no compatibility bridge.
The core HCT, palette, contrast, quantizer, score, and scheme algorithms are
unchanged. Applications using only the core library do not need this migration.

## Initial public API

The Avalonia integration now has one resource lookup path:

`ColorScheme` → `MaterialColorResources` → native Avalonia resource lookup → `Color`.

Register `MaterialColorResources` in `ResourceDictionary.MergedDictionaries`, then
consume its colors with native `DynamicResource`. `Scheme` is a nullable
`StyledProperty` and the provider's XAML content property. Existing `ColorScheme`
classes remain mutable inputs, including contrast, platform, spec, custom colors,
and CMF's secondary seed. Inputs and scheme replacement invalidate resources.

The initial API intentionally defers all ergonomic MCU markup extensions. There is
no replacement `MdSysColor`, `MdRefPalette`, or `MdCustomColor` extension in this
release. Use the native syntax shown in the [README](../README.md#avalonia-integration).

## Removed API and replacements

| Removed API | Replacement |
| --- | --- |
| `MaterialColor.Scheme` / `MaterialColor.SetScheme` | `MaterialColorResources { Scheme = ... }` in `MergedDictionaries` |
| Inherited `MaterialColor.ResolvedScheme` / `SetResolvedScheme` | Native resource scopes and ordinary `Color` resources |
| Public `ResolvedColorScheme` reads (`GetColor`, `GetBrush`, `GetPaletteColor`) | `TryFindResource(key, actualTheme, out value)` or direct core algorithms |
| `{mcu:MdSysColor Primary}` | `{DynamicResource {x:Static mcu:SysColorToken.Primary}}` |
| `{mcu:MdRefPalette Primary, 60}` | `DynamicResource` using `new RefPaletteKey(RefPaletteToken.Primary, 60)` |
| `{mcu:MdRefPalette Custom, 60, CustomKey=Brand}` | `DynamicResource` using `new RefPaletteKey("Brand", 60)` |
| `SysColorToken.Custom` | `new CustomColorKey(name, CustomColorRole.Color)` |
| `SysColorToken.OnCustom` | `new CustomColorKey(name, CustomColorRole.OnColor)` |
| `SysColorToken.CustomContainer` | `new CustomColorKey(name, CustomColorRole.Container)` |
| `SysColorToken.OnCustomContainer` | `new CustomColorKey(name, CustomColorRole.OnContainer)` |
| `MdSysColor.Theme` | `ThemeVariantScope`, theme dictionaries, or an explicit one-time C# lookup |
| `ColorTokenBinding` and target-type helper | Avalonia's native `DynamicResource` and native color-to-brush conversion |

Standard `SysColorToken` values retain their numbers. Use the actual enum key;
strings such as `"Primary"` or `"md.sys.color.primary"` are not equivalent keys.
The token enums and immutable keys are available through the existing MCU XML
namespace and `MaterialColorUtilities.Avalonia.Tokens` in C#.

External theme integrations that formerly published a resolved snapshot should
publish ordinary `Color` resources with these keys. Do not create another inherited
snapshot path or copy MCU's generation into a custom resolver.

## Register at the intended scope

Move the old scheme declaration into a normal dictionary:

```xml
<Border.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <mcu:MaterialColorResources>
                <mcu:TonalSpotScheme Color="#6750A4" />
            </mcu:MaterialColorResources>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Border.Resources>
```

Use `Application.Resources` or `Window.Resources` for broader scope. A provider is
not an `IResourceDictionary` or `IThemeVariantProvider`; it cannot directly replace
`Resources` or be a `ThemeDictionaries` value. The ordinary dictionary owns it.
Create separate providers for separate resource owners; a scheme can be shared as
input. Removing and reattaching a provider must not require a public `Dispose` call.

No application provider or fallback seed is created for you. Null `Scheme` or an
unset/null primary color produces a resource miss. CMF's missing secondary seed
continues to use its primary seed. Existing core spec fallback behavior is unchanged;
CMF still requires `Spec2026`.

Bind the provider or its input using an explicit source, such as a named root:

```xml
<mcu:MaterialColorResources
    Scheme="{ReflectionBinding DataContext.Scheme, ElementName=Root}" />
```

Here `Root` is the named `UserControl` in the same XAML namescope, whose view model
has a `Scheme` property. Resource providers do not automatically inherit owner
`DataContext`. An independent resource dictionary does not gain a data context
simply by receiving an owner. Dynamic-resource seeds are supported when the
application supplies the named seed resource; do not use a generated color as its
own input.

With `Source={StaticResource ThemeSettings}`, Avalonia's compiled binding sees an
object source unless its path includes an explicit type cast. `ReflectionBinding`
is a native alternative. Put `ThemeSettings` in an earlier merged resource dictionary
or a preexisting outer scope: a direct entry in the same dictionary is unavailable
while its merged provider is being constructed, regardless of textual declaration
order. These native source/construction rules also apply to independent dictionaries.

## Immutable palette and custom keys

Avalonia 12.1's XAML compiler rejects struct constructors expressed with
`x:Arguments` (AVLN3000). Expose the constructed keys as static properties in your
application, then consume them with native `x:Static` inside `DynamicResource`.
See the complete README example and `SchemePlaygroundView.axaml` plus its code-behind.
No converter, MCU alias syntax, or markup extension is required. The same immutable
value works for native dictionary overrides in C# or `x:Key="{x:Static ...}"` in XAML.

- `RefPaletteKey(RefPaletteToken palette, byte tone)` accepts standard palettes
- `RefPaletteKey(string customName, byte tone)` selects `RefPaletteToken.Custom`
- `CustomColorKey(string name, CustomColorRole role)` selects one of four roles
- Tone must be 0–100; standard construction with `RefPaletteToken.Custom` is invalid
- Explicit names cannot be null, empty, whitespace-only, or have leading/trailing whitespace
- Equality and hashing use `StringComparer.OrdinalIgnoreCase`; no culture-sensitive comparison, trimming, or Unicode normalization is applied
- Duplicate complete custom inputs use the last declaration with that name; inputs with a null name or seed are skipped
- Unknown names/keys and invalid default/unchecked keys miss rather than returning null or transparent

Contrast must be finite and within −1 to 1 (null means 0); unknown platform and
spec enum values are rejected. Malformed explicit key constructor parameters fail
early. Key equality and hashing are safe even for default struct values. `default(RefPaletteKey)` represents standard
Primary tone 0; `default(CustomColorKey)` is not a valid lookup key.

Custom colors retain their existing fixed-tone contract:

| Role | Light tone | Dark tone |
| --- | --- | --- |
| `Color` | 40 | 80 |
| `OnColor` | 100 | 20 |
| `Container` | 90 | 30 |
| `OnContainer` | 10 | 90 |

These custom role tones do not become contrast-adjusted system roles. Harmonization
still changes the custom palette input when enabled.

## Deliberate behavior changes

### Palette colors follow the requested theme

The old palette path always used the light scheme. The new provider selects the
palette from the requested light/dark scheme, including `InheritVariant` chains.
This corrects integration behavior: some Spec2025 palettes differ between light
and dark. Existing dark palette swatches can therefore change without any change
to the core algorithms.

### Native resource precedence and per-key fallback

Avalonia owns lookup, including local overrides, templates, reparenting, and theme
contexts. A direct entry overrides a merged provider. Merged dictionaries are
searched in reverse registration order. An unknown key or unavailable custom name
returns a miss, so native lookup can continue.

A local scheme does **not** mask all outer custom colors. A missing local custom key
can resolve from an outer dictionary/provider. Explicitly override that key, for
example with transparent `Color`, if that is the desired application policy. There
is no MCU masking marker. Overriding `Primary` alone does not regenerate `OnPrimary`
or otherwise guarantee contrast.

Missing resources no longer receive MCU's transparent fallback. Native unset/miss
semantics apply. Detached objects use Avalonia's lookup semantics; MCU does not
manually search `Application.Current` to reproduce the old fallback behavior.

### Themes and C# reads

Native dynamic resources receive Avalonia's resource theme context. One provider
can answer concurrent Light and Dark subtrees without a global current-theme value.
Custom variants follow their inheritance chain. Null, `Default`, or an unrelated
custom root resolves to light when it reaches MCU's provider.

Always pass `control.ActualThemeVariant` to `TryFindResource` when reading the
control's actual colors. The no-theme overload passes null, not the actual theme.
Do not use `Resources[key]` to read generated entries: it only checks direct entries.
Native lookup returns a boxed `Color`; creating a brush in C# is explicit. For
ongoing updates, use Avalonia's `GetResourceObservable(key)` and dispose subscriptions.

A `ThemeVariantScope` replaces the old expression-level `Theme` pin only at subtree
scope. It also affects other themed resources within that subtree; it is not an
exact replacement for pinning one property while leaving its neighbors unchanged.
Avalonia 12.1 has two distinct native brush declaration behaviors:

- In Light/Dark theme dictionaries declared inline under a control, a brush's nested
  `DynamicResource` uses the enclosing `StyledElement` as its anchor. Both dictionary
  entries follow that declaring control's actual theme; the dictionary key does not
  pin the nested expression
- In a standalone `ResourceDictionary` loaded through `ResourceInclude`, the resource
  provider anchor preserves the Light/Dark dictionary variant for the nested
  expression, including across seed and host-theme changes

Use the external `ResourceInclude` pattern when a shared brush must stay pinned to
its theme dictionary, as in the Gallery's `Assets/ResourceDictionary1.axaml`. For
per-target theme/scheme behavior, keep the color `DynamicResource` directly on the
target, setter, or template. MCU does not replace these native anchor rules or add
an expression-level theme override.

### Color-only values and brushes

Successful provider lookups always return Avalonia `Color`. Native dynamic resources
convert to `ImmutableSolidColorBrush` for target properties typed exactly `IBrush`.
This is not a guarantee for arbitrary concrete `SolidColorBrush`, `IImmutableBrush`,
or custom target types. Build a `SolidColorBrush` explicitly and apply the dynamic
resource to its `Color` property when you need that concrete type.

There is no public brush token set, MCU brush cache, or brush identity guarantee.
Separate consumers/updates may allocate different brush instances. A shared brush
resolves resources in its declaration/owner context, not separately in each
consumer's subtree. Use direct target/setter/template expressions when lookup must
follow each target's local scheme.

## Migration checklist

1. Move each scheme into a provider in the intended dictionary's `MergedDictionaries`
2. Replace snapshot publishing/reading with native resource writes/lookups
3. Replace all removed markup extensions with native `DynamicResource`
4. Replace four custom enum roles with named `CustomColorKey` values
5. Replace expression theme pins with appropriate scopes or dictionary contexts
6. Check missing-resource behavior, custom per-key fallback, and dark palette colors
7. Give bindings an explicit source and C# reads an explicit theme
8. Verify concrete brush consumers, shared brush scope, and theme/seed updates

The Gallery provides compiled examples of these APIs. The Avalonia integration
suite covers native scope/update behavior in addition to the core regression tests.
