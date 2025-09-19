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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        //全圖顯示視窗參數
        public Bitmap fullBitmap;

        private void Form3_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = fullBitmap;
            if(fullBitmap.Width <= SystemInformation.PrimaryMonitorSize.Width && (fullBitmap.Height + (this.Height - this.ClientRectangle.Height)) <= SystemInformation.PrimaryMonitorSize.Height)
            {
                this.Height = fullBitmap.Height + (this.Height - this.ClientRectangle.Height);
                this.Width = fullBitmap.Width;
            }
            else if (fullBitmap.Width <= SystemInformation.PrimaryMonitorSize.Width)
            {
                this.Height = SystemInformation.PrimaryMonitorSize.Height;
                this.Width = fullBitmap.Width;
            }
            else if ((fullBitmap.Height + (this.Height - this.ClientRectangle.Height)) <= SystemInformation.PrimaryMonitorSize.Height)
            {
                this.Height = fullBitmap.Height;
                this.Width = SystemInformation.PrimaryMonitorSize.Width;
            }
            else
            {
                //this.FormBorderStyle = FormBorderStyle.None;//去除邊框
                this.WindowState = FormWindowState.Maximized;//最大化
            }
            this.TopMost = true;//視窗置頂
        }

    }
}
