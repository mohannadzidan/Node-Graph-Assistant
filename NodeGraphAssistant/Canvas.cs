using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Runtime.InteropServices;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;
using NGA.Holders;

public partial class Canvas : RenderForm
{
    public const float MainGridSize = 100f;
    SwapChain swapChain;
    SwapChainDescription swapChainDsc;
    Device d3dDevice;
    Texture2D backBuffer;
    RenderTargetView renderView;
    Surface surface;
    Factory factory;
    RenderTarget renderTarget;
    Vector2 translation;
    Vector2 draggingAnchor;
    Vector2 nodeDraggingAnchor;
    Vector2 draggingAmount;
    RectangleF selectionRectangle;
    ContextMenu defaultContextMenu;
    List<Drawable> drawables = new List<Drawable>();
    List<Collider> selectionBucket = new List<Collider>();
    Collider mouseEventCollider;
    bool isDraggingNodes;
    bool isSelecting = false;
    public bool holdRender;
    Thread controlThread;
    public List<Drawable> Drawbles { get => drawables; set => drawables = value; }
    public Vector2 Translation { get => translation + draggingAmount; set => translation = value; }
    public List<Collider> SelectionBucket { get => selectionBucket; }

    public void UpdateView()
    {

    }
    public void Render()
    {
        renderTarget.BeginDraw();
        ////////////////
        renderTarget.Clear(Colors.BackgroundColor);
        DrawGrid(renderTarget);
        for (int i = 0; i < drawables.Count; i++) drawables[i].Draw(renderTarget, Translation);
        Physics.DrawColliders(renderTarget, Translation);
        if (isSelecting)
        {
            renderTarget.DrawRectangle(selectionRectangle, Brushes.Accent);
        }
        if (selectionBucket.Count > 0)
        {
            foreach (Collider selection in selectionBucket)
            {
                RectangleF rect = ((RectangleCollider)selection).rect;
                rect.Location += translation + selection.Drawable.GetGlobalPosition();
                rect.X -= 5;
                rect.Y -= 5;
                rect.Width += 10;
                rect.Height += 10;
                renderTarget.FillRectangle(rect, Brushes.Selection);
            }
        }
        ////////////////
        renderTarget.EndDraw();
        swapChain.Present(0, PresentFlags.None);

    }
    void DrawGrid(RenderTarget renderTarget)
    {
        float smallGridSize = MainGridSize / 10f;
        Vector2 gridTranslation = translation + draggingAmount;
        float mainGridX = gridTranslation.X % MainGridSize, mainGridY = gridTranslation.Y % MainGridSize;
        float smallGridX = gridTranslation.X % smallGridSize, smallGridY = gridTranslation.Y % smallGridSize;
        bool verticalLinesDrawn = false, horizontalLinesDrawn = false;
        while (!verticalLinesDrawn)
        {
            verticalLinesDrawn = true;

            if (smallGridX <= ClientSize.Width)
            {
                verticalLinesDrawn = false;
                renderTarget.DrawLine(new Vector2(smallGridX, 0), new Vector2(smallGridX, ClientSize.Height), Brushes.Grid, 0.1f);
                smallGridX += smallGridSize;
            }
            if (mainGridX <= ClientSize.Width)
            {
                verticalLinesDrawn = false;
                renderTarget.DrawLine(new Vector2(mainGridX, 0), new Vector2(mainGridX, ClientSize.Height), Brushes.Grid, 0.2f);
                mainGridX += MainGridSize;
            }
        }

        while (!horizontalLinesDrawn)
        {
            horizontalLinesDrawn = true;

            if (smallGridY <= ClientSize.Width)
            {
                horizontalLinesDrawn = false;
                renderTarget.DrawLine(new Vector2(0, smallGridY), new Vector2(ClientSize.Width, smallGridY), Brushes.Grid, 0.1f);
                smallGridY += smallGridSize;
            }
            if (mainGridY <= ClientSize.Width)
            {
                horizontalLinesDrawn = false;
                renderTarget.DrawLine(new Vector2(0, mainGridY), new Vector2(ClientSize.Width, mainGridY), Brushes.Grid, 0.2f);
                mainGridY += MainGridSize;
            }
        }
    }
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        controlThread.Start();
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        controlThread.Abort();

    }
    public Canvas(System.Drawing.Size resolution) : base()
    {

        InitDX(resolution);

    }
    public Canvas(string name, System.Drawing.Size resolution, Thread controlThread) : base(name)
    {
        this.controlThread = controlThread;
        InitDX(resolution);
    }
    private void InitDX(System.Drawing.Size resolution)
    {
        WindowState = FormWindowState.Maximized;
        SetDesktopBounds(0, 0, resolution.Width, resolution.Height);
        // SwapChain description
        swapChainDsc = new SwapChainDescription()
        {
            BufferCount = 1,
            ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.B8G8R8A8_UNorm),
            IsWindowed = true,
            OutputHandle = Handle,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.Discard,
            Usage = Usage.RenderTargetOutput | Usage.ShaderInput,
            Flags = SwapChainFlags.None
        };

        // Create Device and SwapChain
        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, swapChainDsc, out d3dDevice, out swapChain);

        var d2dFactory = new SharpDX.Direct2D1.Factory();
        // Ignore all windows events
        factory = swapChain.GetParent<Factory>();
        factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);
        // New RenderTargetView from the backbuffer
        backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
        renderView = new RenderTargetView(d3dDevice, backBuffer);
        surface = backBuffer.QueryInterface<Surface>();
        renderTarget = new RenderTarget(d2dFactory, surface,
                                                        new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));

        Brushes.Initialize(renderTarget);
        // initialize menus
        defaultContextMenu = new ContextMenu();
        defaultContextMenu.MenuItems.Add("Add node", (object o, EventArgs e) => {
            drawables.Add(new Node(Input.mousePosition.X, Input.mousePosition.Y));
            Program.MarkCanvasDirty();
        });
        MainMenuStrip = new MenuStrip();
        ToolStripMenuItem fileTap = new ToolStripMenuItem("File");
        fileTap.DropDownItems.Add(new ToolStripMenuItem("Save", null, SaveCanvas, Keys.Control|Keys.S));
        fileTap.DropDownItems.Add(new ToolStripMenuItem("Save as..", null, SaveCanvasAs, Keys.Control|Keys.Shift|Keys.S));
        fileTap.DropDownItems.Add(new ToolStripMenuItem("Load", null, LoadCanvas, Keys.O|Keys.Control));
        MainMenuStrip.Items.Add(fileTap);
        MainMenuStrip.Renderer = new NGAProfessionalRenderer();
        this.Controls.Add(MainMenuStrip);
    }
    public void Start()
    {
        RenderLoop.Run(this, new RenderLoop.RenderCallback(Render));
        renderView.Dispose();
        backBuffer.Dispose();
        d3dDevice.ImmediateContext.ClearState();
        d3dDevice.ImmediateContext.Flush();
        d3dDevice.Dispose();
        swapChain.Dispose();
        factory.Dispose();
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
            else {
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
            if (!isDraggingNodes && (Physics.Pointcast<Node>(Input.mousePosition - Translation)) != null)
            {
                nodeDraggingAnchor = Input.mousePosition;
                isDraggingNodes = true;
            }
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
            isDraggingNodes = false;
        }

        if (e.Button == MouseButtons.Middle)
        { // end canvas translatting
            translation += draggingAmount;
            draggingAmount = Vector2.Zero;
        }
    }

    protected override void OnResizeBegin(EventArgs e)
    {
        base.OnResizeBegin(e);
        holdRender = true;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.S && ModifierKeys == Keys.Control)
        {
            SaveCanvas(null, null);
        }
        else if (e.KeyCode == Keys.S && ModifierKeys == (Keys.Control|Keys.Shift)) {
            SaveCanvasAs(null, null);
        }

    }
    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        if (renderTarget == null) return;
        var f = renderTarget.Factory;
        d3dDevice.ImmediateContext.ClearState();
        renderTarget.Dispose();
        backBuffer.Dispose();
        renderView.Dispose();
        surface.Dispose();
        swapChain.ResizeBuffers(1, 0, 0, Format.Unknown, SwapChainFlags.AllowModeSwitch);
        backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
        renderView = new RenderTargetView(d3dDevice, backBuffer);
        surface = backBuffer.QueryInterface<Surface>();
        renderTarget = new RenderTarget(f, surface, new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
        holdRender = false;
        Render();
    }

    public void SaveCanvas(object sender, EventArgs args)
    {
        if (string.IsNullOrEmpty(Program.activeFilePath))
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "NCH files(*.nch) | *.nch";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Program.activeFilePath = saveFileDialog.FileName;
                Console.WriteLine(Program.activeFilePath);
                CanvasHolder ch = new CanvasHolder(this);
                string data = ch.ToJson();
                System.IO.Stream stream = saveFileDialog.OpenFile();
                System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(stream);
                streamWriter.Write(data);
                streamWriter.Close();
                stream.Close();
            }
        }
        else {
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(Program.activeFilePath);
            streamWriter.Write(new CanvasHolder(this).ToJson());
            streamWriter.Close();
        }

    }
    public void SaveCanvasAs(object sender, EventArgs args)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "NCH files(*.nch) | *.nch";
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            Program.activeFilePath = saveFileDialog.FileName;
            Console.WriteLine(Program.activeFilePath);
            CanvasHolder ch = new CanvasHolder(this);
            string data = ch.ToJson();
            System.IO.Stream stream = saveFileDialog.OpenFile();
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(stream);
            streamWriter.Write(data);
            streamWriter.Close();
            stream.Close();
        }
    }
    public void LoadCanvas(object sender, EventArgs args)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "NCH files(*.nch) | *.nch";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            Program.activeFilePath = openFileDialog.FileName;
            System.IO.Stream stream = openFileDialog.OpenFile();
            System.IO.StreamReader streamReader = new System.IO.StreamReader(stream);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            CanvasHolder ch = CanvasHolder.FromJson(data);
            drawables.Clear();
            selectionBucket.Clear();
            Physics.colliders.Clear();
            ch.Release(this);
        }
    }


    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // GraphicsWindow
        // 
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.ControlBox = false;
        this.DoubleBuffered = true;
        this.ForeColor = System.Drawing.SystemColors.ControlText;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "GraphicsWindow";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Node Graph Assistant";
        this.TransparencyKey = System.Drawing.Color.Transparent;
        this.ResumeLayout(false);

    }
}
