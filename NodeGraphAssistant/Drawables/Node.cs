using System;
using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Forms;
using System.Collections.Generic;

public class Node : Drawable
{
    public const float HeaderHeight = 25f;
    public float headerSize = 50;
    private List<Drawable> elements = new List<Drawable>();
    string title = "Untitled";
    public string Title { get => title; set => title = value; }
    public Drawable[] Elements { get => elements.ToArray(); }

    public Node(float x, float y, float width = 120f) : base(null)
    {
        boundingBox.X = x;
        boundingBox.Y = y;
        boundingBox.Width = width;
        boundingBox.Height = HeaderHeight+5;
        collider = new RectangleCollider(this, new RectangleF(0, 0, width, 30));
        InitializeContextMenu();
    }
    public Node(float x, float y, Node copy) : base(null){
        boundingBox = new RectangleF(x, y, copy.BoundingBox.Width, copy.BoundingBox.Height);
        RectangleCollider copyCollider = (RectangleCollider) copy.Collider;
        collider = new RectangleCollider(this, new RectangleF(copyCollider.rect.X, copyCollider.rect.Y, copyCollider.rect.Width, copyCollider.rect.Height));
        title = copy.title;
        Random r = new Random();
        foreach (Drawable  d in copy.elements)
        {
            if (d.GetType() == typeof(NodeRing)) {
                NodeRing copyNodeRing = (NodeRing)d;
                Color4 newColor = new Color4(r.NextFloat(0f, 1f), r.NextFloat(0f, 1f), r.NextFloat(0f, 1f), 1f);
                NodeRing nodeRing = new NodeRing(this, copyNodeRing.Title, newColor, copyNodeRing.Direction);
                elements.Add(nodeRing);
            }
        }
        CalculateElementsPositions();
        InitializeContextMenu();
        Program.MarkCanvasDirty();
    }
    void InitializeContextMenu() {
        contextMenu = new ContextMenuStrip();
        contextMenu.Renderer = new NGAProfessionalRenderer();
        contextMenu.BackColor = System.Drawing.Color.FromArgb(255, 64, 64, 64);
        contextMenu.ForeColor = System.Drawing.Color.White;
        ToolStripMenuItem generic = new ToolStripMenuItem("Add Generic Element");
        generic.BackColor = contextMenu.BackColor;
        generic.ForeColor = contextMenu.ForeColor;
        generic.DropDownItems.AddRange(new ToolStripMenuItem[]
           {
                new ToolStripMenuItem("Output", null, new EventHandler((object o, EventArgs args) => {
                        AddElement(new NodeRing(this, "Output "+elements.Count, Colors.Accent, Direction.Out));
                    })),
                new ToolStripMenuItem("Input", null,new EventHandler((object o, EventArgs args) => {
                    AddElement(new NodeRing(this, "Input "+elements.Count, Colors.Accent, Direction.In));
                })),
           });

        contextMenu.Items.Add(generic);
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Rename", null, (object o, EventArgs e) => {
            TextEditForm form = new TextEditForm(title, (string res) => { title = res; Program.MarkCanvasDirty(); });
            form.Show();
        });
        contextMenu.Items.Add("Dublicate", null, (object o, EventArgs e) => {
            Vector2 newPos = boundingBox.Location;
            newPos.Y += boundingBox.Height + 10;
            Program.canvas.Drawbles.Add(new Node(newPos.X, newPos.Y, this));
        });
        contextMenu.Items.Add("Delete", null, (object o, EventArgs e) => { Destroy(); Program.MarkCanvasDirty(); });
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Properties");
    }

    public void AddElement(Drawable d)
    {
        boundingBox.Height += d.BoundingBox.Height;
        ((RectangleCollider)collider).rect.Height = boundingBox.Height;
        elements.Add(d);
        CalculateElementsPositions();
        Program.MarkCanvasDirty();
    }
    public void RemoveElement(Drawable element)
    {
        if (elements.Contains(element))
        {
            boundingBox.Height -= element.BoundingBox.Height;
            ((RectangleCollider)collider).rect.Height = boundingBox.Height;
            elements.Remove(element);
            CalculateElementsPositions();
            Program.MarkCanvasDirty();
        }
    }
    private void CalculateElementsPositions()
    {
        float displacment = HeaderHeight+5; // for header
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].SetLocation(new Vector2(0, displacment));
            displacment += elements[i].BoundingBox.Height;
        }
    }
    public override void Update()
    {
        foreach (Drawable d in elements) d.Update();
    }
    public override void Draw(RenderTarget renderTarget, Vector2 translation)
    {
        RectangleF translatedRect = new RectangleF(translation.X + boundingBox.X, translation.Y + boundingBox.Y, boundingBox.Width, boundingBox.Height);
        renderTarget.FillRoundedRectangle(new RoundedRectangle() { RadiusX = 10, RadiusY = 10, Rect = translatedRect }, Brushes.NodeBackground); ;
        renderTarget.FillRectangle(new RectangleF(translatedRect.X, translatedRect.Y, translatedRect.Width, HeaderHeight), Brushes.DarkGrey);
        float titleY = translatedRect.Y + HeaderHeight/2 - Utils.MeasureStringHeight(title, Utils.elementTextFormatDefault)/2;
        RectangleF titleRect = new RectangleF(translatedRect.X + 10, titleY, translatedRect.Width - 10, HeaderHeight);
        renderTarget.DrawText(title, Utils.elementTextFormatDefault, titleRect, Brushes.White);
        for (int i = 0; i < elements.Count; i++)
        {

            elements[i].Draw(renderTarget, translation);
        }

    }
    public Vector2 SnapToGrid(Vector2 point)
    {
        float minGridSize = Canvas.MainGridSize / 10f;
        Vector2 closestGridNode;
        float deltaX = point.X % minGridSize;
        float deltaY = point.Y % minGridSize;
        closestGridNode.X = deltaX <= minGridSize / 2f ? point.X - deltaX : point.X - deltaX + minGridSize;
        closestGridNode.Y = deltaY <= minGridSize / 2f ? point.Y - deltaY : point.Y - deltaY + minGridSize;
        Console.WriteLine(" input=" + point + " rex=" + closestGridNode);
        return closestGridNode;
    }
    void EnableConnectionColliders(bool enable)
    {
        foreach (Drawable d in elements)
        {
            if (d.GetType() == typeof(NodeRing))
            {
                NodeRing nodeRing = (NodeRing)d;
                foreach (Wire connection in nodeRing.Connections)
                {
                    if (enable)
                    {
                        connection.ReconstructCollider();
                        connection.Collider.RecivesRaycasts = true;
                    }
                    else
                    {
                        connection.Collider.RecivesRaycasts = false;
                    }

                }
            }
        }
    }
    public void MoveElementDown(Drawable e)
    {
        int index = elements.IndexOf(e);
        if (index != -1 && elements.Count > 1 && index < elements.Count - 1)
        {
            Drawable temp = elements[index + 1];
            elements[index + 1] = e;
            elements[index] = temp;
        }
        CalculateElementsPositions();
        Program.MarkCanvasDirty();
    }
    public void MoveElementUp(Drawable e)
    {
        int index = elements.IndexOf(e);
        if (index != -1 && elements.Count > 1 && index > 0)
        {
            Drawable temp = elements[index - 1];
            elements[index - 1] = e;
            elements[index] = temp;
        }
        CalculateElementsPositions();
        Program.MarkCanvasDirty();
    }
    public void BringElementToTop(Drawable e)
    {
        if (elements.Contains(e))
        {
            elements.Remove(e);
            elements.Insert(0, e);
        }
        CalculateElementsPositions();
        Program.MarkCanvasDirty();
    }
    public void BringElementToBottom(Drawable e)
    {
        if (elements.Contains(e))
        {
            elements.Remove(e);
            elements.Add(e);
        }
        CalculateElementsPositions();
        Program.MarkCanvasDirty();
    }
    public override void RecalculateBounds()
    {
        throw new NotImplementedException();
    }

    public override void OnMouseDown(MouseEventArgs e, Collider collider)
    {
        EnableConnectionColliders(false);
    }

    public override void OnMouseUp(MouseEventArgs e, Collider collider)
    {
        EnableConnectionColliders(true);
        // notify all wires to invalidate their colliders for the new position
    }
    public override void Destroy()
    {
        for (int i = elements.Count -1; i >= 0; i--)
        {
            elements[i].Destroy();
        } 
        Program.canvas.SelectionBucket.Remove(this.Collider);
        base.Destroy();
    }
}
