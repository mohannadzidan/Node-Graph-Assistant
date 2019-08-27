using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Input
{
    public static float MouseWheelDelta { get => 0; }
    public static Vector2 mousePosition => PointToVector2((System.Drawing.Point)Program.canvas.Invoke(new SingleParam(Program.canvas.PointToClient), Cursor.Position));
    static bool[] mouseButtonsWasPressed = new bool[5];
    static bool[] mouseButtonsDown = new bool[5];
    static bool[] mouseButtonsUp = new bool[5];
    static bool[] mouseButtons = new bool[5];
    delegate System.Drawing.Point SingleParam(System.Drawing.Point p);
    public static void ReadInputs()
    {
        mouseButtons[0] = Control.MouseButtons == MouseButtons.Left;
        mouseButtons[1] = Control.MouseButtons == MouseButtons.Right;
        mouseButtons[2] = Control.MouseButtons == MouseButtons.Middle;
        mouseButtonsDown[0] = !mouseButtonsWasPressed[0] && mouseButtons[0] ? true : false;
        mouseButtonsDown[1] = !mouseButtonsWasPressed[1] && mouseButtons[1] ? true : false;
        mouseButtonsDown[2] = !mouseButtonsWasPressed[2] && mouseButtons[2] ? true : false;
        mouseButtonsUp[0] = mouseButtonsWasPressed[0] && !mouseButtons[0] ? true : false;
        mouseButtonsUp[1] = mouseButtonsWasPressed[1] && !mouseButtons[1] ? true : false;
        mouseButtonsUp[2] = mouseButtonsWasPressed[2] && !mouseButtons[2] ? true : false;
        mouseButtonsWasPressed[0] = mouseButtons[0];
        mouseButtonsWasPressed[1] = mouseButtons[1];
        mouseButtonsWasPressed[2] = mouseButtons[2];
    }
    private static Vector2 PointToVector2(System.Drawing.Point p)
    {
        return new Vector2(p.X, p.Y);
    }
    public static bool GetMouseButtonDown(int button)
    {
        return mouseButtonsDown[button];
    }
    public static bool GetMouseButton(int button)
    {
        return mouseButtons[button];
    }
    public static bool GetMouseButtonUp(int button)
    {
        return mouseButtonsUp[button];
    }

}

