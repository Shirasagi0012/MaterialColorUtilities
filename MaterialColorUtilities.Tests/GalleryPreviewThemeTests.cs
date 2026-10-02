using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using MaterialColorUtilities.Gallery.ViewModels;
using MaterialColorUtilities.Gallery.Views;
using Xunit;

namespace MaterialColorUtilities.Tests;

public class GalleryPreviewThemeTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void WatchPreviewIsDark_AndPhoneRestoresInheritedTheme(bool startDark)
    {
        var globalTheme = startDark ? ThemeVariant.Dark : ThemeVariant.Light;
        var model = new SchemePlaygroundViewModel();
        model.SelectedSpecOption = model.SpecOptions.Single(option => option.Version == ColorSpec.SpecVersion.Spec2025);
        var view = new SchemePlaygroundView { DataContext = model };
        var window = new Window { Content = view, RequestedThemeVariant = globalTheme };
        try
        {
            window.Show();
            var preview = view.FindControl<ThemeVariantScope>("SchemePreview")!;
            Assert.Equal(ThemeVariant.Default, preview.RequestedThemeVariant);
            Assert.Equal(globalTheme, preview.ActualThemeVariant);
            AssertPreviewPrimary(preview, model, globalTheme);

            // Repeat transitions and change the parent theme while Watch is active.
            for (var i = 0; i < 2; i++)
            {
                model.SelectedPlatformOption = SchemePlaygroundViewModel.PlatformOptions.Single(
                    option => option.Platform == DynamicScheme.Platform.Watch);
                Assert.Equal(ThemeVariant.Dark, preview.RequestedThemeVariant);
                Assert.Equal(ThemeVariant.Dark, preview.ActualThemeVariant);
                AssertPreviewPrimary(preview, model, ThemeVariant.Dark);
                Assert.Equal(globalTheme, view.ActualThemeVariant);
                Assert.Equal(globalTheme, window.RequestedThemeVariant);

                globalTheme = globalTheme == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;
                window.RequestedThemeVariant = globalTheme;
                Assert.Equal(ThemeVariant.Dark, preview.ActualThemeVariant);
                AssertPreviewPrimary(preview, model, ThemeVariant.Dark);
                Assert.Equal(globalTheme, view.ActualThemeVariant);

                model.SelectedPlatformOption = SchemePlaygroundViewModel.PlatformOptions.Single(
                    option => option.Platform == DynamicScheme.Platform.Phone);
                Assert.Equal(ThemeVariant.Default, preview.RequestedThemeVariant);
                Assert.Equal(globalTheme, preview.ActualThemeVariant);
                AssertPreviewPrimary(preview, model, globalTheme);
                Assert.Equal(globalTheme, window.RequestedThemeVariant);
            }
        }
        finally { window.Close(); }
    }

    private static void AssertPreviewPrimary(ThemeVariantScope preview, SchemePlaygroundViewModel model, ThemeVariant theme)
    {
        var border = Assert.IsType<Border>(preview.Child);
        Assert.Equal(MaterialColorTestHelper.Primary(model.Scheme!, theme),
            MaterialColorTestHelper.ReadColor(border, SysColorToken.Primary, border.ActualThemeVariant));
    }
}
