using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CircleCollider : Collider
{
    Vector2 center;
    float radius;


    public override Vector2 Center => center;

    public override Vector2 Offset => center;

    public CircleCollider(Drawable drawable, Vector2 center, float radius) : base(drawable)
    {
        this.center = center;
        this.radius = radius;
    }

    public override bool Raycast(Vector2 point)
    {
        return Vector2.DistanceSquared(point, drawable.GetGlobalPosition() + center) <= radius * radius;
    }


    public override RectangleF GetBounds()
    {
        RectangleF boundsRect = new RectangleF(center.X, center.Y, radius * 2, radius * 2);
        boundsRect.Location += drawable.GetGlobalPosition();
        return boundsRect;
    }
    public override void Draw(RenderTarget renderTarget, Vector2 tanslation)
    {

        renderTarget.DrawEllipse(new Ellipse(center + drawable.GetGlobalPosition() + tanslation, radius, radius), new SolidColorBrush(renderTarget, Colors.Green), 0.5f);
    }

}
