using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class TextEditForm : Form
{
    public delegate void ReasultDelegate(string s);
    public delegate void CloseDelegate();
    ReasultDelegate onApplyCallback;
    public TextEditForm(string text, ReasultDelegate onApply)
    {
        InitializeComponent();
        onApplyCallback = onApply;
        textbox.Text = text;
    }
    private void OnApply(object sender, EventArgs e) {
        onApplyCallback.Invoke(textbox.Text);
        this.Close();
    }
    private void OnCancel(object sender, EventArgs e)
    {
        this.Close();
    }

}
