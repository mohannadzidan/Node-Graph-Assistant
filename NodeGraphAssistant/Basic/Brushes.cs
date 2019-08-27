using SharpDX;
using SharpDX.Direct2D1;


public static class Brushes
{
    public static SolidColorBrush White;
    public static SolidColorBrush DarkGrey;
    public static SolidColorBrush Collider;
    public static SolidColorBrush Grid;
    public static SolidColorBrush NodeBackground;
    public static SolidColorBrush HeaderBackground;
    public static SolidColorBrush Accent;
    public static SolidColorBrush HeaderText;
    public static SolidColorBrush Ring;
    public static SolidColorBrush Wire;
    public static SolidColorBrush Highlight;
    public static SolidColorBrush Selection;
    public static void Initialize(RenderTarget renderTarget) {
        White = new SolidColorBrush(renderTarget, Colors.White);
        DarkGrey = new SolidColorBrush(renderTarget, new Color4(0.105f, 0.105f, 0.105f, 1f));
        Collider = new SolidColorBrush(renderTarget, Colors.Green);
        Grid = new SolidColorBrush(renderTarget, Colors.GridColor);
        NodeBackground = new SolidColorBrush(renderTarget, Colors.DeafultNodeBackgroundColor);
        HeaderBackground = new SolidColorBrush(renderTarget, Colors.DeafultNodeBackgroundColor); ;
        Accent = new SolidColorBrush(renderTarget, Colors.Accent);
        HeaderText = new SolidColorBrush(renderTarget, Colors.White);
        Ring = new SolidColorBrush(renderTarget, Colors.White);
        Wire = new SolidColorBrush(renderTarget, Colors.White);
        Highlight = new SolidColorBrush(renderTarget, new Color4(Colors.Accent.Red, Colors.Accent.Green, Colors.Accent.Blue, 0.5f));
        Selection = new SolidColorBrush(renderTarget, new Color4(Colors.Accent.Red, Colors.Accent.Green, Colors.Accent.Blue, 0.35f));
    }
    public static LinearGradientBrush MakeLinearGradientBrush(RenderTarget renderTarget, Color4 startColor, Color4 endColor) {
        GradientStop[] stops = new GradientStop[] {
            new GradientStop(){ Color = startColor, Position = 0},
            new GradientStop(){ Color = endColor, Position = 1f}
        };
        return new LinearGradientBrush(renderTarget, new LinearGradientBrushProperties(), new GradientStopCollection(renderTarget, stops));
    }
}
