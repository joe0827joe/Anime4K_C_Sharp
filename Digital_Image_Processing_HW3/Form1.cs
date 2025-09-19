using System;
using System.Drawing;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

//其餘cs程式碼
using Anime4K;//Anime4K核心程式碼
using FileProcessing;//開.讀.寫檔


namespace Digital_Image_Processing_HW3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region 參數
        //圖檔參數
        Bitmap srcBitmap, showBitmap;

        //設定資訊
        public float[] Anime4Kset = { 2f, 0.2f, 0.4f, 2f }; // scale, pushStrength, pushGradStrength, iterations

        public int ImageSetChoose = 3;//ImageSetChoose輸出圖片選擇
        public double Distance0 = 80, Powern = 1
            , HomomorphicRh = 2, HomomorphicRl = 0.25
            , HomomorphicC = 1, BandWidth = 100;//頻率域
        
        public int FilterSize = 20;//空間域
        #endregion

        #region 檔案處理
        //開檔.讀檔
        private void OpenfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            srcBitmap = Picture.Open();
            if (srcBitmap != null)
            {
                pictureBox1.Image = srcBitmap;//繪製影像
            }
        }

        //存檔
        private void OrginalimageSave_Click(object sender, EventArgs e)
        {
            Picture.Close((Bitmap)(pictureBox1.Image));
        }

        private void TargetImageSave_Click(object sender, EventArgs e)
        {
            Picture.Close((Bitmap)(pictureBox3.Image));
        }

        private void Targeimage2Save_Click(object sender, EventArgs e)
        {
            Picture.Close((Bitmap)(pictureBox4.Image));
        }

        private void SpectrumimageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Picture.Close((Bitmap)(pictureBox2.Image));
        }

        //輸入文字檢測
        public static double TextBoxJudge(string text)
        {
            double x;
            bool result = double.TryParse(text, out x);
            if (result)
            {
                return x;
            }
            else
            {
                MessageBox.Show("請不要輸入非數字字符");
                return 0;
            }
        }
        #endregion

        #region 全圖顯示視窗
        //全圖顯示視窗
        private void PictureFullForm(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                //Chart視窗開啟.設定
                Form3 f = new Form3();
                f.Owner = this;
                f.Enabled = true;
                f.fullBitmap = (Bitmap)pictureBox.Image;
                f.Show();
                //f.ShowDialog(this);
            }
        }

        //開啟視窗
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PictureFullForm((PictureBox)sender);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            PictureFullForm((PictureBox)sender);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            PictureFullForm((PictureBox)sender);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            PictureFullForm((PictureBox)sender);
        }
        #endregion

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {//測試鍵
            if (e.Control && e.KeyCode == Keys.T)
            {
                if (ImageSetChoose == 1)
                {
                    //showBitmap = ProcessingImage_BiCubic(showBitmap, (int)srcBitmap.Width, (int)srcBitmap.Height);
                    pictureBox3.Image = showBitmap;//繪製目標影像
                }
                else if (ImageSetChoose == 2)
                {
                    //showBitmap = ProcessingImage_Padding_double(srcBitmap);
                    pictureBox4.Image = showBitmap;//繪製目標影像2
                }
                else if (ImageSetChoose == 3)
                {
                    //showBitmap = ProcessingImage_Freqencyfilter(showBitmap, 5)[1];
                    pictureBox2.Image = showBitmap;//繪製目標影像
                }
            }
        }

        #region 介面
        private void NearestNeighbor_TSM_Click(object sender, EventArgs e)
        {
            ImageBoxSet(1);
        }

        private void Bilinear_TSM_Click(object sender, EventArgs e)
        {
            ImageBoxSet(2);
        }

        private void Bicublic_TSM_Click(object sender, EventArgs e)
        {
            ImageBoxSet(3);
        }

        private void anime4K_TSM_Click(object sender, EventArgs e)
        {
            ImageBoxSet(4);
        }

        //type: 插值法類型
        private void ImageBoxSet(int type)
        {
            if (srcBitmap == null)
            {
                MessageBox.Show("請先開啟一張圖片再進行處理。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (type == 1)
                Program.NearNeighbor(srcBitmap, ref showBitmap);
            else if (type == 2)
                Program.Bilinear(srcBitmap, ref showBitmap);
            else if (type == 3)
                Program.Bicubic(srcBitmap, ref showBitmap);
            else if (type == 4)
                Program.Anime4K(srcBitmap, ref showBitmap, Anime4Kset);

            switch (ImageSetChoose)
            {
                case 1:
                    pictureBox3.Image = showBitmap;//繪製目標影像3
                    break;
                case 2:
                    pictureBox4.Image = showBitmap;//繪製目標影像4
                    break;
                case 3:
                    pictureBox2.Image = showBitmap;//繪製目標影像2
                    break;
            }
        }

        //設定
        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Chart視窗開啟.設定
            Form2 f = new Form2();
            f.Owner = this;
            f.Enabled = true;
            f.Show();
            //f.ShowDialog(this);
        }
        #endregion

        #region 繪圖
        //檢查格式
        public static void CheckSourceFormat(Bitmap original)
        {
            if ((original.PixelFormat != PixelFormat.Format8bppIndexed) ||
                (IsGrayscale(original) == false))
            {
                MessageBox.Show("Source pixel format is not supported.");
                //throw new Exception("Source pixel format is not supported.");
            }
        }

        // 判斷位圖是不是8位灰度
        public static bool IsGrayscale(Bitmap original)
        {
            bool ret = false;

            // 檢查像素格式
            if (original.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ret = true;
                // 檢查調色板
                ColorPalette palette = original.Palette;
                Color color;

                for (int i = 0; i < 256; i++)
                {
                    color = palette.Entries[i];
                    if ((color.R != i) || (color.G != i) || (color.B != i))
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }

        //新建8位灰度位圖
        public static Bitmap CreateGrayscaleImage(int width, int height)
        {
            // 新建圖像
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // 設置灰度圖像的調色板
            SetGrayscalePalette(bitmap);

            return bitmap;
        }

        //設置灰度位圖調色板
        public static void SetGrayscalePalette(Bitmap original)
        {
            // 檢查像素格式
            if (original.PixelFormat != PixelFormat.Format8bppIndexed)
                MessageBox.Show("Source image is not 8 bpp image.");
            //throw new Exception("Source image is not 8 bpp image.");

            // 獲取調色板
            ColorPalette palette = original.Palette;
            // init palette
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            // 設置調色板
            original.Palette = palette;
        }
        #endregion

        #region 演算法
        public class Program
        {

            public static void NearNeighbor(Bitmap srcimage, ref Bitmap showimage)
            {
                Bitmap img = srcimage;
                img = copyType(img);
                float scale = 2f;
                img = upscale(1, img, (int)(img.Width * scale), (int)(img.Height * scale));
                showimage = img;
            }

            public static void Bilinear(Bitmap srcimage, ref Bitmap showimage)
            {
                Bitmap img = srcimage;
                img = copyType(img);
                float scale = 2f;
                img = upscale(2, img, (int)(img.Width * scale), (int)(img.Height * scale));
                showimage = img;
            }

            public static void Bicubic(Bitmap srcimage, ref Bitmap showimage)
            {
                Bitmap img = srcimage;
                img = copyType(img);
                float scale = 2f;
                img = upscale(3, img, (int)(img.Width * scale), (int)(img.Height * scale));
                showimage = img;
            }

            public static void Anime4K(Bitmap srcimage, ref Bitmap showimage, float[] setdata)
            {
                #region parameter
                Bitmap img = srcimage;
                img = copyType(img);

                float scale = 2f;
                scale = setdata[0];

                float pushStrength = scale / 6f;
                float pushGradStrength = scale / 2f;
                int iterations = 2;
                pushStrength = setdata[1];
                pushGradStrength = setdata[2];
                iterations = (int)setdata[3];
                #endregion

                img = upscale(3, img, (int)(img.Width * scale), (int)(img.Height * scale));

                //執行 n 次方向性增強（線性空間、梯度門檻與變化量限制）
                for (int i = 0; i < iterations; i++)
                {
                    var luma = ImageProcess.ComputeLuminanceMask(img);
                    var grad = ImageProcess.ComputeGradientMask(luma, img.Width, img.Height);
                    ImageProcess.CombinedDirectionalPushLinear(
                        ref img,
                        luma,
                        grad,
                        clamp((int)(pushStrength * 255), 0, 255),
                        clamp((int)(pushGradStrength * 255), 0, 255),
                        8,   // 每通道最大變化量
                        24,  // 梯度門檻
                        1.5  // 梯度 γ
                    );
                }
                
                // 應用 Unsharp Mask 進行最終銳化
                ImageProcess.UnsharpMask(ref img, 0.5f, 1.0f);
                
                showimage = img;
            }

            static Bitmap copyType(Bitmap bm)
            {
                Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
                Bitmap clone = bm.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                return clone;
            }

            static Bitmap upscale(int Intertype,Bitmap bm, int width, int height)
            {
                Bitmap newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    switch (Intertype)
                    {
                        case 1:
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            break;
                        case 2:
                            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                            break;
                        case 3:
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            break;
                    }
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.None;
                    g.DrawImage(bm, 0, 0, width, height);
                }
                return newImage;
            }

            private static int clamp(int i, int min, int max)
            {
                if (i < min)
                    i = min;
                else if (i > max)
                    i = max;
                return i;
            }
        }
        #endregion



    }
}