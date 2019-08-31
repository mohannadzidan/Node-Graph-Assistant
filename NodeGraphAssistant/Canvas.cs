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
using NGA.ChangesManagement;

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
    ContextMenu defaultContextMenu;
    List<Drawable> drawables = new List<Drawable>();
    Thread controlThread;
    private StatusStrip statusStrip;

    public List<Drawable> Drawbles { get => drawables; set => drawables = value; }

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

        Init(resolution);

    }
    public Canvas(string name, System.Drawing.Size resolution, Thread controlThread) : base(name)
    {
        this.controlThread = controlThread;
        Init(resolution);
    }
    private void Init(System.Drawing.Size resolution)
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
            NodeAdd change = new NodeAdd(Input.mousePosition.X, Input.mousePosition.Y);
            changesManager.Push(change);
            change.Apply();
            Program.MarkCanvasDirty();
        });
        MainMenuStrip = new MenuStrip();
        ToolStripMenuItem fileTap = new ToolStripMenuItem("File");
        fileTap.DropDownItems.Add(new ToolStripMenuItem("Save", null, SaveCanvas, Keys.Control|Keys.S));
        fileTap.DropDownItems.Add(new ToolStripMenuItem("Save as..", null, SaveCanvasAs, Keys.Control|Keys.Shift|Keys.S));
        fileTap.DropDownItems.Add(new ToolStripMenuItem("Load", null, LoadCanvas, Keys.O|Keys.Control));
        MainMenuStrip.Items.Add(fileTap);
        MainMenuStrip.Renderer = new NGAProfessionalRenderer();
        statusStrip = new StatusStrip();
        statusStrip.Renderer = new NGAProfessionalRenderer();
        statusStrip.Items.Add("fdfads");
        this.Controls.Add(MainMenuStrip);
        this.Controls.Add(statusStrip);
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
    protected override void OnResizeBegin(EventArgs e)
    {
        base.OnResizeBegin(e);
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
        Render();
    }
}
