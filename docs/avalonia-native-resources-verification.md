# Native resources verification

Verified on 2026-10-02 against Avalonia 12.1.0, on Debian 13 x64 using the
.NET 11.0.100-rc.1.26425.128 SDK and .NET 10.0.12 runtime. This is local verification,
not a claim that remote CI has run or that a package has been published.

## Build, test, and package checks

- Release solution build passed, including net8.0 and net10.0 core/Avalonia libraries
  and the net10.0 Gallery
- Full test suite: **444 passed, 0 failed, 1 preexisting skipped**
- The skipped exhaustive HCT test's algorithm was separately executed against the
  built core DLL: all **16,777,216 RGB colors** round-tripped exactly (19.73 seconds)
- Local NuGet and symbol package generation succeeded for both libraries; package
  versions and publishing configuration were not changed
- `git diff --check` passed; core algorithm source files were unchanged

Reproduce the aggregate checks from the repository root:

```sh
dotnet restore MaterialColorUtilities.slnx
dotnet build MaterialColorUtilities.slnx -c Release --no-restore
dotnet test MaterialColorUtilities.Tests/MaterialColorUtilities.Tests.csproj -c Release --no-build
dotnet pack MaterialColorUtilities.slnx -c Release --no-restore --no-build -p:GeneratePackageOnBuild=false
```

The local constrained executor used `-m:1 -p:UseSharedCompilation=false` for builds
and writable NuGet cache directories. The existing test dependency graph reports
three NU1608 warnings because xUnit is pinned to 3.2.1 while Avalonia.Headless.XUnit
12.1.0 requires xunit.v3.extensibility.core 3.2.2. Existing CS8629 in
`TestUtils/TestExtensions.cs` and xUnit2002 in `SchemeCorrectnessTests.cs` also
remain. No warning suppressions or unrelated dependency changes were added.

## Coverage

The native integration and computation tests cover:

- All 49 fixed system roles and six reference palettes, each sampled at tones
  0, 1, 40, 60, 99, and 100, across nine schemes, three specs, two platforms,
  contrasts -1/0/1, and both themes; CMF has additional single/dual-seed coverage
- Case-insensitive immutable key equality/hashing, invalid enums/names/tones,
  default keys, null inputs, palette theme correctness, fixed custom tones, and
  harmonization
- Native app/window/nested precedence, direct/theme/merged dictionaries, local
  custom-key fallback, light/dark subtrees, custom theme inheritance, and reparenting
- Mutable input invalidation, replacement, missing values, collection clearing,
  reset, duplicate instances, old-item cleanup, weak provider lifetime, disposed
  observables, bounded generation caches, and generation failure/reentrancy recovery
- Seven compiled XAML fixtures exercising Color and IBrush properties, style
  setters, shared ControlTheme and templates, data templates, dictionary brushes,
  dynamic seed resources, explicit sources, named-element bindings, and external
  ResourceInclude dictionaries

Documentation examples use the same compiled syntax as those fixtures and the
Gallery: native enum `x:Static` keys, application-owned static value keys, content
property registration, explicit `Scheme` registration, and native resource lookup.
A separate headless Gallery smoke run loaded the real App and playground, checked
all local providers and scheme replacements, changed the dynamic app seed, and
verified simultaneous Light/Dark palette and external dictionary brush consumers.

## Native XAML findings

The migration guide documents three behaviors verified during this work:

1. Avalonia 12.1 rejects struct construction through `x:Arguments` (AVLN3000).
   Static application key properties plus native `x:Static` compile and run
2. Inline theme-dictionary brushes use the enclosing control's theme. Brushes in
   standalone dictionaries loaded through ResourceInclude retain dictionary theme
   context. Both are asserted in the suite; the Gallery uses the external form
3. Explicit StaticResource binding sources must already be available during
   provider construction. An earlier merged dictionary works; a direct entry in
   the dictionary being constructed is not yet available. ReflectionBinding or
   an explicitly typed compiled path is needed for an object-valued static source

No private lookup chain or ergonomic markup wrapper was added to hide these
native semantics.

## Diagnostic performance sample

`ResourcePerformanceTests.ReportFirstCachedAndBatchThemeResolution` is a repeatable
smoke measurement, not a statistical benchmark or a timing-sensitive test gate.
One isolated Release run on the environment above measured:

| Operation | Total elapsed | Thread-local allocations |
| --- | ---: | ---: |
| First Primary lookup for 100 providers, JIT warmed | 69.826 ms | 18,264 bytes/provider |
| 100,000 cached lookups alternating Light/Dark | 31.090 ms | 24 bytes/lookup |
| 200 native IBrush consumers, 50 theme switches | 23.853 ms | 2,253,224 bytes total |

The first lookup creates both core variants and the current custom palette set.
The repeated lookup measurement reuses a boxed enum key; returned Color values are
boxed. The batch includes native resource reevaluation and brush conversion, but
not rendering. These figures depend on hardware, JIT, load, and inputs; no speedup
relative to the previous API is claimed, and brush identity/allocation is not an
MCU contract.

```sh
dotnet test MaterialColorUtilities.Tests/MaterialColorUtilities.Tests.csproj \
  -c Release --no-build --filter FullyQualifiedName~ResourcePerformanceTests \
  --logger "console;verbosity=detailed"
```
