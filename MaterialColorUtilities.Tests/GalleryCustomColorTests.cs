using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.Gallery.Controls;
using MaterialColorUtilities.Gallery.ViewModels;
using MaterialColorUtilities.Gallery.Views;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests;

public class GalleryCustomColorTests
{
    [AvaloniaFact]
    public void NameInputTrimsAcceptedNamesAndRetainsTheLastResourceForInvalidDrafts()
    {
        var model = new SchemePlaygroundViewModel();
        model.AddCustomColorCommand.Execute(null);
        var entry = model.CustomColors[1];
        var editor = new CustomColorEditor { DataContext = entry };
        using var host = Show(editor);
        var input = editor.FindControl<TextBox>("NameInput")!;
        var resource = entry.Resource;

        input.Text = "  Highlight \t";
        Assert.Equal("  Highlight \t", entry.Name);
        Assert.Equal("Highlight", entry.ResourceName);
        Assert.Null(entry.NameError);
        AssertRoles(editor, entry);
        var acceptedRoles = BandColors(editor);

        foreach (var invalid in new[] { "", " \t ", " bRaNd " })
        {
            input.Text = invalid;
            Assert.Equal(invalid, entry.Name);
            Assert.False(string.IsNullOrEmpty(entry.NameError));
            Assert.Same(resource, entry.Resource);
            Assert.Equal("Highlight", resource.Name);
            Assert.Equal(acceptedRoles, BandColors(editor));
            Assert.Equal("Brand", model.CustomColors[0].ResourceName);
        }

        input.Text = "  hiGHlight  ";
        Assert.Equal("hiGHlight", resource.Name);
        Assert.Null(entry.NameError);
        AssertRoles(editor, entry);
        input.Text = " Brand ";
        model.CustomColors[0].Name = "Accent";
        Assert.Null(entry.NameError);
        Assert.Equal("Brand", resource.Name);
        AssertRoles(editor, entry);
    }

    [AvaloniaFact]
    public void HexInputPreservesTheLastPreviewUntilValidAndHctEditsUpdateTheInput()
    {
        var model = new SchemePlaygroundViewModel();
        var entry = model.CustomColors[0];
        var editor = new CustomColorEditor { DataContext = entry };
        using var host = Show(editor);
        var input = editor.FindControl<TextBox>("SourceInput")!;

        input.Text = " #336699 ";
        Assert.Equal(Color.Parse("#336699"), entry.SourceColor);
        Assert.Null(entry.SourceError);
        AssertRoles(editor, entry);
        var hct = entry.SelectedHct;
        var harmonized = entry.HarmonizedColor;
        var roles = BandColors(editor);

        foreach (var invalid in new[] { "", "#", "#12345", "#GGHHII", "#12345678" })
        {
            input.Text = invalid;
            Assert.Equal(invalid, entry.SourceHex);
            Assert.False(string.IsNullOrEmpty(entry.SourceError));
            Assert.Equal(Color.Parse("#336699"), entry.Resource.Color);
            Assert.Equal(hct, entry.SelectedHct);
            Assert.Equal(harmonized, entry.HarmonizedColor);
            Assert.Equal(roles, BandColors(editor));
        }

        input.Text = "aAbBcC";
        Assert.Null(entry.SourceError);
        Assert.Equal(Color.Parse("#AABBCC"), entry.SourceColor);
        AssertRoles(editor, entry);

        var selection = new HctSelection(145, 50, 60);
        entry.SelectedHct = selection;
        var selectedColor = selection.ToHct().ToAvaloniaColor();
        Assert.Equal(selectedColor, entry.SourceColor);
        Assert.Equal($"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}", input.Text);
        Assert.Null(entry.SourceError);
        AssertRoles(editor, entry);
    }

    [AvaloniaFact]
    public void HctFlyoutReopensWithCurrentColorAndEditsTheSourceThroughItsBinding()
    {
        var model = new SchemePlaygroundViewModel();
        var entry = model.CustomColors[0];
        var editor = new CustomColorEditor { DataContext = entry };
        using var host = Show(editor);
        var sourceInput = editor.FindControl<TextBox>("SourceInput")!;
        var button = Assert.IsType<Button>(sourceInput.InnerLeftContent);
        var flyout = Assert.IsType<Flyout>(button.Flyout);
        var picker = Assert.IsType<SeedColorPicker>(flyout.Content);

        for (var i = 0; i < 3; i++)
        {
            sourceInput.Text = i % 2 == 0 ? "#006699" : "#AA5500";
            flyout.ShowAt(button);
            Dispatcher.UIThread.RunJobs();
            Assert.True(flyout.IsOpen);
            Assert.Same(entry, picker.DataContext);
            Assert.Equal(entry.SelectedHct, picker.Hct);

            var selection = new HctSelection(90 + i * 60, 45, 65);
            picker.Hct = selection;
            Assert.Equal(selection, entry.SelectedHct);
            var color = selection.ToHct().ToAvaloniaColor();
            Assert.Equal(color, entry.SourceColor);
            Assert.Equal($"#{color.R:X2}{color.G:X2}{color.B:X2}", sourceInput.Text);
            AssertRoles(editor, entry);

            flyout.Hide();
            Dispatcher.UIThread.RunJobs();
            Assert.False(flyout.IsOpen);
            Assert.Equal(color, entry.SourceColor);
        }
    }

    [AvaloniaFact]
    public void HarmonizeTogglesAreIndependentAndHideOnlyTheAdjustedHexPreview()
    {
        var model = new SchemePlaygroundViewModel();
        model.AddCustomColorCommand.Execute(null);
        var first = model.CustomColors[0];
        var second = model.CustomColors[1];
        first.IsExpanded = true;
        var firstEditor = new CustomColorEditor { DataContext = first };
        var secondEditor = new CustomColorEditor { DataContext = second };
        using var host = Show(new StackPanel { Children = { firstEditor, secondEditor } });
        var toggle = firstEditor.FindControl<ToggleSwitch>("HarmonizeToggle")!;
        var secondColors = BandColors(secondEditor);
        var source = first.SourceColor;

        for (var i = 0; i < 3; i++)
        {
            toggle.IsChecked = false;
            Assert.False(first.Harmonize);
            Assert.False(first.Resource.Harmonize);
            Assert.Equal("Off", first.HarmonizeState);
            Assert.False(firstEditor.FindControl<StackPanel>("HarmonizedPreview")!.IsVisible);
            Assert.True(firstEditor.FindControl<StackPanel>("RolePreview")!.IsVisible);
            Assert.Equal(source, first.SourceColor);
            Assert.True(second.Harmonize);
            Assert.Equal(secondColors, BandColors(secondEditor));
            AssertRoles(firstEditor, first);

            toggle.IsChecked = true;
            Assert.True(first.Resource.Harmonize);
            Assert.Equal("On", first.HarmonizeState);
            Assert.True(firstEditor.FindControl<StackPanel>("HarmonizedPreview")!.IsVisible);
            AssertRoles(firstEditor, first);
            Assert.Equal(secondColors, BandColors(secondEditor));
        }
    }

    [AvaloniaFact]
    public void RebuildingSchemeRetainsEntryIdentityDraftsAndSettingsAcrossAllInputs()
    {
        var model = new SchemePlaygroundViewModel();
        model.AddCustomColorCommand.Execute(null);
        var first = model.CustomColors[0];
        var second = model.CustomColors[1];
        first.SourceHex = "#336699";
        second.SourceHex = "#118844";
        second.Harmonize = false;
        first.Name = "";
        first.SourceHex = "#invalid";
        var entries = model.CustomColors.ToArray();
        var resources = entries.Select(entry => entry.Resource).ToArray();
        var editor = new CustomColorEditor { DataContext = first };
        using var host = Show(editor);

        Action[] rebuilds =
        [
            () => model.SelectedHct = new HctSelection(100, 60, 60),
            () => model.SelectedSpecOption = model.SpecOptions.Single(option => option.Version == ColorSpec.SpecVersion.Spec2025),
            () => model.SelectedPlatformOption = SchemePlaygroundViewModel.PlatformOptions.Single(option => option.Platform == DynamicScheme.Platform.Watch),
            () => model.SelectedContrast = 0.75,
            () => model.SelectedSchemeOption = model.SchemeOptions.Single(option => option.SchemeType == typeof(ExpressiveScheme)),
            () => model.SelectedPlatformOption = SchemePlaygroundViewModel.PlatformOptions.Single(option => option.Platform == DynamicScheme.Platform.Phone),
            () => model.SelectedSchemeOption = model.SchemeOptions.Single(option => option.SchemeType == typeof(CmfScheme)),
            () => model.SelectedSecondaryHct = new HctSelection(250, 40, 50),
        ];

        foreach (var rebuild in rebuilds)
        {
            var previous = model.Scheme!;
            rebuild();
            Assert.NotSame(previous, model.Scheme);
            Assert.Empty(previous.CustomColors);
            Assert.Equal(entries, model.CustomColors);
            Assert.Equal(resources, model.Scheme!.CustomColors);
            Assert.All(entries, entry => Assert.Same(model.Scheme, entry.Scheme));
            Assert.Equal("Brand", first.ResourceName);
            Assert.Equal("", first.Name);
            Assert.Equal("#invalid", first.SourceHex);
            Assert.NotNull(first.NameError);
            Assert.NotNull(first.SourceError);
            Assert.Equal(Color.Parse("#336699"), first.SourceColor);
            Assert.True(first.Harmonize);
            Assert.False(first.IsExpanded);
            Assert.False(second.Resource.Harmonize);
            Assert.True(second.IsExpanded);
            AssertRoles(editor, first);
        }
    }

    [AvaloniaFact]
    public void RoleBrushesFollowThemeRenameSourceAndSeedThroughDynamicResources()
    {
        var model = new SchemePlaygroundViewModel();
        var entry = model.CustomColors[0];
        var editor = new CustomColorEditor { DataContext = entry };
        using var host = Show(editor);
        AssertRoles(editor, entry);
        var light = BandColors(editor);

        host.Window.RequestedThemeVariant = ThemeVariant.Dark;
        AssertRoles(editor, entry);
        Assert.NotEqual(light, BandColors(editor));

        host.Window.RequestedThemeVariant = new ThemeVariant("Dim", ThemeVariant.Dark);
        AssertRoles(editor, entry);

        var rolePreview = editor.FindControl<StackPanel>("RolePreview")!;
        rolePreview.Resources[new CustomColorKey("Brand", CustomColorRole.Color)] = Colors.Magenta;
        Assert.Equal(Colors.Magenta, BandColors(editor)[0]);
        Assert.Equal(Colors.Magenta, MaterialColorTestHelper.BrushColor(editor.FindControl<TextBlock>("OnColorLabel")!.Foreground));
        entry.Name = " Renamed ";
        AssertRoles(editor, entry);
        var renamed = BandColors(editor);
        entry.SourceHex = "#1565C0";
        AssertRoles(editor, entry);
        Assert.NotEqual(renamed, BandColors(editor));
        var beforeSeed = BandColors(editor);
        model.SelectedHct = new HctSelection(140, 60, 55);
        AssertRoles(editor, entry);
        Assert.NotEqual(beforeSeed, BandColors(editor));

        host.Window.RequestedThemeVariant = ThemeVariant.Light;
        AssertRoles(editor, entry);
    }

    [AvaloniaFact]
    public void RepeatedAddCollapseAndRemoveControlsRestoreTheEmptyState()
    {
        var model = new SchemePlaygroundViewModel();
        var view = new SchemePlaygroundView { DataContext = model };
        using var host = Show(view);
        var items = view.FindControl<ItemsControl>("CustomColorItems")!;
        var add = view.FindControl<Button>("AddCustomColorButton")!;
        var empty = view.GetVisualDescendants().OfType<TextBlock>()
            .Single(block => block.Text == "Add a custom color to preview its roles.");
        Assert.False(empty.IsVisible);

        for (var cycle = 0; cycle < 3; cycle++)
        {
            while (model.CustomColors.Count > 0) model.CustomColors[0].RemoveCommand.Execute(null);
            host.Window.UpdateLayout();
            Assert.True(model.HasNoCustomColors);
            Assert.True(empty.IsVisible);
            Assert.Empty(model.Scheme!.CustomColors);
            Assert.Empty(items.GetVisualDescendants().OfType<CustomColorEditor>());

            add.Command!.Execute(add.CommandParameter);
            add.Command.Execute(add.CommandParameter);
            add.Command.Execute(add.CommandParameter);
            host.Window.UpdateLayout();
            Assert.False(model.HasNoCustomColors);
            Assert.False(empty.IsVisible);
            Assert.Equal(new[] { "Custom", "Custom 2", "Custom 3" }, model.CustomColors.Select(entry => entry.ResourceName));
            Assert.Equal(3, model.Scheme.CustomColors.Count);
            var editors = items.GetVisualDescendants().OfType<CustomColorEditor>().ToArray();
            Assert.Equal(3, editors.Length);
            Assert.False(editors[0].FindControl<StackPanel>("EditorBody")!.IsVisible);
            Assert.True(editors[2].FindControl<StackPanel>("EditorBody")!.IsVisible);

            var expand = editors[2].FindControl<Button>("ExpandButton")!;
            expand.Command!.Execute(expand.CommandParameter);
            Assert.False(model.CustomColors[2].IsExpanded);
            Assert.False(editors[2].FindControl<StackPanel>("EditorBody")!.IsVisible);
            expand.Command.Execute(expand.CommandParameter);
            var removed = model.CustomColors[2];
            var remove = editors[2].GetVisualDescendants().OfType<Button>()
                .Single(button => AutomationProperties.GetName(button) == "Remove color");
            remove.Command!.Execute(remove.CommandParameter);
            removed.RemoveCommand.Execute(null);
            Assert.Equal(2, model.CustomColors.Count);
            Assert.DoesNotContain(removed.Resource, model.Scheme.CustomColors);
            Assert.True(model.CustomColors[1].IsExpanded);
        }

        foreach (var entry in model.CustomColors.ToArray()) entry.RemoveCommand.Execute(null);
        Assert.True(model.HasNoCustomColors);
        Assert.True(empty.IsVisible);
        Assert.Empty(model.Scheme!.CustomColors);
    }

    [AvaloniaFact]
    public void RoleBandsHaveTheApprovedHeightsAndFourPixelContainerGap()
    {
        var editor = new CustomColorEditor { DataContext = new SchemePlaygroundViewModel().CustomColors[0] };
        using var host = Show(editor);
        var bands = Bands(editor);
        Assert.Equal(new[] { 56d, 40d, 56d, 40d }, bands.Select(band => band.Height));
        Assert.Equal(new Thickness(0, 4, 0, 0), bands[2].Margin);
        Assert.Equal(new[] { 56d, 40d, 56d, 40d }, bands.Select(band => band.Bounds.Height));
        Assert.Equal(0, bands[1].Bounds.Top - bands[0].Bounds.Bottom);
        Assert.Equal(4, bands[2].Bounds.Top - bands[1].Bounds.Bottom);
        Assert.Equal(0, bands[3].Bounds.Top - bands[2].Bounds.Bottom);
        Assert.Equal(196, editor.FindControl<StackPanel>("RolePreview")!.Bounds.Height);
    }

    [AvaloniaFact]
    public void HarmonizeResultsStayInlineAtNarrowAndWideWidthsWithoutDescriptiveHeader()
    {
        var entry = new SchemePlaygroundViewModel().CustomColors[0];
        entry.IsExpanded = true;
        var editor = new CustomColorEditor { DataContext = entry };
        using var host = Show(editor);
        var row = editor.FindControl<Grid>("HarmonizeRow")!;
        var label = editor.FindControl<TextBlock>("HarmonizeLabel")!;
        var preview = editor.FindControl<StackPanel>("HarmonizedPreview")!;
        var state = editor.FindControl<TextBlock>("HarmonizeStateLabel")!;
        var toggle = editor.FindControl<ToggleSwitch>("HarmonizeToggle")!;
        Assert.Same(row, preview.Parent);
        Assert.Null(editor.FindControl<TextBlock>("ThemeLabel"));
        Assert.DoesNotContain(editor.GetVisualDescendants().OfType<TextBlock>(), block => block.Text == "Role preview");

        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        foreach (var width in new[] { 340d, 480d })
        {
            host.Window.RequestedThemeVariant = theme;
            host.Window.Width = width;
            foreach (var harmonize in new[] { true, false, true })
            {
                entry.Harmonize = harmonize;
                host.Window.UpdateLayout();
                Assert.Equal(harmonize, preview.IsVisible);
                Assert.True(toggle.Bounds.Right <= row.Bounds.Width);
                Assert.True(state.Bounds.Right <= toggle.Bounds.Left);
                Assert.Equal(label.Bounds.Center.Y, toggle.Bounds.Center.Y, 0);
                if (harmonize)
                {
                    Assert.InRange(preview.Bounds.Left - label.Bounds.Right, 7, 9);
                    Assert.True(preview.Bounds.Right <= state.Bounds.Left);
                    Assert.Equal(label.Bounds.Center.Y, preview.Bounds.Center.Y, 0);
                }
                AssertRoles(editor, entry);
            }
        }
    }

    [AvaloniaFact]
    public void VectorIconButtonsExposeActionsAndSupportRepeatedKeyboardActivation()
    {
        var model = new SchemePlaygroundViewModel();
        var entry = model.CustomColors[0];
        var editor = new CustomColorEditor { DataContext = entry };
        using var host = Show(editor);
        var expand = editor.FindControl<Button>("ExpandButton")!;
        var remove = editor.FindControl<Button>("RemoveButton")!;
        var icon = editor.FindControl<PathIcon>("ExpandIcon")!;
        Assert.IsType<PathIcon>(remove.Content);
        Assert.Same(icon, expand.Content);
        foreach (var button in new[] { expand, remove })
        {
            Assert.True(button.Bounds.Width >= 32);
            Assert.True(button.Bounds.Height >= 32);
            Assert.True(button.Focusable);
            Assert.True(button.IsTabStop);
            Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(button)));
            Assert.NotNull(ToolTip.GetTip(button));
        }
        Assert.True(expand.Focus(NavigationMethod.Tab));
        Assert.True(expand.IsFocused);
        for (var i = 0; i < 4; i++)
        {
            var wasExpanded = entry.IsExpanded;
            host.Window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
            host.Window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);
            host.Window.UpdateLayout();
            Assert.Equal(!wasExpanded, entry.IsExpanded);
            Assert.Equal(entry.IsExpanded, icon.Classes.Contains("expanded"));
            var action = entry.IsExpanded ? "Collapse custom color" : "Expand custom color";
            Assert.Equal(action, AutomationProperties.GetName(expand));
            Assert.Equal(action, ToolTip.GetTip(expand));
            Assert.True(expand.IsFocused);
        }
        entry.IsExpanded = true;
        host.Window.UpdateLayout();
        var toggle = editor.FindControl<ToggleSwitch>("HarmonizeToggle")!;
        Assert.True(toggle.Focus(NavigationMethod.Tab));
        host.Window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        host.Window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Assert.False(entry.Harmonize);
        Assert.True(remove.Focus(NavigationMethod.Tab));
        host.Window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        host.Window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Assert.Empty(model.CustomColors);
        Assert.Empty(model.Scheme!.CustomColors);
    }

    [AvaloniaFact]
    public void DetachedEditorIsCollectedWhileItsEntryAndSchemeStayAlive()
    {
        var model = new SchemePlaygroundViewModel();
        var weakEditor = CreateDetachedEditor(model.CustomColors[0]);
        Dispatcher.UIThread.RunJobs();
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.False(weakEditor.TryGetTarget(out _));
        GC.KeepAlive(model);
    }

    [AvaloniaFact]
    public void ReattachedEditorUsesCurrentEntryNameAndSchemeAfterDetachedChanges()
    {
        var model = new SchemePlaygroundViewModel();
        var entry = model.CustomColors[0];
        var editor = new CustomColorEditor { DataContext = entry };
        var panel = new StackPanel { Children = { editor } };
        using var host = Show(panel);
        AssertRoles(editor, entry);

        for (var i = 0; i < 3; i++)
        {
            panel.Children.Remove(editor);
            entry.Name = $"Renamed {i}";
            model.SelectedHct = new HctSelection(90 + i * 45, 60, 55);
            panel.Children.Add(editor);
            host.Window.UpdateLayout();
            AssertRoles(editor, entry);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<CustomColorEditor> CreateDetachedEditor(CustomColorEntryViewModel entry)
    {
        var editor = new CustomColorEditor { DataContext = entry };
        var panel = new StackPanel { Children = { editor } };
        using var host = Show(panel);
        panel.Children.Remove(editor);
        return new WeakReference<CustomColorEditor>(editor);
    }

    private static Border[] Bands(CustomColorEditor editor) =>
        new[] { "ColorBand", "OnColorBand", "ContainerBand", "OnContainerBand" }
            .Select(name => editor.FindControl<Border>(name)!).ToArray();

    private static Color[] BandColors(CustomColorEditor editor) =>
        Bands(editor).Select(band => MaterialColorTestHelper.BrushColor(band.Background)).ToArray();

    private static void AssertRoles(CustomColorEditor editor, CustomColorEntryViewModel entry)
    {
        var source = ArgbColor.FromAvaloniaColor(entry.SourceColor);
        if (entry.Harmonize)
            source = global::MaterialColorUtilities.Blend.Blend.Harmonize(source,
                ArgbColor.FromAvaloniaColor(entry.Scheme!.Color!.Value));
        var palette = new TonalPalette(Hct.From(source));
        var dark = ColorScheme.IsDark(editor.ActualThemeVariant);
        var tones = dark ? new[] { 80, 20, 30, 90 } : new[] { 40, 100, 90, 10 };
        var expected = tones.Select(tone => palette.Get(tone).ToAvaloniaColor()).ToArray();
        Assert.Equal(expected, BandColors(editor));
        var labels = new[] { "ColorLabel", "OnColorLabel", "ContainerLabel", "OnContainerLabel" };
        var foregrounds = labels.Select(name => MaterialColorTestHelper.BrushColor(editor.FindControl<TextBlock>(name)!.Foreground));
        Assert.Equal(new[] { expected[1], expected[0], expected[3], expected[2] }, foregrounds);
    }

    private static WindowHost Show(Control content) => new(content);

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Control content)
        {
            Window = new Window { Content = content, Width = 1000, Height = 1600, RequestedThemeVariant = ThemeVariant.Light };
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }
        public void Dispose() => Window.Close();
    }
}
