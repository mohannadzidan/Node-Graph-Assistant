using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class Utils
{
    public static SharpDX.DirectWrite.Factory defaultWriteFactory = new SharpDX.DirectWrite.Factory();
    public static TextFormat elementTextFormatDefault = new TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", 12f);
    public static TextFormat elementTextFormatSmall = new TextFormat(new SharpDX.DirectWrite.Factory(), "Arial", 10f);
    public static System.Drawing.SizeF MeasureString(string str, TextFormat format)
    {
        TextLayout l = new TextLayout(defaultWriteFactory, str, format, float.MaxValue, float.MaxValue);
        float h = l.Metrics.Height;
        float w = l.Metrics.Width;
        l.Dispose();
        return new System.Drawing.SizeF(w, h);
    }
    public static float MeasureStringHeight(string str, TextFormat format, float maxWidth)
    {
        TextLayout l = new TextLayout(defaultWriteFactory, str, format, maxWidth, float.MaxValue);
        float h = l.Metrics.Height;
        l.Dispose();
        return h;
    }
    public static RectangleF StringBoundingBox(string str, TextFormat format) {
        TextLayout l = new TextLayout(defaultWriteFactory, str, format, float.MaxValue, float.MaxValue);
        float h = l.Metrics.Height;
        float w = l.Metrics.Width;
        l.Dispose();
        return new RectangleF(0, 0, w, h);
    }
    public static float MeasureStringWidth(string str, TextFormat format)
    {
        TextLayout l = new TextLayout(defaultWriteFactory, str, format, float.MaxValue, float.MaxValue);
        float w = l.Metrics.Width;
        l.Dispose();
        return w;
    }
    public static float MeasureStringHeight(string str, TextFormat format)
    {
        TextLayout l = new TextLayout(defaultWriteFactory, str, format, float.MaxValue, float.MaxValue);
        float h = l.Metrics.Height;
        l.Dispose();
        return h;
    }
   
    /// <summary>
    /// Loads a Direct2D Bitmap from a file using System.Drawing.Image.FromFile(...)
    /// </summary>
    /// <param name="renderTarget">The render target.</param>
    /// <param name="file">The file.</param>
    /// <returns>A D2D1 Bitmap</returns>
    public static Bitmap LoadFromFile(RenderTarget renderTarget, System.Drawing.Size newSize, string file)
    {
        // Loads from file using System.Drawing.Image
        System.Drawing.Bitmap bitmap;

        int width, height;
        if (newSize.IsEmpty)
        {
            bitmap = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(file));
        }
        else if (newSize.Width == 0)
        {
            //scale to height
            bitmap = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(file));
            height = newSize.Height;
            float ratio = bitmap.Width / (float)bitmap.Height;
            width = (int)(height * ratio);
            bitmap = new System.Drawing.Bitmap(bitmap, width, height);
        }
        else if (newSize.Height == 0)
        {
            bitmap = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(file));
            width = newSize.Width;
            float ratio = bitmap.Height / (float)bitmap.Width;
            height = (int)(width * ratio);
            bitmap = new System.Drawing.Bitmap(bitmap, width, height);
        }
        else
        {
            bitmap = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(file), newSize);
        }

        var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));
        var size = new Size2(bitmap.Width, bitmap.Height);

        // Transform pixels from BGRA to RGBA
        int stride = bitmap.Width * sizeof(int);
        using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
        {
            // Lock System.Drawing.Bitmap
            var bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            // Convert all pixels 
            for (int y = 0; y < bitmap.Height; y++)
            {
                int offset = bitmapData.Stride * y;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Not optimized 
                    byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    int rgba = R | (G << 8) | (B << 16) | (A << 24);
                    tempStream.Write(rgba);
                }

            }
            bitmap.UnlockBits(bitmapData);
            tempStream.Position = 0;

            return new Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
        }
    }
}