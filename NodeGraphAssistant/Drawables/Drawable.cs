
using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Forms;

public abstract class Drawable : IDestroy
{
    protected RectangleF boundingBox;
    protected Drawable parent;
    protected Collider collider;
    protected ContextMenuStrip contextMenu;
    protected bool reciveRaycasts = true;
    
    public RectangleF BoundingBox { get => boundingBox;}

    public Collider Collider { get => collider;}
    protected Drawable Parent { get => parent; set => parent = value; }
    public ContextMenuStrip ContextMenu => contextMenu;
    public Drawable(Drawable parent) {
        this.parent = parent;
    }
    virtual public void Draw(RenderTarget renderTarget, Vector2 translation) { }
    virtual public void RecalculateBounds() { }
    virtual public void Update() { }
    virtual public void OnMouseEnter(MouseEventArgs e, Collider collider) { }
    virtual public void OnMouseExit(MouseEventArgs e, Collider collider) { }
    virtual public void OnMouseHover(MouseEventArgs e, Collider collider) { }
    virtual public void OnMouseDown(MouseEventArgs e, Collider collider) { }
    virtual public void OnMouseUp(MouseEventArgs e, Collider collider) { }
    virtual public void OnMouseDoubleClick(MouseEventArgs e, Collider collider) { }
    public void SetLocation(Vector2 location)
    {
        boundingBox.Location = location;
    }
    public void Translate(Vector2 translation)
    {
        boundingBox.Location += translation;
    }
    public Vector2 GetGlobalPosition()
    {
        if (parent == null) return boundingBox.Location;
        Vector2 globalPosition = boundingBox.Location;
        Drawable upperParent = parent;
        while (upperParent != null) {
            globalPosition += upperParent.boundingBox.Location;
            upperParent = upperParent.parent;
        }
        return globalPosition;
    }
    public virtual void Destroy() {
        Program.canvas.Drawbles.Remove(this);
        if (collider != null) {
            collider.Destroy();
            collider = null;
        }
    }
}
