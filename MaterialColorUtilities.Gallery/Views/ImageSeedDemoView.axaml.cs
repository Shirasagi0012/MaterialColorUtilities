using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MaterialColorUtilities.Avalonia;

namespace MaterialColorUtilities.Gallery.Views;

public partial class ImageSeedDemoView : UserControl
{
    private readonly ImageColorSource _source;
    private Bitmap? _ownedBitmap;
    private string? _selectedAsset = "image-seed-coast.png";

    public ImageSeedDemoView()
    {
        InitializeComponent();
        Resources.TryGetResource("DemoImageColors", null, out var source);
        _source = (ImageColorSource)source!;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        LoadSelectedImage();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // ImageColorSource never owns or disposes the input. Clear its queued request
        // and the Image binding before disposing our Bitmap, on the UI thread.
        ReplaceImage(null);
        base.OnDetachedFromVisualTree(e);
    }

    private void ShowCoast(object? sender, RoutedEventArgs e) => SelectImage("image-seed-coast.png");
    private void ShowDusk(object? sender, RoutedEventArgs e) => SelectImage("image-seed-dusk.png");
    private void ClearImage(object? sender, RoutedEventArgs e) => SelectImage(null);

    private void SelectImage(string? asset)
    {
        _selectedAsset = asset;
        LoadSelectedImage();
    }

    private void LoadSelectedImage()
    {
        if (_selectedAsset is null)
        {
            ReplaceImage(null);
            return;
        }

        // These small deterministic raster assets are bundled with the Gallery.
        // Loading stays with the caller; the color source only receives a Bitmap.
        using var stream = AssetLoader.Open(new Uri(
            $"avares://MaterialColorUtilities.Gallery/Assets/{_selectedAsset}"));
        ReplaceImage(new Bitmap(stream));
    }

    private void ReplaceImage(Bitmap? next)
    {
        var previous = _ownedBitmap;
        _ownedBitmap = next;
        _source.Image = next;
        previous?.Dispose();
    }
}
