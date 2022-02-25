using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium_Desktop
{
    public partial class InputDialog : Form
    {
        public InputDialog()
        {
            InitializeComponent();
        }



        public static string Input(Form owner, String hint,string prefilled = "") {
            bool ownerToplevel = owner.TopMost;
            String text = "";
            InputDialog form = new InputDialog();
            form.Text = hint;
            form.textBox1.Text = prefilled;
            if (form.ShowDialog(owner) == DialogResult.OK) {
                text = form.textBox1.Text;
            }
            else
            {
                text = null;
            }
            owner.TopMost = ownerToplevel;
            return text;
        }

        
        private void button2_Click(object sender, EventArgs e)
        {

            string t = ""; 
            try
            {
                t= Clipboard.GetText().Trim().Replace("\r","").Replace("\n","");
            }
            catch
            {
                t = "";
            }
            if(t != null && t.Length > 0)
            {
                textBox1.Text = t;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("\n"))
            {
                textBox1.Text = textBox1.Text.Replace("\n", "").Replace("\r", "");
                DialogResult = DialogResult.OK;
            }
        }

        private void InputDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
