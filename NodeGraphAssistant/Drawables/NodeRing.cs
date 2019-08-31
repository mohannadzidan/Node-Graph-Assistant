
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NGA.ChangesManagement;
using SharpDX;
using SharpDX.Direct2D1;

public class NodeRing : Drawable, IDestroy
{
    string title;
    Color4 color;
    Direction direction;
    Wire activeWire;
    bool highlight = false;
    List<Wire> connections = new List<Wire>();
    public List<Wire> Connections { get => connections; }
    public Color4 Color { get => color; set => color = value; }
    public Direction Direction { get => direction; }
    public string Title { get => title; set => title = value; }

    public NodeRing(Node parent, string title, Color4 color, Direction direction) : base(parent)
    {
        this.parent = parent;
        this.title = title;
        this.color = color;
        this.direction = direction;
        boundingBox.Width = parent.BoundingBox.Width;
        boundingBox.Height = 20;
        if (direction == Direction.Out)
        {
            collider = new CircleCollider(this, new Vector2(boundingBox.Width, boundingBox.Height / 2), 10);
        }
        else
        {
            collider = new CircleCollider(this, new Vector2(0, boundingBox.Height / 2), 10);
        }
        contextMenu = new ContextMenuStrip();
        contextMenu.Renderer = new NGAProfessionalRenderer();
        contextMenu.Items.Add("Edit", null, (object o, EventArgs e) =>
        {
            TextEditForm form = new TextEditForm(this.title, (string res) => { this.title = res; Program.MarkCanvasDirty(); Console.WriteLine(title); });
            form.Show();

        });
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Bring to top", null, (object s, EventArgs args) => {
            Change ringPosChange = new NodeRingIndex(this, 0);
            ringPosChange.Apply();
            Program.canvas.ChangesManager.Push(ringPosChange);
        });
        contextMenu.Items.Add("Bring to bottom", null, (object s, EventArgs args) => {
            Change ringPosChange = new NodeRingIndex(this, parent.Elements.Count - 1);
            ringPosChange.Apply();
            Program.canvas.ChangesManager.Push(ringPosChange);
        });
        contextMenu.Items.Add("Move up", null, (object s, EventArgs args) => {
            Change ringPosChange = new NodeRingIndex(this, parent.Elements.IndexOf(this) - 1);
            ringPosChange.Apply();
            Program.canvas.ChangesManager.Push(ringPosChange);
        });
        contextMenu.Items.Add("Move down", null, (object s, EventArgs args) => {
            Change ringPosChange = new NodeRingIndex(this, parent.Elements.IndexOf(this) + 1);
            ringPosChange.Apply();
            Program.canvas.ChangesManager.Push(ringPosChange);
        });
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Delete", null, (object o, EventArgs args) => { Destroy(); });
        isAttached = true;
    }

    public override void Draw(RenderTarget renderTarget, Vector2 translation)
    {
        Vector2 globalPos = GetGlobalPosition();
        RectangleF translatedRect = new RectangleF(globalPos.X + translation.X, globalPos.Y + translation.Y, boundingBox.Width, boundingBox.Height);
        Vector2 ringPosition = collider.Center + globalPos + translation;
        float textWidth = Utils.MeasureStringWidth(title, Utils.elementTextFormatDefault);
        if (direction == Direction.Out)
        {

            renderTarget.DrawText(title
                , Utils.elementTextFormatDefault, new RectangleF(translatedRect.Right - textWidth - 10f, translatedRect.Y, translatedRect.Width, 0f), Brushes.White);

        }
        else
        {
            renderTarget.DrawText(title
                , Utils.elementTextFormatDefault, new RectangleF(translatedRect.Left + 10f, translatedRect.Y, translatedRect.Width, 0f), Brushes.White);

        }
        Brushes.Ring.Color = color;
        if (highlight) renderTarget.FillEllipse(new Ellipse(ringPosition, 10f, 10f), Brushes.Highlight);
        renderTarget.FillEllipse(new Ellipse(ringPosition, 5f, 5f), Brushes.Ring);
        if (activeWire != null)
        {
            activeWire.Connect(collider);
            activeWire.Draw(renderTarget, translation);
        }
    }

    public override void OnMouseDown(MouseEventArgs e, Collider c)
    {
        if (e.Button == MouseButtons.Left) activeWire = new Wire();
    }


    public override void OnMouseUp(MouseEventArgs e, Collider c)
    {
        if (activeWire == null) return;
        if (
            c != null &&
            c.Drawable.GetType() == typeof(NodeRing) &&
            c.Drawable != this
            && ((NodeRing)c.Drawable).Approachable(this))
        {
            activeWire.Connect(this.collider, c);
            Change change = new WireConnected(activeWire);
            change.Apply();
            Program.canvas.ChangesManager.Push(change);
            activeWire = null;
        }
        else
        {
            activeWire.Destroy();
            activeWire = null;
        }
        Program.MarkCanvasDirty();
    }
    public bool Approachable(NodeRing from)
    {
        if (direction == from.direction) return false;
        if (direction == Direction.In)
        {
            // can be connected to only one output
            return connections.Count == 0;
        }
        else
        {
            return from.connections.Count == 0;
        }
    }
    public override void Update()
    {
        if (activeWire != null)
        {
            Program.MarkCanvasDirty();
        }
        foreach (Drawable d in connections) d.Update();
    }
    public override void Attach()
    {
        if (isAttached) return;
        Physics.colliders.Add(this.collider);
        for (int i = connections.Count - 1; i >= 0; i--) connections[i].Attach();
        isAttached = true;
    }
    public override void Detach()
    {
        if (!isAttached) return;
        Physics.colliders.Remove(this.collider);
        for (int i = connections.Count - 1; i >= 0; i--) connections[i].Detach();
        isAttached = false;
    }
    public override void Destroy()
    {
        base.Destroy();
        for (int i = connections.Count - 1; i >= 0; i--)
        {
            connections[i].Destroy();
        }

        ((Node)parent).RemoveElement(this);
    }
    public override void OnMouseEnter(MouseEventArgs e, Collider collider)
    {
        highlight = true;
        Program.MarkCanvasDirty();
    }

    public override void OnMouseExit(MouseEventArgs e, Collider collider)
    {
        highlight = false;
        Program.MarkCanvasDirty();
    }
    ~NodeRing()
    {
        Console.WriteLine("NodeRing object finallized");
    }
}
