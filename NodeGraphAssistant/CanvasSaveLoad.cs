using NGA.Holders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Canvas
{
    bool isCanvasEdited = false;
    public bool IsCanvasEdited { get => isCanvasEdited; set => isCanvasEdited = value; }

    public void SaveCanvas(object sender, EventArgs args)
    {
        ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;

        if (string.IsNullOrEmpty(Program.activeFilePath))
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "NCH files(*.nch) | *.nch";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Program.activeFilePath = saveFileDialog.FileName;
                toolStripMenuItem.Text = "Save to " + System.IO.Path.GetFileName(saveFileDialog.FileName);
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
        else
        {
            toolStripMenuItem.Text = "Save to " + System.IO.Path.GetFileName(Program.activeFilePath);
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
        if (isCanvasEdited)
        {

            DialogResult result = MessageBox.Show("Your changes have not been saved!\nDo you want to save this file first?", "Confirmation",
                                     MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel)
            {
                return;
            }
            else if (result == DialogResult.Yes)
            {
                SaveCanvas(null, null);
            }
        }
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "NCH files(*.nch) | *.nch";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            Program.activeFilePath = openFileDialog.FileName;
            this.Text = openFileDialog.FileName;
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

}
