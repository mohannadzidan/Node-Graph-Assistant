using System;
using SharpDX;
using SharpDX.Direct2D1;

public class MeshCollider : Collider
{
    Triangle[] triangles;
    RectangleF bounds;
    public Triangle[] Triangles
    {
        get
        {
            return triangles;
        }
        set
        {
            triangles = value;
            CalculateBounds();
        }
    }
    public MeshCollider(Drawable drawable) : base(drawable)
    {
        triangles = new Triangle[0];
    }
    public MeshCollider(Drawable drawable, Triangle[] triangles) : base(drawable)
    {
        this.triangles = triangles;
    }
    public override Vector2 Center => throw new NotImplementedException();

    public override Vector2 Offset => Vector2.Zero;
    public override void Draw(RenderTarget renderTarget, Vector2 tanslation)
    {
        foreach (Triangle t in Triangles) t.Draw(renderTarget, Offset + drawable.GetGlobalPosition() + tanslation, Brushes.Collider);
    }
    void CalculateBounds()
    {
        float maxY = float.MinValue, maxX = float.MinValue, minY = float.MaxValue, minX = float.MaxValue;
        foreach (Triangle t in triangles)
        {
            float tMaxY = float.MinValue, tMaxX = float.MinValue, tMinY = float.MaxValue, tMinX = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                if (t[i].X < tMinX) tMinX = t[i].X;
                else if (t[i].X > tMaxX) tMaxX = t[i].X;
                if (t[i].Y < tMinY) tMinY = t[i].X;
                else if (t[i].Y > tMaxY) tMaxY = t[i].X;
            }
            minX = Math.Min(minX, tMinX);
            maxX = Math.Max(maxX, tMaxX);
            minY = Math.Min(minY, tMinY);
            maxY = Math.Max(maxY, tMaxY);
        }
        bounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }
    public override RectangleF GetBounds()
    {
        return new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    public override bool Raycast(Vector2 point)
    {

        point -= drawable.GetGlobalPosition();
        foreach (Triangle t in Triangles)
        {
            if (t.Contains(point)) return true;
        }
        return false;
    }
}
