using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct Triangle
{
    Vector2 a, b, c;
    public Vector2 A { get => a; }
    public Vector2 B { get => b; }
    public Vector2 C { get => c; }
    public float left => Math.Min(Math.Min(a.X, b.X), c.X);
    public float right => Math.Max(Math.Max(a.X, b.X), c.X);
    public float top => Math.Min(Math.Min(a.Y, b.Y), c.Y);
    public float bottom => Math.Max(Math.Max(a.Y, b.Y), c.Y);
    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
    public void Draw(RenderTarget renderTarget, Vector2 translation, SolidColorBrush brush) {
        renderTarget.DrawLine(a + translation, b + translation, brush, 0.5f);
        renderTarget.DrawLine(b + translation, c + translation, brush, 0.5f);
        renderTarget.DrawLine(c + translation, a + translation, brush, 0.5f);
    }
    public bool Contains(Vector2 p)
    {
        var s = a.Y * c.X - a.X * c.Y + (c.Y - a.Y) * p.X + (a.X - c.X) * p.Y;
        var t = a.X * b.Y - a.Y * b.X + (a.Y - b.Y) * p.X + (b.X - a.X) * p.Y;

        if ((s < 0) != (t < 0))
            return false;

        var A = -b.Y * c.X + a.Y * (c.X - b.X) + a.X * (b.Y - c.Y) + b.X * c.Y;

        return A < 0 ?
                (s <= 0 && s + t >= A) :
                (s >= 0 && s + t <= A);
    }

    public Vector2 this[int index]
    {
        get {
            if(index > 2) throw new IndexOutOfRangeException();
            if (index == 0) return a; else if (index == 1) return b; else return c;
        }
        set {
            if(index > 2) throw new IndexOutOfRangeException();
            if (index == 0) a = value; else if (index == 1) a = value; else c = value;
            /* set the specified index to value here */
        }
    }
}
