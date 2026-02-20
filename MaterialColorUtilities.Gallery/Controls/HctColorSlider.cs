using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MaterialColorUtilities.Avalonia;
using HctColor = MaterialColorUtilities.HCT.Hct;

namespace MaterialColorUtilities.Gallery.Controls;

[PseudoClasses(pcDarkSelector, pcLightSelector)]
public class HctColorSlider : Slider
{
    private const string pcDarkSelector = ":dark-selector";
    private const string pcLightSelector = ":light-selector";

    private const double MaxHue = 359.0;
    private const double MaxTone = 100.0;
    private const double MaxChromaProbe = 200.0;
    private const double HueTolerance = 1.0;
    private const double ToneTolerance = 0.2;
    private const int ChromaSearchIterations = 16;

    private static readonly HctSelection DefaultHct = HctSelection.FromHct(HctColor.FromAvaloniaColor(Colors.White));

    private Bitmap? _backgroundBitmap;
    private bool _ignorePropertyChanged;

    public static readonly StyledProperty<HctSelection> HctProperty =
        AvaloniaProperty.Register<HctColorSlider, HctSelection>(
            nameof(Hct),
            DefaultHct,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<HctComponent> HctComponentProperty =
        AvaloniaProperty.Register<HctColorSlider, HctComponent>(
            nameof(HctComponent),
            HctComponent.Hue);

    protected override Type StyleKeyOverride => typeof(ColorSlider);

    public HctSelection Hct
    {
        get => GetValue(HctProperty);
        set => SetValue(HctProperty, value);
    }

    public HctComponent HctComponent
    {
        get => GetValue(HctComponentProperty);
        set => SetValue(HctComponentProperty, value);
    }

    public static double GetDynamicMaxChroma(double hue, double tone)
    {
        var safeHue = Math.Clamp(hue, 0.0, MaxHue);
        var safeTone = Math.Clamp(tone, 0.0, MaxTone);

        return Math.Max(0.0, HctColor.From(safeHue, MaxChromaProbe, safeTone).Chroma);
    }

    private void UpdatePseudoClasses()
    {
        if (ColorHelper.GetRelativeLuminance(Hct.ToHct().ToAvaloniaColor()) <= 0.5)
        {
            PseudoClasses.Set(pcDarkSelector, false);
            PseudoClasses.Set(pcLightSelector, true);
        }
        else
        {
            PseudoClasses.Set(pcDarkSelector, true);
            PseudoClasses.Set(pcLightSelector, false);
        }
    }

    private void SetHctToSliderValues()
    {
        var hct = Hct.Normalize();

        switch (HctComponent)
        {
            case HctComponent.Hue:
                Minimum = 0.0;
                Maximum = MaxHue;
                Value = Math.Clamp(hct.Hue, 0.0, MaxHue);
                break;
            case HctComponent.Chroma:
            {
                var maxChroma = GetDynamicMaxChroma(hct.Hue, hct.Tone);
                Minimum = 0.0;
                Maximum = maxChroma;
                Value = Math.Clamp(hct.Chroma, 0.0, maxChroma);
                break;
            }
            case HctComponent.Tone:
                Minimum = 0.0;
                Maximum = MaxTone;
                Value = Math.Clamp(hct.Tone, 0.0, MaxTone);
                break;
        }
    }

    private double GetSafeSliderValue()
    {
        if (Maximum <= Minimum)
        {
            return Minimum;
        }

        return Math.Clamp(Value, Minimum, Maximum);
    }

    private HctSelection GetHctFromSliderValues()
    {
        var baseHct = Hct.Normalize();
        var hue = baseHct.Hue;
        var tone = baseHct.Tone;
        var chroma = baseHct.Chroma;
        var sliderValue = GetSafeSliderValue();

        switch (HctComponent)
        {
            case HctComponent.Hue:
                hue = Math.Clamp(sliderValue, 0.0, MaxHue);
                chroma = Math.Min(chroma, GetDynamicMaxChroma(hue, tone));
                break;
            case HctComponent.Chroma:
                chroma = Math.Clamp(sliderValue, 0.0, GetDynamicMaxChroma(hue, tone));
                break;
            case HctComponent.Tone:
                tone = Math.Clamp(sliderValue, 0.0, MaxTone);
                chroma = Math.Min(chroma, GetDynamicMaxChroma(hue, tone));
                break;
        }

        return new HctSelection(hue, chroma, tone).Normalize();
    }

    private void UpdateBackground()
    {
        var scale = LayoutHelper.GetLayoutScale(this);
        int pixelWidth;
        int pixelHeight;

        if (Track is not null)
        {
            pixelWidth = Convert.ToInt32(Track.Bounds.Width * scale);
            pixelHeight = Convert.ToInt32(Track.Bounds.Height * scale);
        }
        else
        {
            pixelWidth = Convert.ToInt32(Bounds.Width * scale);
            pixelHeight = Convert.ToInt32(Bounds.Height * scale);
        }

        if (pixelWidth <= 0 || pixelHeight <= 0)
        {
            return;
        }

        var pixelData = CreateComponentPixelData(
            pixelWidth,
            pixelHeight,
            Orientation,
            HctComponent,
            Hct.Normalize());

        _backgroundBitmap?.Dispose();
        _backgroundBitmap = CreateBitmapFromPixelData(pixelData, pixelWidth, pixelHeight);
        Background = new ImageBrush(_backgroundBitmap);
    }

    private static byte[] CreateComponentPixelData(
        int width,
        int height,
        Orientation orientation,
        HctComponent component,
        HctSelection baseHct)
    {
        var bgraPixelData = new byte[width * height * 4];
        var pixelDataIndex = 0;
        var rowWidth = width * 4;
        var dimension = orientation == Orientation.Horizontal ? width : height;
        var stepDivisor = Math.Max(1, dimension - 1);

        var componentMax = component switch
        {
            HctComponent.Hue => MaxHue,
            HctComponent.Tone => MaxTone,
            HctComponent.Chroma => GetDynamicMaxChroma(baseHct.Hue, baseHct.Tone),
            _ => MaxHue
        };

        var componentStep = componentMax / stepDivisor;

        Color GetSweepColor(double value)
        {
            var hue = Math.Clamp(baseHct.Hue, 0.0, MaxHue);
            var tone = Math.Clamp(baseHct.Tone, 0.0, MaxTone);
            var chroma = Math.Max(0.0, baseHct.Chroma);

            switch (component)
            {
                case HctComponent.Hue:
                    hue = Math.Clamp(value, 0.0, MaxHue);
                    chroma = Math.Min(chroma, GetDynamicMaxChroma(hue, tone));
                    break;
                case HctComponent.Chroma:
                    chroma = Math.Clamp(value, 0.0, componentMax);
                    break;
                case HctComponent.Tone:
                    tone = Math.Clamp(value, 0.0, MaxTone);
                    chroma = Math.Min(chroma, GetDynamicMaxChroma(hue, tone));
                    break;
            }

            return HctColor.From(hue, chroma, tone).Argb.ToAvaloniaColor();
        }

        if (orientation == Orientation.Horizontal)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (y == 0)
                    {
                        WritePixel(GetSweepColor(x * componentStep), bgraPixelData, pixelDataIndex);
                    }
                    else
                    {
                        bgraPixelData[pixelDataIndex + 0] = bgraPixelData[pixelDataIndex + 0 - rowWidth];
                        bgraPixelData[pixelDataIndex + 1] = bgraPixelData[pixelDataIndex + 1 - rowWidth];
                        bgraPixelData[pixelDataIndex + 2] = bgraPixelData[pixelDataIndex + 2 - rowWidth];
                        bgraPixelData[pixelDataIndex + 3] = bgraPixelData[pixelDataIndex + 3 - rowWidth];
                    }

                    pixelDataIndex += 4;
                }
            }
        }
        else
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (x == 0)
                    {
                        WritePixel(GetSweepColor((height - 1 - y) * componentStep), bgraPixelData, pixelDataIndex);
                    }
                    else
                    {
                        bgraPixelData[pixelDataIndex + 0] = bgraPixelData[pixelDataIndex - 4];
                        bgraPixelData[pixelDataIndex + 1] = bgraPixelData[pixelDataIndex - 3];
                        bgraPixelData[pixelDataIndex + 2] = bgraPixelData[pixelDataIndex - 2];
                        bgraPixelData[pixelDataIndex + 3] = bgraPixelData[pixelDataIndex - 1];
                    }

                    pixelDataIndex += 4;
                }
            }
        }

        return bgraPixelData;
    }

    private static void WritePixel(Color color, byte[] pixelData, int index)
    {
        pixelData[index + 0] = Convert.ToByte(color.B * color.A / 255);
        pixelData[index + 1] = Convert.ToByte(color.G * color.A / 255);
        pixelData[index + 2] = Convert.ToByte(color.R * color.A / 255);
        pixelData[index + 3] = color.A;
    }

    private static Bitmap CreateBitmapFromPixelData(byte[] pixelData, int pixelWidth, int pixelHeight)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(pixelWidth, pixelHeight),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var framebuffer = bitmap.Lock();
        Marshal.Copy(pixelData, 0, framebuffer.Address, pixelData.Length);
        return bitmap;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_ignorePropertyChanged)
        {
            base.OnPropertyChanged(change);
            return;
        }

        if (change.Property == HctProperty)
        {
            _ignorePropertyChanged = true;

            var normalized = Hct.Normalize();
            if (!normalized.Equals(Hct))
            {
                SetCurrentValue(HctProperty, normalized);
            }

            SetHctToSliderValues();
            UpdateBackground();
            UpdatePseudoClasses();

            _ignorePropertyChanged = false;
        }
        else if (change.Property == HctComponentProperty ||
                 change.Property == OrientationProperty)
        {
            _ignorePropertyChanged = true;

            SetHctToSliderValues();
            UpdateBackground();
            UpdatePseudoClasses();

            _ignorePropertyChanged = false;
        }
        else if (change.Property == BoundsProperty)
        {
            _backgroundBitmap?.Dispose();
            _backgroundBitmap = null;
            UpdateBackground();
            UpdatePseudoClasses();
        }
        else if (change.Property == ValueProperty)
        {
            _ignorePropertyChanged = true;

            var newHct = GetHctFromSliderValues();
            SetCurrentValue(HctProperty, newHct);
            SetHctToSliderValues();
            UpdatePseudoClasses();

            _ignorePropertyChanged = false;
        }

        base.OnPropertyChanged(change);
    }
}
