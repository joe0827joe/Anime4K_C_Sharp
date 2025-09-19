using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace FileProcessing
{
    class Picture
    {
        public static Bitmap Open()
        {
            OpenFileDialog ofd = new OpenFileDialog();//開啟檔案物件
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap srcBitmap = (Bitmap)Bitmap.FromFile(ofd.FileName, false);
                return srcBitmap;
            }
            return null;
        }

        public static Bitmap Close(Bitmap dstBitmap)
        {
            if (dstBitmap != null)
            {

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = @"Bitmap文件(*.bmp)|*.bmp|Jpeg文件(*.jpg)|*.jpg|所有合適文件(*.bmp,*.jpg)|*.bmp;*.jpg";

                sfd.FilterIndex = 3;
                sfd.RestoreDirectory = true;
                if (DialogResult.OK == sfd.ShowDialog())
                {
                    ImageFormat format = ImageFormat.Jpeg;
                    switch (Path.GetExtension(sfd.FileName).ToLower())
                    {
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        default:
                            MessageBox.Show("Unsupported image format was specified", "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                    try
                    {
                        dstBitmap.Save(sfd.FileName, format);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed writing image file", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return null;
        }


    }
}
