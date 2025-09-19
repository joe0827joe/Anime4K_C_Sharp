using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digital_Image_Processing_HW3
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Form1 f = (Form1)this.Owner;
            if (f.ImageSetChoose == 1)
                radioButton1.Checked = true;
            else if (f.ImageSetChoose == 2)
                radioButton2.Checked = true;
            else
                radioButton3.Checked = true;

            tbox_scale.Text = Convert.ToString(f.Anime4Kset[0]);
            tbox_pushStrength.Text = Convert.ToString(f.Anime4Kset[1]);
            tbox_pushGradStrength.Text = Convert.ToString(f.Anime4Kset[2]);
            tbox_iterations.Text = Convert.ToString(f.Anime4Kset[3]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 f = (Form1)this.Owner;
            if (radioButton1.Checked)
                f.ImageSetChoose = 1;
            else if (radioButton2.Checked)
                f.ImageSetChoose = 2;
            else if (radioButton3.Checked)
                f.ImageSetChoose = 3;
            else
            {
                MessageBox.Show("選項有誤，請重新操作");
                this.Close();
            }


            if (tbox_scale.Text != "")
                f.Anime4Kset[0] = float.Parse(tbox_scale.Text);
            if (tbox_pushStrength.Text != "")
                f.Anime4Kset[1] = float.Parse(tbox_pushStrength.Text);
            if (tbox_pushGradStrength.Text != "")
                f.Anime4Kset[2] = float.Parse(tbox_pushGradStrength.Text);
            if (tbox_iterations.Text != "")
                f.Anime4Kset[3] = float.Parse(tbox_iterations.Text);

            MessageBox.Show("設定完成","設定");

        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(sender, e);
        }
    }
}
