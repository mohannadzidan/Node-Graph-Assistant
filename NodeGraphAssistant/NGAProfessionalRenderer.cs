using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

class NGAProfessionalRenderer : ToolStripProfessionalRenderer
{
    SolidBrush backgroundBrush = new SolidBrush(Colors.ToSystemARGB(Colors.DarkGrey));
    SolidBrush selectedBrush = new SolidBrush(Colors.ToSystemARGB(Colors.Grey));
    SolidBrush accentBrush = new SolidBrush(Colors.ToSystemARGB(Colors.Accent));
    Pen seperatorPen;
   
    public NGAProfessionalRenderer() : base()
    {
        
        seperatorPen = new Pen(System.Drawing.Brushes.Gray);
        seperatorPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        seperatorPen.DashOffset = 5;
        seperatorPen.DashPattern = new float[] { 15, 5 };
    }
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
        if (e.Item.Selected)
        {
            e.Graphics.FillRectangle(selectedBrush, rc);

        }
        else
        {

            e.Graphics.FillRectangle(backgroundBrush, rc);
        }
    }
    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = Color.White;
        base.OnRenderArrow(e);
    }
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.White;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        e.Item.BackColor = seperatorPen.Color;
        e.Item.ForeColor = seperatorPen.Color;
        e.Graphics.DrawLine(seperatorPen, 0, e.Item.Size.Height / 2, e.Item.Width, e.Item.Size.Height / 2);

    }
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        Rectangle rc = new Rectangle(Point.Empty, e.ToolStrip.Size);
        e.Graphics.FillRectangle(backgroundBrush, rc);
    }
}
