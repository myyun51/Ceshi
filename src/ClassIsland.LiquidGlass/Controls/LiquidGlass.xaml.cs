using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClassIsland.LiquidGlass.Controls;

/// <summary>
/// A WPF liquid-glass surface whose tint and foreground are selected from the
/// luminance of the pixels directly under the control. Set <see cref="BackdropSource"/>
/// to the visual that paints the component background.
/// </summary>
public class LiquidGlass : ContentControl
{
    private readonly DispatcherTimer _refreshTimer;
    private bool _refreshQueued;
    private Image? _backdropImage;

    static LiquidGlass()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LiquidGlass), new FrameworkPropertyMetadata(typeof(LiquidGlass)));
    }

    public LiquidGlass()
    {
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33),
        };
        _refreshTimer.Tick += (_, _) => Refresh();
        Loaded += OnLoaded;
        Unloaded += (_, _) => _refreshTimer.Stop();
        SizeChanged += (_, _) => QueueRefresh();
    }

    /// <summary>The host visual containing only the component background (not this control).</summary>
    public FrameworkElement? BackdropSource
    {
        get => (FrameworkElement?)GetValue(BackdropSourceProperty);
        set => SetValue(BackdropSourceProperty, value);
    }

    public static readonly DependencyProperty BackdropSourceProperty = DependencyProperty.Register(
        nameof(BackdropSource), typeof(FrameworkElement), typeof(LiquidGlass),
        new PropertyMetadata(null, (d, _) => ((LiquidGlass)d).QueueRefresh()));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(LiquidGlass), new PropertyMetadata(new CornerRadius(18)));

    /// <summary>Strength of the softened background, in device-independent pixels.</summary>
    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
        nameof(BlurRadius), typeof(double), typeof(LiquidGlass), new PropertyMetadata(14d, (d, _) => ((LiquidGlass)d).QueueRefresh()));

    /// <summary>White surface alpha over a dark backdrop; the inverse is used over a light backdrop.</summary>
    public double SurfaceOpacity
    {
        get => (double)GetValue(SurfaceOpacityProperty);
        set => SetValue(SurfaceOpacityProperty, value);
    }

    public static readonly DependencyProperty SurfaceOpacityProperty = DependencyProperty.Register(
        nameof(SurfaceOpacity), typeof(double), typeof(LiquidGlass), new PropertyMetadata(.32d, (d, _) => ((LiquidGlass)d).UpdateAdaptiveColors()));

    public Brush AdaptiveBrightnessBrush
    {
        get => (Brush)GetValue(AdaptiveBrightnessBrushProperty);
        private set => SetValue(AdaptiveBrightnessBrushKey, value);
    }

    private static readonly DependencyPropertyKey AdaptiveBrightnessBrushKey = DependencyProperty.RegisterReadOnly(
        nameof(AdaptiveBrightnessBrush), typeof(Brush), typeof(LiquidGlass), new PropertyMetadata(Brushes.White));
    public static readonly DependencyProperty AdaptiveBrightnessBrushProperty = AdaptiveBrightnessBrushKey.DependencyProperty;

    public Brush RimBrush
    {
        get => (Brush)GetValue(RimBrushProperty);
        private set => SetValue(RimBrushKey, value);
    }

    private static readonly DependencyPropertyKey RimBrushKey = DependencyProperty.RegisterReadOnly(
        nameof(RimBrush), typeof(Brush), typeof(LiquidGlass), new PropertyMetadata(Brushes.White));
    public static readonly DependencyProperty RimBrushProperty = RimBrushKey.DependencyProperty;

    public Thickness RimThickness
    {
        get => (Thickness)GetValue(RimThicknessProperty);
        private set => SetValue(RimThicknessProperty, value);
    }

    public static readonly DependencyProperty RimThicknessProperty = DependencyProperty.Register(
        nameof(RimThickness), typeof(Thickness), typeof(LiquidGlass), new PropertyMetadata(new Thickness(1)));

    /// <summary>Use this on text/icons in the content template for contrast selected from the backdrop.</summary>
    public Brush AdaptiveForeground
    {
        get => (Brush)GetValue(AdaptiveForegroundProperty);
        private set => SetValue(AdaptiveForegroundKey, value);
    }

    private static readonly DependencyPropertyKey AdaptiveForegroundKey = DependencyProperty.RegisterReadOnly(
        nameof(AdaptiveForeground), typeof(Brush), typeof(LiquidGlass), new PropertyMetadata(Brushes.Black));
    public static readonly DependencyProperty AdaptiveForegroundProperty = AdaptiveForegroundKey.DependencyProperty;

    private double _luminance = .5;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _backdropImage = GetTemplateChild("PART_BackdropImage") as Image;
        QueueRefresh();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _refreshTimer.Start();
        QueueRefresh();
    }

    private void QueueRefresh()
    {
        if (!IsLoaded || _refreshQueued)
            return;

        _refreshQueued = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
        {
            _refreshQueued = false;
            Refresh();
        }));
    }

    private void Refresh()
    {
        if (BackdropSource is not { IsLoaded: true } source || ActualWidth < 1 || ActualHeight < 1)
            return;

        try
        {
            var scale = VisualTreeHelper.GetDpi(source);
            var sourceWidth = Math.Max(1, (int)Math.Ceiling(source.ActualWidth * scale.DpiScaleX));
            var sourceHeight = Math.Max(1, (int)Math.Ceiling(source.ActualHeight * scale.DpiScaleY));
            var full = new RenderTargetBitmap(sourceWidth, sourceHeight, 96 * scale.DpiScaleX, 96 * scale.DpiScaleY, PixelFormats.Pbgra32);
            full.Render(source);

            var position = TranslatePoint(new Point(0, 0), source);
            var x = Math.Clamp((int)Math.Floor(position.X * scale.DpiScaleX), 0, sourceWidth - 1);
            var y = Math.Clamp((int)Math.Floor(position.Y * scale.DpiScaleY), 0, sourceHeight - 1);
            var width = Math.Clamp((int)Math.Ceiling(ActualWidth * scale.DpiScaleX), 1, sourceWidth - x);
            var height = Math.Clamp((int)Math.Ceiling(ActualHeight * scale.DpiScaleY), 1, sourceHeight - y);
            var crop = new CroppedBitmap(full, new Int32Rect(x, y, width, height));
            crop.Freeze();
            if (_backdropImage is null)
                return;

            _backdropImage.Source = crop;
            _backdropImage.Effect = new BlurEffect { Radius = BlurRadius, RenderingBias = RenderingBias.Performance };
            _luminance = CalculateLuminance(crop);
            UpdateAdaptiveColors();
        }
        catch (InvalidOperationException)
        {
            // The host visual can be transiently disconnected during a component reload.
        }
    }

    private void UpdateAdaptiveColors()
    {
        // Relative luminance follows sRGB weights. The threshold ensures at least
        // readable contrast while retaining AndroidLiquidGlass-style adaptive tinting.
        var isDark = _luminance < .52;
        var alpha = (byte)Math.Clamp(Math.Round(SurfaceOpacity * 255), 0, 255);
        AdaptiveBrightnessBrush = new SolidColorBrush(isDark
            ? Color.FromArgb(alpha, 255, 255, 255)
            : Color.FromArgb(alpha, 16, 20, 28));
        RimBrush = new SolidColorBrush(isDark ? Color.FromArgb(150, 255, 255, 255) : Color.FromArgb(90, 0, 0, 0));
        AdaptiveForeground = new SolidColorBrush(isDark ? Color.FromRgb(250, 250, 252) : Color.FromRgb(20, 23, 30));
    }

    private static double CalculateLuminance(BitmapSource bitmap)
    {
        const int sampleStride = 8;
        var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
        double total = 0;
        var count = 0;
        for (var y = 0; y < bitmap.PixelHeight; y += sampleStride)
        for (var x = 0; x < bitmap.PixelWidth; x += sampleStride)
        {
            var offset = (y * bitmap.PixelWidth + x) * 4;
            var blue = pixels[offset] / 255d;
            var green = pixels[offset + 1] / 255d;
            var red = pixels[offset + 2] / 255d;
            total += .2126 * red + .7152 * green + .0722 * blue;
            count++;
        }
        return count == 0 ? .5 : total / count;
    }
}
