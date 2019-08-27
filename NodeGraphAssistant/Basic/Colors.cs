using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Colors
{
    public static readonly SharpDX.Color4 White = SharpDX.Color4.White;
    public static readonly SharpDX.Color4 Black = SharpDX.Color4.White;
    public static readonly SharpDX.Color4 Red = new SharpDX.Color4(1, 0, 0, 1f);
    public static readonly SharpDX.Color4 Green = new SharpDX.Color4(0, 1, 0, 1f);
    public static readonly SharpDX.Color4 Blue = new SharpDX.Color4(0, 0, 1, 1f);
    public static readonly SharpDX.Color4 Grey = new SharpDX.Color4(0.5f, 0.5f, 0.5f, 1);
    public static readonly SharpDX.Color4 DarkBlue = new SharpDX.Color4(0.047f, 0.368f, 0.650f, 1f);
    public static readonly SharpDX.Color4 DarkRed = new SharpDX.Color4(0.419f, 0.098f, 0.098f, 1f);
    public static readonly SharpDX.Color4 DarkGrey = new SharpDX.Color4(0.25f,0.25f, 0.25f, 1);
    public static readonly SharpDX.Color4 LightGrey = new SharpDX.Color4(0.75f, 0.75f, 0.75f, 1f);
    public static readonly SharpDX.Color4 BackgroundColor = new SharpDX.Color4(0.15f, 0.15f, 0.15f, 1f);
    public static readonly SharpDX.Color4 GridColor = new SharpDX.Color4(0, 0, 0, 1f);
    public static readonly SharpDX.Color4 DeafultNodeBackgroundColor = new SharpDX.Color4(0.3f, 0.3f, 0.3f, 0.6f);
    public static readonly SharpDX.Color4 WireColor = new SharpDX.Color4(1f, 1f, 1f, 0.35f);
    public static readonly SharpDX.Color4 Accent = new SharpDX.Color4(1, 0.5f, 0, 1f);

    public static System.Drawing.Color ToSystemARGB(SharpDX.Color4 c) {
        return System.Drawing.Color.FromArgb((int)(c.Alpha * 255), (int)(c.Red * 255), (int)(c.Green * 255), (int) (c.Blue * 255));
    }
}
