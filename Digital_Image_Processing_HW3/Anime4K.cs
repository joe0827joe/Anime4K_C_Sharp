using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace Anime4K
{
    public sealed class ImageProcess
    {
        // 將 sRGB 0..255 轉線性 0..1
        private static double SrgbToLinear(byte v)
        {
            double c = v / 255.0;
            if (c <= 0.04045) return c / 12.92;
            return Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        // 將線性 0..1 轉 sRGB 0..255
        private static byte LinearToSrgb(double l)
        {
            double c = l <= 0.0031308 ? 12.92 * l : 1.055 * Math.Pow(l, 1.0 / 2.4) - 0.055;
            int v = (int)Math.Round(Math.Max(0.0, Math.Min(1.0, c)) * 255.0);
            return (byte)v;
        }

        // Rec.709 亮度（sRGB 直接套係數的近似）
        private static int Luma709(byte r, byte g, byte b)
        {
            return clamp((int)Math.Round(0.2126 * r + 0.7152 * g + 0.0722 * b), 0, 255);
        }

        //取得亮度(採用 Rec.709 近似)，並放入Alpha通道
        public static void ComputeLuminance(ref Bitmap bm)
        {
            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    var pixel = bm.GetPixel(x, y);
                    // Rec.709 感知亮度近似（未做 gamma 線性化以維持最小侵入）
                    int y709 = clamp((int)Math.Round(0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B), 0, 0xFF);
                    bm.SetPixel(x, y, Color.FromArgb(y709, pixel.R, pixel.G, pixel.B));
                }
            }
        }

        // 使用 LockBits 計算亮度遮罩，不動用 Alpha
        public static byte[] ComputeLuminanceMask(Bitmap bm)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap src = bm.Clone(rect, PixelFormat.Format32bppArgb);
            BitmapData data = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int stride = data.Stride;
                int w = bm.Width, h = bm.Height;
                byte[] mask = new byte[w * h];
                unsafe
                {
                    byte* basePtr = (byte*)data.Scan0;
                    for (int y = 0; y < h; y++)
                    {
                        byte* row = basePtr + y * stride;
                        int idx = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            byte b = row[x * 4 + 0];
                            byte g = row[x * 4 + 1];
                            byte r = row[x * 4 + 2];
                            mask[idx + x] = (byte)Luma709(r, g, b);
                        }
                    }
                }
                return mask;
            }
            finally
            {
                src.UnlockBits(data);
                src.Dispose();
            }
        }

        // 對亮度遮罩做 Sobel，輸出 0..255 的梯度強度遮罩（未反相）
        public static byte[] ComputeGradientMask(byte[] lumaMask, int width, int height)
        {
            int w = width, h = height;
            byte[] grad = new byte[w * h];
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int tl = lumaMask[(y - 1) * w + (x - 1)];
                    int tc = lumaMask[(y - 1) * w + x];
                    int tr = lumaMask[(y - 1) * w + (x + 1)];
                    int ml = lumaMask[y * w + (x - 1)];
                    int mc = lumaMask[y * w + x];
                    int mr = lumaMask[y * w + (x + 1)];
                    int bl = lumaMask[(y + 1) * w + (x - 1)];
                    int bc = lumaMask[(y + 1) * w + x];
                    int br = lumaMask[(y + 1) * w + (x + 1)];

                    int dx = -tl + tr - (ml << 1) + (mr << 1) - bl + br;
                    int dy = -tl - (tc << 1) - tr + bl + (bc << 1) + br;
                    int mag = (int)Math.Min(255.0, Math.Sqrt(dx * dx + dy * dy));
                    grad[y * w + x] = (byte)mag;
                }
            }
            return grad;
        }

        //放入顏色基於亮度值
        public static void PushColor(ref Bitmap bm, int strength)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap temp = bm.Clone(rect, PixelFormat.Format32bppArgb);

            for (int x = 1; x < bm.Width - 1; x++)
            {
                for (int y = 1; y < bm.Height - 1; y++)
                {
                    #region 參數
                    //定義轉換常數
                    int xn = -1;
                    int xp = 1;
                    int yn = -1;
                    int yp = 1;

                    /*
                     * Kernel 定義:
                     * --------------
                     * [tl] [tc] [tr]
                     * [ml] [mc] [mr]
                     * [bl] [bc] [br]
                     * --------------
                     */

                    //首部
                    var tl = bm.GetPixel(x + xn, y + yn);
                    var tc = bm.GetPixel(x, y + yn);
                    var tr = bm.GetPixel(x + xp, y + yn);

                    //中間
                    var ml = bm.GetPixel(x + xn, y);
                    var mc = bm.GetPixel(x, y);
                    var mr = bm.GetPixel(x + xp, y);

                    //底部
                    var bl = bm.GetPixel(x + xn, y + yp);
                    var bc = bm.GetPixel(x, y + yp);
                    var br = bm.GetPixel(x + xp, y + yp);

                    var lightestColor = bm.GetPixel(x, y);
                    #endregion

                    //Kernel 0 . 4
                    float maxDark = max3A(br, bc, bl);
                    float minLight = min3A(tl, tc, tr);

                    if (minLight > mc.A && minLight > maxDark)
                    {
                        lightestColor = getLargest(mc, lightestColor, tl, tc, tr, strength);
                    }
                    else
                    {
                        maxDark = max3A(tl, tc, tr);
                        minLight = min3A(br, bc, bl);
                        if (minLight > mc.A && minLight > maxDark)
                        {
                            lightestColor = getLargest(mc, lightestColor, br, bc, bl, strength);
                        }
                    }

                    //Kernel 1 . 5
                    maxDark = max3A(mc, ml, bc);
                    minLight = min3A(mr, tc, tr);

                    if (minLight > maxDark)
                    {
                        lightestColor = getLargest(mc, lightestColor, mr, tc, tr, strength);
                    }
                    else
                    {
                        maxDark = max3A(mc, mr, tc);
                        minLight = min3A(bl, ml, bc);
                        if (minLight > maxDark)
                        {
                            lightestColor = getLargest(mc, lightestColor, bl, ml, bc, strength);
                        }
                    }

                    //Kernel 2 . 6
                    maxDark = max3A(ml, tl, bl);
                    minLight = min3A(mr, br, tr);

                    if (minLight > mc.A && minLight > maxDark)
                    {
                        lightestColor = getLargest(mc, lightestColor, mr, br, tr, strength);
                    }
                    else
                    {
                        maxDark = max3A(mr, br, tr);
                        minLight = min3A(ml, tl, bl);
                        if (minLight > mc.A && minLight > maxDark)
                        {
                            lightestColor = getLargest(mc, lightestColor, ml, tl, bl, strength);
                        }
                    }

                    //Kernel 3 . 7
                    maxDark = max3A(mc, ml, tc);
                    minLight = min3A(mr, br, bc);

                    if (minLight > maxDark)
                    {
                        lightestColor = getLargest(mc, lightestColor, mr, br, bc, strength);
                    }
                    else
                    {
                        maxDark = max3A(mc, mr, bc);
                        minLight = min3A(tc, ml, tl);
                        if (minLight > maxDark)
                        {
                            lightestColor = getLargest(mc, lightestColor, tc, ml, tl, strength);
                        }
                    }

                    temp.SetPixel(x, y, lightestColor);
                }
            }

            //寫入結果回bm
            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }

        //計算梯度，採用Sobel運算子
        public static void ComputeGradient(ref Bitmap bm)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap temp = bm.Clone(rect, PixelFormat.Format32bppArgb);

            //Sobel運算子
            int[][] sobelx = {new int[] {-1, 0, 1},
                              new int[] {-2, 0, 2},
                              new int[] {-1, 0, 1}};

            int[][] sobely = {new int[] {-1, -2, -1},
                              new int[] { 0, 0, 0},
                              new int[] { 1, 2, 1}};
            //捲積
            for (int x = 1; x < bm.Width - 1; x++)
            {
                for (int y = 1; y < bm.Height - 1; y++)
                {
                    int dx = bm.GetPixel(x - 1, y - 1).A * sobelx[0][0] + bm.GetPixel(x, y - 1).A * sobelx[0][1] + bm.GetPixel(x + 1, y - 1).A * sobelx[0][2]
                              + bm.GetPixel(x - 1, y).A * sobelx[1][0] + bm.GetPixel(x, y).A * sobelx[1][1] + bm.GetPixel(x + 1, y).A * sobelx[1][2]
                              + bm.GetPixel(x - 1, y + 1).A * sobelx[2][0] + bm.GetPixel(x, y + 1).A * sobelx[2][1] + bm.GetPixel(x + 1, y + 1).A * sobelx[2][2];

                    int dy = bm.GetPixel(x - 1, y - 1).A * sobely[0][0] + bm.GetPixel(x, y - 1).A * sobely[0][1] + bm.GetPixel(x + 1, y - 1).A * sobely[0][2]
                           + bm.GetPixel(x - 1, y).A * sobely[1][0] + bm.GetPixel(x, y).A * sobely[1][1] + bm.GetPixel(x + 1, y).A * sobely[1][2]
                           + bm.GetPixel(x - 1, y + 1).A * sobely[2][0] + bm.GetPixel(x, y + 1).A * sobely[2][1] + bm.GetPixel(x + 1, y + 1).A * sobely[2][2];
                    double derivata = Math.Sqrt((dx * dx) + (dy * dy));

                    var pixel = bm.GetPixel(x, y);
                    //反轉圖片結果
                    if (derivata > 255)
                    {
                        temp.SetPixel(x, y, Color.FromArgb(0, pixel.R, pixel.G, pixel.B));
                    }
                    else
                    {
                        temp.SetPixel(x, y, Color.FromArgb(0xFF-(int)derivata, pixel.R, pixel.G, pixel.B));
                    }
                }
            }

            //寫入結果回bm
            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }

        /*
        //計算梯度，採用Sobel運算子
        public static void ComputeGradient(ref Bitmap bm)
        {
            Bitmap temp = new Bitmap(bm.Width, bm.Height);

            for (int x = 0; x < bm.Width - 1; x++)
            {
                for (int y = 0; y < bm.Height - 1; y++)
                {
                    #region 參數
                    //定義轉換常數
                    int xn = -1;
                    int xp = 1;
                    int yn = -1;
                    int yp = 1;

                    //如果靠到圖片邊界，則轉換常數設定為邊界上
                    if (x == 0)
                        xn = 0;
                    else if (x == bm.Width - 1)
                        xp = 0;

                    if (y == 0)
                        yn = 0;
                    else if (y == bm.Height - 1)
                        yp = 0;

                    //首部
                    var tl = bm.GetPixel(x + xn, y + yn);
                    var tc = bm.GetPixel(x, y + yn);
                    var tr = bm.GetPixel(x + xp, y + yn);

                    //中間
                    var ml = bm.GetPixel(x + xn, y);
                    var mc = bm.GetPixel(x, y);
                    var mr = bm.GetPixel(x + xp, y);

                    //底部
                    var bl = bm.GetPixel(x + xn, y + yp);
                    var bc = bm.GetPixel(x, y + yp);
                    var br = bm.GetPixel(x + xp, y + yp);

                    int xgrad = (-tl.A + tr.A - ml.A - ml.A + mr.A + mr.A - bl.A + br.A);
                    int ygrad = (-tl.A - tc.A - tc.A - tr.A + bl.A + bc.A + bc.A + br.A);

                    double derivata = Math.Sqrt((xgrad * xgrad) + (ygrad * ygrad));
                    #endregion

                    if (derivata > 255)
                    {
                       temp.SetPixel(x, y, Color.FromArgb(255, mc.R, mc.G, mc.B));
                    }
                    else
                    {
                        temp.SetPixel(x, y, Color.FromArgb((int)derivata, mc.R, mc.G, mc.B));
                    }
                }
            }

            //寫入結果回bm
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }
        */

        //放入顏色基於梯度強度
        public static void PushGradient(ref Bitmap bm, int strength)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap temp = bm.Clone(rect, PixelFormat.Format32bppArgb);

            for (int x = 1; x < bm.Width - 1; x++)
            {
                for (int y = 1; y < bm.Height - 1; y++)
                {
                    #region 參數
                    //定義轉換常數
                    int xn = -1;
                    int xp = 1;
                    int yn = -1;
                    int yp = 1;

                    //首部
                    var tl = bm.GetPixel(x + xn, y + yn);
                    var tc = bm.GetPixel(x, y + yn);
                    var tr = bm.GetPixel(x + xp, y + yn);

                    //中間
                    var ml = bm.GetPixel(x + xn, y);
                    var mc = bm.GetPixel(x, y);
                    var mr = bm.GetPixel(x + xp, y);

                    //底部
                    var bl = bm.GetPixel(x + xn, y + yp);
                    var bc = bm.GetPixel(x, y + yp);
                    var br = bm.GetPixel(x + xp, y + yp);
                    #endregion

                    var lightestColor = bm.GetPixel(x, y);

                    //Kernel 0 . 4
                    float maxDark = max3A(br, bc, bl);
                    float minLight = min3A(tl, tc, tr);

                    if (minLight > mc.A && minLight > maxDark)
                    {
                        lightestColor = getAverage(mc, tl, tc, tr, strength);
                    }
                    else
                    {
                        maxDark = max3A(tl, tc, tr);
                        minLight = min3A(br, bc, bl);
                        if (minLight > mc.A && minLight > maxDark)
                        {
                            lightestColor = getAverage(mc, br, bc, bl, strength);
                        }
                    }

                    //Kernel 1 . 5
                    maxDark = max3A(mc, ml, bc);
                    minLight = min3A(mr, tc, tr);

                    if (minLight > maxDark)
                    {
                        lightestColor = getAverage(mc, mr, tc, tr, strength);
                    }
                    else
                    {
                        maxDark = max3A(mc, mr, tc);
                        minLight = min3A(bl, ml, bc);
                        if (minLight > maxDark)
                        {
                            lightestColor = getAverage(mc, bl, ml, bc, strength);
                        }
                    }

                    //Kernel 2 . 6
                    maxDark = max3A(ml, tl, bl);
                    minLight = min3A(mr, br, tr);

                    if (minLight > mc.A && minLight > maxDark)
                    {
                        lightestColor = getAverage(mc, mr, br, tr, strength);
                    }
                    else
                    {
                        maxDark = max3A(mr, br, tr);
                        minLight = min3A(ml, tl, bl);
                        if (minLight > mc.A && minLight > maxDark)
                        {
                            lightestColor = getAverage(mc, ml, tl, bl, strength);
                        }
                    }

                    //Kernel 3 . 7
                    maxDark = max3A(mc, ml, tc);
                    minLight = min3A(mr, br, bc);

                    if (minLight > maxDark)
                    {
                        lightestColor = getAverage(mc, mr, br, bc, strength);
                    }
                    else
                    {
                        maxDark = max3A(mc, mr, bc);
                        minLight = min3A(tc, ml, tl);
                        if (minLight > maxDark)
                        {
                            lightestColor = getAverage(mc, tc, ml, tl, strength);
                        }
                    }

                    //寫入結果回bm
                    lightestColor = Color.FromArgb(255, lightestColor.R, lightestColor.G, lightestColor.B);
                    temp.SetPixel(x, y, lightestColor);
                }
            }

            // 將整張圖的 Alpha 設為 255，避免中途步驟殘留的透明度
            for (int x = 0; x < temp.Width; x++)
            {
                for (int y = 0; y < temp.Height; y++)
                {
                    var p = temp.GetPixel(x, y);
                    if (p.A != 255) temp.SetPixel(x, y, Color.FromArgb(255, p.R, p.G, p.B));
                }
            }

            //寫入結果回bm
            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }

        // 基於亮度遮罩的線性空間 PushColor
        public static void PushColorLinearWithMask(ref Bitmap bm, byte[] lumaMask, int strength)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap temp = bm.Clone(rect, PixelFormat.Format32bppArgb);
            double s = Math.Max(0, Math.Min(255, strength)) / 255.0;

            for (int x = 1; x < bm.Width - 1; x++)
            {
                for (int y = 1; y < bm.Height - 1; y++)
                {
                    int w = bm.Width; int idx = y * w + x;

                    // 取鄰域亮度
                    int tlA = lumaMask[(y - 1) * w + (x - 1)];
                    int tcA = lumaMask[(y - 1) * w + x];
                    int trA = lumaMask[(y - 1) * w + (x + 1)];
                    int mlA = lumaMask[y * w + (x - 1)];
                    int mcA = lumaMask[y * w + x];
                    int mrA = lumaMask[y * w + (x + 1)];
                    int blA = lumaMask[(y + 1) * w + (x - 1)];
                    int bcA = lumaMask[(y + 1) * w + x];
                    int brA = lumaMask[(y + 1) * w + (x + 1)];

                    Color mc = bm.GetPixel(x, y);
                    // 初始為中心像素
                    Color best = mc;
                    int bestLum = mcA;

                    // 定義一個內部函式：給三點取線性平均並與中心做線性混合
                    Func<Color, Color, Color, Color> mix3 = (a, b, c) =>
                    {
                        double r0 = SrgbToLinear(mc.R), g0 = SrgbToLinear(mc.G), b0 = SrgbToLinear(mc.B);
                        double r1 = (SrgbToLinear(a.R) + SrgbToLinear(b.R) + SrgbToLinear(c.R)) / 3.0;
                        double g1 = (SrgbToLinear(a.G) + SrgbToLinear(b.G) + SrgbToLinear(c.G)) / 3.0;
                        double b1 = (SrgbToLinear(a.B) + SrgbToLinear(b.B) + SrgbToLinear(c.B)) / 3.0;
                        double r = r0 * (1.0 - s) + r1 * s;
                        double g = g0 * (1.0 - s) + g1 * s;
                        double b2 = b0 * (1.0 - s) + b1 * s;
                        return Color.FromArgb(255, LinearToSrgb(r), LinearToSrgb(g), LinearToSrgb(b2));
                    };

                    // Kernel 0/4
                    int maxDark = Math.Max(Math.Max(brA, bcA), blA);
                    int minLight = Math.Min(Math.Min(tlA, tcA), trA);
                    if (minLight > mcA && minLight > maxDark)
                    {
                        Color cand = mix3(bm.GetPixel(x - 1, y - 1), bm.GetPixel(x, y - 1), bm.GetPixel(x + 1, y - 1));
                        int lum = Luma709(cand.R, cand.G, cand.B);
                        if (lum > bestLum) { bestLum = lum; best = cand; }
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(tlA, tcA), trA);
                        minLight = Math.Min(Math.Min(brA, bcA), blA);
                        if (minLight > mcA && minLight > maxDark)
                        {
                            Color cand = mix3(bm.GetPixel(x + 1, y + 1), bm.GetPixel(x, y + 1), bm.GetPixel(x - 1, y + 1));
                            int lum = Luma709(cand.R, cand.G, cand.B);
                            if (lum > bestLum) { bestLum = lum; best = cand; }
                        }
                    }

                    // Kernel 1/5
                    maxDark = Math.Max(Math.Max(mcA, mlA), bcA);
                    minLight = Math.Min(Math.Min(mrA, tcA), trA);
                    if (minLight > maxDark)
                    {
                        Color cand = mix3(bm.GetPixel(x + 1, y), bm.GetPixel(x, y - 1), bm.GetPixel(x + 1, y - 1));
                        int lum = Luma709(cand.R, cand.G, cand.B);
                        if (lum > bestLum) { bestLum = lum; best = cand; }
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(mcA, mrA), tcA);
                        minLight = Math.Min(Math.Min(blA, mlA), bcA);
                        if (minLight > maxDark)
                        {
                            Color cand = mix3(bm.GetPixel(x, y + 1), bm.GetPixel(x - 1, y), bm.GetPixel(x, y + 1));
                            int lum = Luma709(cand.R, cand.G, cand.B);
                            if (lum > bestLum) { bestLum = lum; best = cand; }
                        }
                    }

                    // Kernel 2/6
                    maxDark = Math.Max(Math.Max(mlA, tlA), blA);
                    minLight = Math.Min(Math.Min(mrA, brA), trA);
                    if (minLight > mcA && minLight > maxDark)
                    {
                        Color cand = mix3(bm.GetPixel(x + 1, y), bm.GetPixel(x + 1, y + 1), bm.GetPixel(x + 1, y - 1));
                        int lum = Luma709(cand.R, cand.G, cand.B);
                        if (lum > bestLum) { bestLum = lum; best = cand; }
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(mrA, brA), trA);
                        minLight = Math.Min(Math.Min(mlA, tlA), blA);
                        if (minLight > mcA && minLight > maxDark)
                        {
                            Color cand = mix3(bm.GetPixel(x - 1, y), bm.GetPixel(x - 1, y - 1), bm.GetPixel(x - 1, y + 1));
                            int lum = Luma709(cand.R, cand.G, cand.B);
                            if (lum > bestLum) { bestLum = lum; best = cand; }
                        }
                    }

                    // Kernel 3/7
                    maxDark = Math.Max(Math.Max(mcA, mlA), tcA);
                    minLight = Math.Min(Math.Min(mrA, brA), bcA);
                    if (minLight > maxDark)
                    {
                        Color cand = mix3(bm.GetPixel(x + 1, y), bm.GetPixel(x + 1, y + 1), bm.GetPixel(x, y + 1));
                        int lum = Luma709(cand.R, cand.G, cand.B);
                        if (lum > bestLum) { bestLum = lum; best = cand; }
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(mcA, mrA), bcA);
                        minLight = Math.Min(Math.Min(tcA, mlA), tlA);
                        if (minLight > maxDark)
                        {
                            Color cand = mix3(bm.GetPixel(x, y - 1), bm.GetPixel(x - 1, y), bm.GetPixel(x - 1, y - 1));
                            int lum = Luma709(cand.R, cand.G, cand.B);
                            if (lum > bestLum) { bestLum = lum; best = cand; }
                        }
                    }

                    temp.SetPixel(x, y, best);
                }
            }

            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }

        // 基於梯度遮罩的線性空間 PushGradient
        public static void PushGradientLinearWithMask(ref Bitmap bm, byte[] gradMask, int strength)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap temp = bm.Clone(rect, PixelFormat.Format32bppArgb);
            double s = Math.Max(0, Math.Min(255, strength)) / 255.0;

            for (int x = 1; x < bm.Width - 1; x++)
            {
                for (int y = 1; y < bm.Height - 1; y++)
                {
                    int w = bm.Width; int idx = y * w + x;
                    int tlA = gradMask[(y - 1) * w + (x - 1)];
                    int tcA = gradMask[(y - 1) * w + x];
                    int trA = gradMask[(y - 1) * w + (x + 1)];
                    int mlA = gradMask[y * w + (x - 1)];
                    int mcA = gradMask[y * w + x];
                    int mrA = gradMask[y * w + (x + 1)];
                    int blA = gradMask[(y + 1) * w + (x - 1)];
                    int bcA = gradMask[(y + 1) * w + x];
                    int brA = gradMask[(y + 1) * w + (x + 1)];

                    Color mc = bm.GetPixel(x, y);
                    Color best = mc;

                    Func<Color, Color, Color, Color> mix3 = (a, b, c) =>
                    {
                        double r0 = SrgbToLinear(mc.R), g0 = SrgbToLinear(mc.G), b0 = SrgbToLinear(mc.B);
                        double r1 = (SrgbToLinear(a.R) + SrgbToLinear(b.R) + SrgbToLinear(c.R)) / 3.0;
                        double g1 = (SrgbToLinear(a.G) + SrgbToLinear(b.G) + SrgbToLinear(c.G)) / 3.0;
                        double b1 = (SrgbToLinear(a.B) + SrgbToLinear(b.B) + SrgbToLinear(c.B)) / 3.0;
                        double r = r0 * (1.0 - s) + r1 * s;
                        double g = g0 * (1.0 - s) + g1 * s;
                        double b2 = b0 * (1.0 - s) + b1 * s;
                        return Color.FromArgb(255, LinearToSrgb(r), LinearToSrgb(g), LinearToSrgb(b2));
                    };

                    int maxDark = Math.Max(Math.Max(brA, bcA), blA);
                    int minLight = Math.Min(Math.Min(tlA, tcA), trA);
                    if (minLight > mcA && minLight > maxDark)
                    {
                        best = mix3(bm.GetPixel(x - 1, y - 1), bm.GetPixel(x, y - 1), bm.GetPixel(x + 1, y - 1));
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(tlA, tcA), trA);
                        minLight = Math.Min(Math.Min(brA, bcA), blA);
                        if (minLight > mcA && minLight > maxDark)
                        {
                            best = mix3(bm.GetPixel(x + 1, y + 1), bm.GetPixel(x, y + 1), bm.GetPixel(x - 1, y + 1));
                        }
                    }

                    maxDark = Math.Max(Math.Max(mcA, mlA), bcA);
                    minLight = Math.Min(Math.Min(mrA, tcA), trA);
                    if (minLight > maxDark)
                    {
                        best = mix3(bm.GetPixel(x + 1, y), bm.GetPixel(x, y - 1), bm.GetPixel(x + 1, y - 1));
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(mcA, mrA), tcA);
                        minLight = Math.Min(Math.Min(blA, mlA), bcA);
                        if (minLight > maxDark)
                        {
                            best = mix3(bm.GetPixel(x, y + 1), bm.GetPixel(x - 1, y), bm.GetPixel(x, y + 1));
                        }
                    }

                    maxDark = Math.Max(Math.Max(mlA, tlA), blA);
                    minLight = Math.Min(Math.Min(mrA, brA), trA);
                    if (minLight > mcA && minLight > maxDark)
                    {
                        best = mix3(bm.GetPixel(x + 1, y), bm.GetPixel(x + 1, y + 1), bm.GetPixel(x + 1, y - 1));
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(mrA, brA), trA);
                        minLight = Math.Min(Math.Min(mlA, tlA), blA);
                        if (minLight > mcA && minLight > maxDark)
                        {
                            best = mix3(bm.GetPixel(x - 1, y), bm.GetPixel(x - 1, y - 1), bm.GetPixel(x - 1, y + 1));
                        }
                    }

                    maxDark = Math.Max(Math.Max(mcA, mlA), tcA);
                    minLight = Math.Min(Math.Min(mrA, brA), bcA);
                    if (minLight > maxDark)
                    {
                        best = mix3(bm.GetPixel(x + 1, y), bm.GetPixel(x + 1, y + 1), bm.GetPixel(x, y + 1));
                    }
                    else
                    {
                        maxDark = Math.Max(Math.Max(mcA, mrA), bcA);
                        minLight = Math.Min(Math.Min(tcA, mlA), tlA);
                        if (minLight > maxDark)
                        {
                            best = mix3(bm.GetPixel(x, y - 1), bm.GetPixel(x - 1, y), bm.GetPixel(x - 1, y - 1));
                        }
                    }

                    temp.SetPixel(x, y, best);
                }
            }

            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }

        //控制在min~max範圍
        private static int clamp(int i, int min, int max)
        {
            return Math.Max(Math.Min(i,max),min);
        }

        //取得最小值
        private static int min3A(Color a, Color b, Color c)
        {
            return Math.Min(Math.Min(a.A, b.A), c.A);
        }

        //取得最大值
        private static int max3A(Color a, Color b, Color c)
        {
            return Math.Max(Math.Max(a.A, b.A), c.A);
        }

        //周圍三點像素加權平均
        private static Color getAverage(Color cc, Color a, Color b, Color c, int strength)
        {
            int ra = (cc.R * (0xFF - strength) + ((a.R + b.R + c.R) / 3) * strength) / 0xFF;
            int ga = (cc.G * (0xFF - strength) + ((a.G + b.G + c.G) / 3) * strength) / 0xFF;
            int ba = (cc.B * (0xFF - strength) + ((a.B + b.B + c.B) / 3) * strength) / 0xFF;
            int aa = (cc.A * (0xFF - strength) + ((a.A + b.A + c.A) / 3) * strength) / 0xFF;

            return Color.FromArgb(aa, ra, ga, ba);
        }

        //最亮值與加權平均取最大
        private static Color getLargest(Color cc, Color lightestColor, Color a, Color b, Color c, int strength)
        {
            var newColor = getAverage(cc, a, b, c, strength);
            return newColor.A > lightestColor.A ? newColor : lightestColor;
        }

        // 將梯度值做門檻與 gamma 曲線，輸出 0..1 權重
        private static double NormalizeGradWeight(byte grad, byte threshold, double gamma)
        {
            if (grad <= threshold) return 0.0;
            double t = (grad - threshold) / (255.0 - threshold);
            t = Math.Max(0.0, Math.Min(1.0, t));
            return Math.Pow(t, Math.Max(0.1, gamma));
        }

        // 單次方向性三點推動（線性空間），含梯度權重、變化量限制與最佳方向選擇
        public static void CombinedDirectionalPushLinear(
            ref Bitmap bm,
            byte[] lumaMask,
            byte[] gradMask,
            int colorStrength,
            int gradStrength,
            int maxChannelDelta = 8,
            byte gradThreshold = 24,
            double gradGamma = 1.5)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap src = bm.Clone(rect, PixelFormat.Format32bppArgb);
            Bitmap dst = new Bitmap(bm.Width, bm.Height, PixelFormat.Format32bppArgb);

            BitmapData sData = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dData = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                int w = bm.Width, h = bm.Height;
                int sStride = sData.Stride, dStride = dData.Stride;

                unsafe
                {
                    byte* sBase = (byte*)sData.Scan0;
                    byte* dBase = (byte*)dData.Scan0;

                    for (int y = 0; y < h; y++)
                    {
                        byte* sRow = sBase + y * sStride;
                        byte* dRow = dBase + y * dStride;
                        for (int x = 0; x < w; x++)
                        {
                            // 邊界直接拷貝
                            if (x == 0 || y == 0 || x == w - 1 || y == h - 1)
                            {
                                byte b = sRow[x * 4 + 0];
                                byte g = sRow[x * 4 + 1];
                                byte r = sRow[x * 4 + 2];
                                dRow[x * 4 + 0] = b;
                                dRow[x * 4 + 1] = g;
                                dRow[x * 4 + 2] = r;
                                dRow[x * 4 + 3] = 255;
                                continue;
                            }

                            // 讀中心像素（BGRA）
                            byte b0 = sRow[x * 4 + 0];
                            byte g0 = sRow[x * 4 + 1];
                            byte r0 = sRow[x * 4 + 2];

                            // 當地梯度權重
                            double wGrad = NormalizeGradWeight(gradMask[y * w + x], gradThreshold, gradGamma);

                            // 計算四組方向三點平均候選（在線性空間）
                            Color candBest = Color.FromArgb(255, r0, g0, b0);
                            int bestLum = lumaMask[y * w + x];

                            // helper 取顏色
                            byte* rowN = sBase + (y - 1) * sStride;
                            byte* rowC = sBase + y * sStride;
                            byte* rowS = sBase + (y + 1) * sStride;

                            // 取鄰域顏色的函式
                            Func<int, int, Color> pix = (dx, dy) =>
                            {
                                byte* p = sBase + (y + dy) * sStride + (x + dx) * 4;
                                return Color.FromArgb(255, p[2], p[1], p[0]);
                            };

                            // 線性空間三點平均 + 與中心做線性混合（待會套強度）
                            Func<Color, Color, Color, Color> mean3 = (a, b, c) =>
                            {
                                double r1 = (SrgbToLinear(a.R) + SrgbToLinear(b.R) + SrgbToLinear(c.R)) / 3.0;
                                double g1 = (SrgbToLinear(a.G) + SrgbToLinear(b.G) + SrgbToLinear(c.G)) / 3.0;
                                double b1 = (SrgbToLinear(a.B) + SrgbToLinear(b.B) + SrgbToLinear(c.B)) / 3.0;
                                return Color.FromArgb(255, LinearToSrgb(r1), LinearToSrgb(g1), LinearToSrgb(b1));
                            };

                            // 亮暗條件（與原邏輯一致）
                            int tlA = lumaMask[(y - 1) * w + (x - 1)];
                            int tcA = lumaMask[(y - 1) * w + x];
                            int trA = lumaMask[(y - 1) * w + (x + 1)];
                            int mlA = lumaMask[y * w + (x - 1)];
                            int mcA = lumaMask[y * w + x];
                            int mrA = lumaMask[y * w + (x + 1)];
                            int blA = lumaMask[(y + 1) * w + (x - 1)];
                            int bcA = lumaMask[(y + 1) * w + x];
                            int brA = lumaMask[(y + 1) * w + (x + 1)];

                            Action<Color, int> consider = (cand, lum) =>
                            {
                                if (lum > bestLum)
                                {
                                    bestLum = lum;
                                    candBest = cand;
                                }
                            };

                            // Kernel 0/4（上列 vs 下列）
                            int maxDark = Math.Max(Math.Max(brA, bcA), blA);
                            int minLight = Math.Min(Math.Min(tlA, tcA), trA);
                            if (minLight > mcA && minLight > maxDark)
                            {
                                var cand = mean3(pix(-1, -1), pix(0, -1), pix(1, -1));
                                consider(cand, Luma709(cand.R, cand.G, cand.B));
                            }
                            else
                            {
                                maxDark = Math.Max(Math.Max(tlA, tcA), trA);
                                minLight = Math.Min(Math.Min(brA, bcA), blA);
                                if (minLight > mcA && minLight > maxDark)
                                {
                                    var cand = mean3(pix(1, 1), pix(0, 1), pix(-1, 1));
                                    consider(cand, Luma709(cand.R, cand.G, cand.B));
                                }
                            }

                            // Kernel 1/5（右上 vs 左下）
                            maxDark = Math.Max(Math.Max(mcA, mlA), bcA);
                            minLight = Math.Min(Math.Min(mrA, tcA), trA);
                            if (minLight > maxDark)
                            {
                                var cand = mean3(pix(1, 0), pix(0, -1), pix(1, -1));
                                consider(cand, Luma709(cand.R, cand.G, cand.B));
                            }
                            else
                            {
                                maxDark = Math.Max(Math.Max(mcA, mrA), tcA);
                                minLight = Math.Min(Math.Min(blA, mlA), bcA);
                                if (minLight > maxDark)
                                {
                                    var cand = mean3(pix(0, 1), pix(-1, 0), pix(0, 1));
                                    consider(cand, Luma709(cand.R, cand.G, cand.B));
                                }
                            }

                            // Kernel 2/6（右列 vs 左列）
                            maxDark = Math.Max(Math.Max(mlA, tlA), blA);
                            minLight = Math.Min(Math.Min(mrA, brA), trA);
                            if (minLight > mcA && minLight > maxDark)
                            {
                                var cand = mean3(pix(1, 0), pix(1, 1), pix(1, -1));
                                consider(cand, Luma709(cand.R, cand.G, cand.B));
                            }
                            else
                            {
                                maxDark = Math.Max(Math.Max(mrA, brA), trA);
                                minLight = Math.Min(Math.Min(mlA, tlA), blA);
                                if (minLight > mcA && minLight > maxDark)
                                {
                                    var cand = mean3(pix(-1, 0), pix(-1, -1), pix(-1, 1));
                                    consider(cand, Luma709(cand.R, cand.G, cand.B));
                                }
                            }

                            // Kernel 3/7（右下 vs 左上）
                            maxDark = Math.Max(Math.Max(mcA, mlA), tcA);
                            minLight = Math.Min(Math.Min(mrA, brA), bcA);
                            if (minLight > maxDark)
                            {
                                var cand = mean3(pix(1, 0), pix(1, 1), pix(0, 1));
                                consider(cand, Luma709(cand.R, cand.G, cand.B));
                            }
                            else
                            {
                                maxDark = Math.Max(Math.Max(mcA, mrA), bcA);
                                minLight = Math.Min(Math.Min(tcA, mlA), tlA);
                                if (minLight > maxDark)
                                {
                                    var cand = mean3(pix(0, -1), pix(-1, 0), pix(-1, -1));
                                    consider(cand, Luma709(cand.R, cand.G, cand.B));
                                }
                            }

                            // 計算實際混合強度（顏色推進 + 梯度權重）
                            double sColor = Math.Max(0, Math.Min(255, colorStrength)) / 255.0;
                            double sGrad = Math.Max(0, Math.Min(255, gradStrength)) / 255.0;
                            double s = Math.Max(0.0, Math.Min(1.0, sColor + sGrad * wGrad));

                            // 線性空間混合
                            double rLin0 = SrgbToLinear(r0);
                            double gLin0 = SrgbToLinear(g0);
                            double bLin0 = SrgbToLinear(b0);
                            double rLin1 = SrgbToLinear(candBest.R);
                            double gLin1 = SrgbToLinear(candBest.G);
                            double bLin1 = SrgbToLinear(candBest.B);

                            int rNew = LinearToSrgb(rLin0 * (1.0 - s) + rLin1 * s);
                            int gNew = LinearToSrgb(gLin0 * (1.0 - s) + gLin1 * s);
                            int bNew = LinearToSrgb(bLin0 * (1.0 - s) + bLin1 * s);

                            // 限制每通道變化量，避免過度擴散
                            rNew = clamp(rNew, r0 - maxChannelDelta, r0 + maxChannelDelta);
                            gNew = clamp(gNew, g0 - maxChannelDelta, g0 + maxChannelDelta);
                            bNew = clamp(bNew, b0 - maxChannelDelta, b0 + maxChannelDelta);

                            dRow[x * 4 + 0] = (byte)bNew;
                            dRow[x * 4 + 1] = (byte)gNew;
                            dRow[x * 4 + 2] = (byte)rNew;
                            dRow[x * 4 + 3] = 255;
                        }
                    }
                }
            }
            finally
            {
                src.UnlockBits(sData);
                dst.UnlockBits(dData);
            }

            bm.Dispose();
            bm = dst;
            src.Dispose();
        }

        // Unsharp Mask - 反遮罩銳化
        public static void UnsharpMask(ref Bitmap bm, float strength = 0.5f, float radius = 1.0f)
        {
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            Bitmap temp = bm.Clone(rect, PixelFormat.Format32bppArgb);
            
            // 簡化的高斯模糊近似 (3x3 kernel)
            float[,] gaussianKernel = {
                { 1f/16f, 2f/16f, 1f/16f },
                { 2f/16f, 4f/16f, 2f/16f },
                { 1f/16f, 2f/16f, 1f/16f }
            };

            for (int x = 1; x < bm.Width - 1; x++)
            {
                for (int y = 1; y < bm.Height - 1; y++)
                {
                    Color original = bm.GetPixel(x, y);
                    
                    // 計算高斯模糊
                    float r = 0, g = 0, b = 0;
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            Color pixel = bm.GetPixel(x + kx, y + ky);
                            float weight = gaussianKernel[ky + 1, kx + 1];
                            r += pixel.R * weight;
                            g += pixel.G * weight;
                            b += pixel.B * weight;
                        }
                    }
                    
                    // Unsharp Mask: original + strength * (original - blurred)
                    int newR = clamp((int)(original.R + strength * (original.R - r)), 0, 255);
                    int newG = clamp((int)(original.G + strength * (original.G - g)), 0, 255);
                    int newB = clamp((int)(original.B + strength * (original.B - b)), 0, 255);
                    
                    temp.SetPixel(x, y, Color.FromArgb(255, newR, newG, newB));
                }
            }

            bm = temp.Clone(rect, PixelFormat.Format32bppArgb);
            temp.Dispose();
        }
    }
}
