using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital_Image_Processing_HW3
{
    class Class1
    {
        //傅立葉光譜
        //image 源圖數據
        public void ProcessingImage_Spectrum(Bitmap image)
        {
            // 檢查源圖像格式
            CheckSourceFormat(image);

            /*
            #region 參數
            // 鎖定源圖像內存
            BitmapData srcData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            // 新建目標圖像
            Bitmap dstImage = CreateGrayscaleImage(image.Width, image.Height);
            // 鎖定目標圖像內存
            BitmapData dstData = dstImage.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            #endregion
            */

            Bitmap spaceImage = new Bitmap(image.Width, image.Height, image.PixelFormat);

            try
            {
                //Spectrum(srcData, ref dstData);
                //pictureBox2.Image = dstImage;//繪製目標影像

                //原始影像
                ComplexImage complexImage = ComplexImage.FromBitmap(image);
                complexImage.ForwardFourierTransform();//正向傅立葉轉換
                //空白影像
                ComplexImage complexImage2 = ComplexImage.FromBitmap(spaceImage);
                complexImage2.ForwardFourierTransform();//正向傅立葉轉換
                //濾波器

                //捲積
                for (int y = 0; y < complexImage.Height; y++)
                {
                    for (int x = 0; x < complexImage.Width; x++)
                    {
                        complexImage2.Data[x, y] = complexImage.Data[x, y];
                    }
                }
                //complexImage2.BackwardFourierTransform();//逆向傅立葉轉換
                showSpectrumBitmap = complexImage2.ToBitmap();//轉換為Bitmap
                pictureBox2.Image = showSpectrumBitmap;//繪製目標影像

            }
            finally
            {
                // 解鎖圖像內存
                /*
                dstImage.UnlockBits(dstData);
                image.UnlockBits(srcData);
                */
            }
        }

        //傅立葉光譜
        //srcData 源圖數據, dstData 目標圖數據
        public void Spectrum(BitmapData srcData, ref BitmapData dstData)
        {
            #region 參數
            //獲取源圖像數據
            int srcWidth = srcData.Width;
            int srcHeight = srcData.Height;
            int srcStride = srcData.Stride;
            IntPtr srcPtr = srcData.Scan0;

            int dstWidth = dstData.Width;
            int dstHeight = dstData.Height;
            int dstStride = dstData.Stride;
            int dstOffset = dstStride - dstWidth;
            IntPtr dstPtr = dstData.Scan0;

            // 將源圖像數據複製到託管內存中
            int srcBytes = srcStride * srcHeight;
            byte[] srcGrayData = new byte[srcBytes];
            Marshal.Copy(srcPtr, srcGrayData, 0, srcBytes);
            // 保存目標圖像數據
            int dstBytes = dstStride * dstHeight;
            byte[] dstGrayData = new byte[dstBytes];
            int dst = 0; // 下標

            // 目標圖像像素值
            double grayValue;

            #endregion

            //掃描X軸座標
            for (int x = 0; x < dstHeight; x++)
            {
                //掃描Y軸座標
                for (int y = 0; y < dstWidth; y++, dst++)
                {
                    grayValue = srcGrayData[x * srcStride + y] * Math.Pow((-1), (x + y));//取第x行第y個
                    /*
                    data[x , y].Re = (int)grayValue;
                    FourierTransform.FFT2(data, FourierTransform.Direction.Forward);//傅立葉正向轉換
                    */
                }
                dst += dstOffset;//偏移圖片位置
            }
            Marshal.Copy(dstGrayData, 0, dstPtr, dstBytes);//處理完的陣列複製到dstPtr指標
        }
*/
    }
}
