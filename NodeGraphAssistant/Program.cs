using SharpDX;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static bool isCanvasDirty;
    public static Canvas canvas;
    private static bool sleep;
    public static string activeFilePath = "";
    delegate void Void();
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Thread t = new Thread(MainLoop);
        //t.Start();
        canvas = new Canvas("New File", new System.Drawing.Size(800, 600), t);
        Application.Run(canvas);
    }
    public static void MainLoop()
    {
        Stopwatch sw = new Stopwatch();
        MarkCanvasDirty();
        while (true)
        {
            if (sleep)
            {
                Thread.Sleep(33);
                continue;
            }
            sw.Restart();
            Input.ReadInputs();
            UpdateAll();
            if (isCanvasDirty)
            {
                canvas.Invoke(new Void(canvas.Render));
                isCanvasDirty = false;
            }
            sw.Stop();
            int holdingMillis = (int)Math.Max(33 - sw.ElapsedMilliseconds, sw.ElapsedMilliseconds);
            Thread.Sleep(holdingMillis);
        }
    }
    /// <summary>
    /// calling this method will cause a canvas redraw
    /// </summary>
    public static void MarkCanvasDirty()
    {
        isCanvasDirty = true;
    }
    public static void Sleep(bool s)
    {
        sleep = s;
        canvas.Enabled = !s;
    }
    public static void UpdateAll()
    {
        foreach (Drawable d in canvas.Drawbles)
        {
            d.Update();
        }
    }
}

