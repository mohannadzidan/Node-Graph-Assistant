using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

public class RectangleCollider : Collider
{
    public RectangleF rect;

    public override Vector2 Center => rect.Center;
    public override Vector2 Offset => rect.Location;

    public RectangleCollider(Drawable drawable, RectangleF rect) : base(drawable)
    {
        this.rect = rect;
    }
    public override RectangleF GetBounds()
    {
        return rect;
    }

    public override bool Raycast(Vector2 point)
    {
        RectangleF translatedRect = rect;
        translatedRect.Location = rect.Location + drawable.GetGlobalPosition();
        return translatedRect.Contains(point);
    }

    public override void Draw(RenderTarget renderTarget, Vector2 tanslation)
    {
        RectangleF backgroundRect = rect;
        backgroundRect.Location = drawable.GetGlobalPosition() + tanslation;
        renderTarget.DrawRectangle(backgroundRect, Brushes.Collider);
    }
}
