using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Collider : IDestroy
{
    protected bool recivesRaycasts = true;
    protected Drawable drawable;
    public abstract Vector2 Center { get; }
    public abstract Vector2 Offset { get; }
   
    public Collider(Drawable drawable, bool recivesRaycasts = true)
    {
        this.drawable = drawable;
        this.recivesRaycasts = recivesRaycasts;
        Physics.colliders.Add(this);
    }
   
    public Drawable Drawable { get => drawable; set => drawable = value; }
    public bool RecivesRaycasts { get => recivesRaycasts; set => recivesRaycasts = value; }

    public abstract bool Raycast(Vector2 point);
    public abstract RectangleF GetBounds();
    public abstract void Draw(RenderTarget renderTarget, Vector2 tanslation);
    public void Destroy() {
        Physics.colliders.Remove(this);
    }

}
