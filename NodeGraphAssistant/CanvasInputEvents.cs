using NGA.ChangesManagement;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Canvas
{
    bool isSelecting = false;
    bool isDraggingNodes;
    List<Collider> selectionBucket = new List<Collider>();
    Collider mouseEventCollider;

    Vector2 translation;
    Vector2 draggingAnchor;
    Vector2 nodeDraggingAnchor;
    Vector2 startNodeDraggingAnchor;
    Vector2 draggingAmount;
    RectangleF selectionRectangle;
    ChangesManager changesManager = new ChangesManager();

    public Vector2 Translation { get => translation + draggingAmount; set => translation = value; }
    public List<Collider> SelectionBucket { get => selectionBucket; }
    public ChangesManager ChangesManager { get => changesManager;}

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (e.Button == MouseButtons.Middle) // perform canvas translation
        {
            draggingAmount = Input.mousePosition - draggingAnchor;
            Program.MarkCanvasDirty();
        }
        else if (e.Button == MouseButtons.Left &&
            (mouseEventCollider == null || (mouseEventCollider != null && mouseEventCollider.Drawable.GetType() == typeof(Node))))
        {

            if (isSelecting)
            {
                Vector2 location = new Vector2(e.X, e.Y);
                selectionRectangle.Width = location.X - selectionRectangle.X;
                selectionRectangle.Height = location.Y - selectionRectangle.Y;
                Program.MarkCanvasDirty();
            }
            else if (isDraggingNodes)
            {
                foreach (Collider s in selectionBucket)
                {
                    s.Drawable.Translate(Input.mousePosition - nodeDraggingAnchor);
                }
                nodeDraggingAnchor = Input.mousePosition;
                Program.MarkCanvasDirty();

            }
            else if (!isDraggingNodes && (Physics.Pointcast<Node>(Input.mousePosition - Translation)) != null)
            {
                nodeDraggingAnchor = Input.mousePosition;
                startNodeDraggingAnchor = nodeDraggingAnchor;
                isDraggingNodes = true;
            }
        }

    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (mouseEventCollider != null)
        {
            mouseEventCollider.Drawable.OnMouseUp(e, Physics.Pointcast(Input.mousePosition - Translation));
        }
        if (e.Button == MouseButtons.Left)
        {
            if (isSelecting)
            {
                if (selectionRectangle.Width < 0)
                {
                    selectionRectangle.Width = Math.Abs(selectionRectangle.Width);
                    selectionRectangle.X -= selectionRectangle.Width;
                }
                if (selectionRectangle.Height < 0)
                {
                    selectionRectangle.Height = Math.Abs(selectionRectangle.Height);
                    selectionRectangle.Y -= selectionRectangle.Height;
                }
                Collider[] newSelecitons = Physics.RectcastAll<Node>(selectionRectangle);
                if (ModifierKeys != Keys.Control)
                {
                    selectionBucket.Clear();
                }
                foreach (Collider newSelection in newSelecitons)
                {
                    if (selectionBucket.Contains(newSelection))
                        selectionBucket.Remove(newSelection);
                    else
                        selectionBucket.Add(newSelection);
                }
                Program.MarkCanvasDirty();
            }
            isSelecting = false;
            if (isDraggingNodes)
            {
                Node[] affected = new Node[selectionBucket.Count];
                for (int i = 0; i < selectionBucket.Count; i++) affected[i] = (Node)selectionBucket[i].Drawable;
                changesManager.Push(new NodeMovement(Input.mousePosition - startNodeDraggingAnchor, affected));
                isDraggingNodes = false;
            }
        }

        if (e.Button == MouseButtons.Middle)
        { // end canvas translation
            translation += draggingAmount;
            draggingAmount = Vector2.Zero;
        }
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {

        base.OnMouseDown(e);
        Collider c;
        if ((c = Physics.Pointcast(new Vector2(e.Location.X, e.Location.Y) - Translation)) != null)
        {
            mouseEventCollider = c;
            mouseEventCollider.Drawable.OnMouseDown(e, c);
        }

        if (e.Button == MouseButtons.Middle)
        { // start canvas translation event
            draggingAnchor = Input.mousePosition;
        }
        else if (e.Button == MouseButtons.Left)
        {
            if (c == null)
            {
                selectionRectangle.Location = Input.mousePosition;
                selectionRectangle.Width = 0;
                selectionRectangle.Height = 0;
                isSelecting = true;
            }
            else if (c.Drawable.GetType() == typeof(Node) && !selectionBucket.Contains(c))
            {
                selectionBucket.Clear();
                selectionBucket.Add(c);
            }
        }

    }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (e.Button == MouseButtons.Right)
        {
            Collider collider;
            if (Physics.Pointcast(new Vector2(e.Location.X, e.Location.Y) - Translation, out collider))
            {
                if (collider.Drawable.ContextMenu != null)
                {
                    collider.Drawable.ContextMenu.Show(this, e.Location);
                }
            }
            else
            {
                defaultContextMenu.Show(this, e.Location);
            }
        }
        Collider c;
        if (e.Button == MouseButtons.Left && !isDraggingNodes)
        {
            if ((c = Physics.Pointcast<Node>(Input.mousePosition - Translation)) != null)
            {
                if (ModifierKeys == Keys.Control)
                {
                    if (selectionBucket.Contains(c))
                    {
                        selectionBucket.Remove(c);
                    }
                    else selectionBucket.Add(c);
                }
                else
                {
                    selectionBucket.Clear();
                    selectionBucket.Add(c);

                }
                Program.MarkCanvasDirty();
            }

        }

    }
    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {

        base.OnMouseDoubleClick(e);
        Collider collider;
        if ((collider = Physics.Pointcast(new Vector2(e.Location.X, e.Location.Y) - Translation)) != null)
        {
            mouseEventCollider = collider;
            mouseEventCollider.Drawable.OnMouseDoubleClick(e, collider);
        }
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.S && ModifierKeys == Keys.Control)
        {
            SaveCanvas(null, null);
        }
        else if (e.KeyCode == Keys.S && ModifierKeys == (Keys.Control | Keys.Shift))
        {
            SaveCanvasAs(null, null);
        }
        if (e.KeyCode == Keys.Delete)
        {
            Node[] affected = new Node[selectionBucket.Count];
            for (int i = 0; i < affected.Length; i++) affected[i] = (Node)selectionBucket[i].Drawable;
            NodeRemoved nodeRemove = new NodeRemoved(affected);
            nodeRemove.Apply();
            changesManager.Push(nodeRemove);
            selectionBucket.Clear();
            Program.MarkCanvasDirty();

        }
        if (e.KeyCode == Keys.Z && ModifierKeys == Keys.Control)
        {
            changesManager.Undo();
            Program.MarkCanvasDirty();
        }
        else if (e.KeyCode == Keys.Z && ModifierKeys == (Keys.Control | Keys.Shift))
        {
            changesManager.Redo();
            Program.MarkCanvasDirty();
        }
    }
}

