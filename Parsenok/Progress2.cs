using System;
using System.Text;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Parsenok
{
    public class Progress
    {
        public class ImageEffect
        {
            public class BitmapData
            {
                public byte[] argb;
                public System.Drawing.Imaging.BitmapData data;
                public Bitmap source;

                public IntPtr GetPtr { get { return data.Scan0; } }

                public BitmapData(Bitmap sourcebit, ref byte[] argbarray, System.Drawing.Imaging.BitmapData bitdata)
                {
                    source = sourcebit;
                    argb = argbarray;
                    data = bitdata;
                }
            }

            /// <summary>Получение указателя на биты битмапа</summary>
            public static BitmapData _LockAndGetBitmapData(Bitmap dr)
            {
                System.Drawing.Imaging.BitmapData bmpData =
                dr.LockBits(new Rectangle(0, 0, dr.Width, dr.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, dr.PixelFormat);

                byte[] rgbValues = new byte[bmpData.Stride * dr.Height];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, rgbValues.Length);

                return new BitmapData(dr, ref rgbValues, bmpData);
            }
            /// <summary>Освобождение битмапа</summary>
            public static Bitmap _UnlockAndUpdateBitmapData(BitmapData data)
            {
                System.Runtime.InteropServices.Marshal.Copy(data.argb, 0, data.GetPtr, data.argb.Length);
                data.source.UnlockBits(data.data);
                return data.source;
            }
            /// <summary>ПОлучение количества байтов на позицию</summary>
            public static int _GetPixelWeight(BitmapData data) { return data.data.Stride / data.source.Width; }
            /// <summary>Взятие цвета из указаной позиции</summary>
            public static Color _GetBitmapDataPixel(BitmapData data, int x, int y) { return _GetBitmapDataPixel(data, data.data.Width * y + x); }
            /// <summary>Взятие цвета из указаной позиции с пересчетом позиции в байты</summary>
            public static Color _GetBitmapDataPixel(BitmapData data, int commonpos)
            {
                int pos = _GetPixelWeight(data) * commonpos;
                if (pos >= data.argb.Length || pos < 0) return Color.Empty;

                switch (data.source.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                        return Color.FromArgb(data.argb[pos + 3], data.argb[pos + 2], data.argb[pos + 1], data.argb[pos]);
                    default: return Color.Empty;
                }
            }
            /// <summary>Взятие цвета из указаной позиции без пересчета позиции в байты</summary>
            public static Color _GetBitmapDataPixelByBPOS(BitmapData data, int bytepos)
            {
                if (bytepos >= data.argb.Length || bytepos < 0) return Color.Empty;

                switch (data.source.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                        return Color.FromArgb(data.argb[bytepos + 3], data.argb[bytepos + 2], data.argb[bytepos + 1], data.argb[bytepos]);
                    default: return Color.Empty;
                }
            }
            /// <summary>Установка цвета в указаной позиции</summary>
            public static bool _SetBitmapDataPixel(BitmapData data, int x, int y, Color color) { return _SetBitmapDataPixel(data, data.data.Width * y + x, color); }
            /// <summary>Установка цвета в указаной позиции с пересчетом позиции в байты</summary>
            public static bool _SetBitmapDataPixel(BitmapData data, int commonpos, Color color)
            {
                int pos = _GetPixelWeight(data) * commonpos;
                if (pos >= data.argb.Length || pos < 0) return false;

                switch (data.source.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                        data.argb[pos + 3] = color.A;
                        data.argb[pos + 2] = color.R;
                        data.argb[pos + 1] = color.G;
                        data.argb[pos] = color.B;
                        return true;
                    default: return false;
                }
            }
            /// <summary>Установка цвета в указаной позиции без пересчета позиции в байты</summary>
            public static bool _SetBitmapDataPixelbyBPOS(BitmapData data, int bytepos, Color color)
            {
                if (bytepos >= data.argb.Length || bytepos < 0) return false;

                switch (data.source.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                        data.argb[bytepos + 3] = color.A;
                        data.argb[bytepos + 2] = color.R;
                        data.argb[bytepos + 1] = color.G;
                        data.argb[bytepos] = color.B;
                        return true;
                    default: return false;
                }
            }

            public static Bitmap IncludePict(Bitmap target,Bitmap include,Point startpos)
            {
                BitmapData res = _LockAndGetBitmapData(target);//new Bitmap(target, target.Width, target.Height));
                BitmapData incres = _LockAndGetBitmapData(include);
                int step = _GetPixelWeight(res);

                /*for (int loop1 = 0, loop2 = res.data.Width * startpos.Y-1; loop1 < incres.argb.Length; loop1 += step)
                {
                    if ((loop1) % incres.data.Width == 0) loop2+=res.data.Width * step;

                    if (_GetBitmapDataPixelByBPOS(incres, loop1+3).A != 0)
                    {
                        int loop = 0;
                        //Color c = InvertColour();
                    }
                }*/

                for (int loop1 = 0; loop1 < include.Width; loop1++)
                    for (int loop2 = 0; loop2 < include.Height; loop2++)
                        if (_GetBitmapDataPixel(incres, loop1, loop2).A != 0)
                            _SetBitmapDataPixel(res, loop1 + startpos.X, loop2 + startpos.Y, InvertColour(_GetBitmapDataPixel(res,loop1 + startpos.X, loop2 + startpos.Y)));

                return _UnlockAndUpdateBitmapData(res);
            }

            public static Bitmap Antialiasing(Bitmap target)
            {
                BitmapData res = _LockAndGetBitmapData(new Bitmap(target, target.Width, target.Height));
                int step = _GetPixelWeight(res);

                for (int loop1 = 0; loop1 < res.argb.Length; loop1 += step)
                    GetOctetMixColor(res, loop1, true);

                return _UnlockAndUpdateBitmapData(res);
            }

            public static Bitmap Transparent(Bitmap target, Color level)
            {
                BitmapData res = _LockAndGetBitmapData(new Bitmap(target, target.Width, target.Height));
                int step = _GetPixelWeight(res);
                byte[] complevel = { level.B, level.G, level.R, level.A };
                double temp;

                for (int loop1 = 0; loop1 < res.argb.Length; loop1 += step)
                {
                    if (res.argb[loop1] == 0) continue;
                    temp = IsLower(res, loop1, complevel);
                    if (temp > 0)
                        res.argb[loop1] = 0;//(byte)((temp == 0) ? 255 : 255 - 255 * Math.Abs(temp));
                }

                return _UnlockAndUpdateBitmapData(res);
            }
            public static Color GetMixColor(Color f, Color s, double proportion)
            {
                if (proportion > 1) proportion = 1d;
                if (proportion < 0) proportion = 0d;
                return Color.FromArgb((int)(proportion * s.A + (1d - proportion) * f.A), (int)(proportion * s.R + (1d - proportion) * f.R),
                    (int)(proportion * s.G + (1d - proportion) * f.G), (int)(proportion * s.B + (1d - proportion) * f.B));
            }
            public static Color GetMixColor(Color f, Color s) { return Color.FromArgb((f.A + s.A) / 2, (f.R + s.R) / 2, (f.G + s.G) / 2, (f.B + s.B) / 2); }
            public static Color GetOctetMixColor(BitmapData data, int x, int y, bool initcenterpixel)
            {
                int step = _GetPixelWeight(data), prom = 0, del = 0, work, main = 0;
                int A = 0, R = 0, G = 0, B = 0;

                for (int loop1 = -1; loop1 < 2; loop1++)
                {
                    prom = (data.data.Width * (y + loop1) + x) * step;
                    if (loop1 == 0) main = prom;

                    for (int loop2 = -1; loop2 < 2; loop2++)
                    {
                        work = prom + (step * loop2);
                        if (work < 0 || work >= data.argb.Length) continue;
                        A += data.argb[work + 3]; R += data.argb[work + 2]; G += data.argb[work + 1]; B += data.argb[work];
                        del++;
                    }
                }

                A /= del; R /= del; G /= del; B /= del;

                if (initcenterpixel)
                {
                    data.argb[main + 3] = (byte)A; data.argb[main + 2] = (byte)R; data.argb[main + 1] = (byte)G; data.argb[main] = (byte)B;
                    return Color.Empty;
                }
                else return Color.FromArgb(A, R, G, B);
            }
            public static Color GetOctetMixColor(BitmapData data, int pos, bool initcenterpixel)
            {
                int step = _GetPixelWeight(data), prom = 0, del = 0, work;
                int A = 0, R = 0, G = 0, B = 0;

                for (int loop1 = -1; loop1 < 2; loop1++)
                {
                    prom = pos + (data.data.Stride * loop1);

                    for (int loop2 = -1; loop2 < 2; loop2++)
                    {
                        work = prom + (step * loop2);
                        if (work < 0 || work >= data.argb.Length) continue;
                        A += data.argb[work + 3]; R += data.argb[work + 2]; G += data.argb[work + 1]; B += data.argb[work];
                        del++;
                    }
                }

                A /= del; R /= del; G /= del; B /= del;

                if (initcenterpixel)
                {
                    data.argb[pos + 3] = (byte)A; data.argb[pos + 2] = (byte)R; data.argb[pos + 1] = (byte)G; data.argb[pos] = (byte)B;
                    return Color.Empty;
                }
                else return Color.FromArgb(A, R, G, B);
            }

            public static Color InvertColour(Color color) { return Color.FromArgb((byte)~color.R, (byte)~color.G, (byte)~color.B); }
            public static Color DarkerColor(Color color, int percentage)
            {
                if (percentage <= 0) return color;
                if (percentage > 100) percentage = 100;
                return Color.FromArgb(color.A, color.R - color.R * percentage / 100, color.G - color.G * percentage / 100, color.B - color.B * percentage / 100);
            }
            public static Color LighterColor(Color color, int percentage)
            {
                if (percentage <= 0) return color;
                if (percentage > 100) percentage = 100;
                return Color.FromArgb(color.A, color.R + (255 - color.R) * percentage / 100, color.G + (255 - color.G) * percentage / 100, color.B + (255 - color.B) * percentage / 100);
            }
            public static Color ToGrayTone(Color color)
            {
                int val = (color.R + color.G + color.B) / 3;
                return Color.FromArgb(val, val, val);
            }

            static double Del(double f, double s) { if (s == 0) return 1; if (f == 0) return 0; return f / s; }
            static double IsLower(BitmapData data, int fpos, byte[] spos)
            {
                double fs = (Del(255d, data.argb[fpos + 3]) + Del(255d, data.argb[fpos + 2]) + Del(255d, data.argb[fpos + 1]) + Del(255d, data.argb[fpos])) / 4;
                double ss = (Del(255d, spos[3]) + Del(255d, spos[2]) + Del(255d, spos[1]) + Del(255d, spos[0])) / 4;
                return fs / ss * (((data.argb[fpos + 3] << 24 | data.argb[fpos + 2] << 16 | data.argb[fpos + 1] << 8 | data.argb[fpos])
                    < (spos[3] << 24 | spos[2] << 16 | spos[1] << 8 | spos[0])) ? 1 : -1);
            }
            public static double IsLower(Color f, Color s)
            {
                double fs = (255 / f.A + 255 / f.R + 255 / f.G + 255 / f.B) / 4;
                double ss = (255 / f.A + 255 / f.R + 255 / f.G + 255 / f.B) / 4;
                return fs / ss * (f.ToArgb() < s.ToArgb() ? 1 : -1);
            }
            public static double IsHigher(Color f, Color s)
            {
                return -IsLower(f, s);
            }

            static int GetTetraNum(BitmapData data, int x, int y)
            {
                int step = _GetPixelWeight(data), prom = 0, del = 0, pos = (data.data.Width * y + x) * step;
                int A = 0, R = 0, G = 0, B = 0;

                prom = (data.data.Width * (y - 1) + x) * step; if (prom >= 0) { A += Math.Abs(data.argb[pos + 3] - data.argb[prom + 3]); R += Math.Abs(data.argb[pos + 2] - data.argb[prom + 2]); G += Math.Abs(data.argb[pos + 1] - data.argb[prom + 1]); B += Math.Abs(data.argb[pos] - data.argb[prom]); del++; }
                prom = (data.data.Width * (y + 1) + x) * step; if (prom < data.argb.Length) { A += Math.Abs(data.argb[pos + 3] - data.argb[prom + 3]); R += Math.Abs(data.argb[pos + 2] - data.argb[prom + 2]); G += Math.Abs(data.argb[pos + 1] - data.argb[prom + 1]); B += Math.Abs(data.argb[pos] - data.argb[prom]); del++; }
                prom = pos - step; if (prom >= 0) { A += Math.Abs(data.argb[pos + 3] - data.argb[prom + 3]); R += Math.Abs(data.argb[pos + 2] - data.argb[prom + 2]); G += Math.Abs(data.argb[pos + 1] - data.argb[prom + 1]); B += Math.Abs(data.argb[pos] - data.argb[prom]); del++; }
                prom = pos + step; if (prom < data.argb.Length) { A += Math.Abs(data.argb[pos + 3] - data.argb[prom + 3]); R += Math.Abs(data.argb[pos + 2] - data.argb[prom + 2]); G += Math.Abs(data.argb[pos + 1] - data.argb[prom + 1]); B += Math.Abs(data.argb[pos] - data.argb[prom]); del++; }

                return (A + R + G + B) / del;
            }

            /// <summary>Наложение 2х изображений друг на друга.</summary>
            /// <param name="x">1е изображение.</param>
            /// <param name="y">2е изображение.</param>
            /*unsafe Bitmap AlphaBlendingUnsafe(Bitmap x, Bitmap y, byte s)
            {
                if (x == null || y == null)
                    throw new NullReferenceException();

                if (x.PixelFormat != PixelFormat.Format24bppRgb ||
                    y.PixelFormat != PixelFormat.Format24bppRgb)

                    throw new ArgumentException();

                var rect = new Rectangle(0, 0, Math.Min(x.Width, y.Width), Math.Min(x.Height, y.Height));

                Bitmap bmp = new Bitmap(
                    rect.Width,
                    rect.Height,
                    PixelFormat.Format24bppRgb
                    );

                var bd = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                var bd0 = x.LockBits(rect, ImageLockMode.ReadOnly, x.PixelFormat);
                var bd1 = y.LockBits(rect, ImageLockMode.ReadOnly, y.PixelFormat);

                byte* pBmp = (byte*)bd.Scan0;
                byte* pX = (byte*)bd0.Scan0;
                byte* pY = (byte*)bd1.Scan0;

                byte* pEnd = pBmp + bd.Stride * bd.Height;

                while (pBmp != pEnd)
                {
                    *pBmp = (byte)(*pX * (255 - s) / 255 + *pY * s / 255);
                    *(pBmp + 1) = (byte)(*(pX + 1) * (255 - s) / 255 + *(pY + 1) * s / 255);
                    *(pBmp + 2) = (byte)(*(pX + 2) * (255 - s) / 255 + *(pY + 2) * s / 255);

                    pBmp += 3;
                    pX += 3;
                    pY += 3;
                }

                bmp.UnlockBits(bd);
                x.UnlockBits(bd0);
                y.UnlockBits(bd1);
                return bmp;
            }*/

            /// <summary>Наложение 2х изображений друг на друга.</summary>
            /// <param name="x">1е изображение.</param>
            /// <param name="y">2е изображение.</param>
            public static Bitmap Blending(Bitmap x, Bitmap y, byte s)
            {
                if (x == null || y == null)
                    throw new NullReferenceException();

                Bitmap bmp = new Bitmap(
                    Math.Min(x.Width, y.Width),
                    Math.Min(x.Height, y.Height),
                    PixelFormat.Format24bppRgb
                    );

                Color clr0, clr1;

                for (int _x = 0; _x < bmp.Width; _x++)
                    for (int _y = 0; _y < bmp.Height; _y++)
                    {
                        clr0 = x.GetPixel(_x, _y);
                        clr1 = y.GetPixel(_x, _y);
                        bmp.SetPixel(_x, _y,
                            Color.FromArgb(
                                Math.Min(255, clr0.R * (255 - s) / 255 + clr1.R * s / 255),
                                Math.Min(255, clr0.G * (255 - s) / 255 + clr1.G * s / 255),
                                Math.Min(255, clr0.B * (255 - s) / 255 + clr1.B * s / 255)
                            )
                        );
                    }
                return bmp;
            }

            /// <summary>
            /// Наложение 2х изображений друг на друга.
            /// </summary>
            /// <param name="x">1е изображение.</param>
            /// <param name="y">2е изображение.</param>
            /// <param name="percent">Коэффициент прозрачности (от 0 до 1).</param>
            /// <returns>Результат наложения двух изображений.</returns>
            public static Bitmap AlphaBlending(Image x, Image y, float percent)
            {
                if (percent < 0f || percent > 1f)
                    throw new ArgumentOutOfRangeException();

                if (x == null || y == null)
                    throw new NullReferenceException();

                Bitmap bmp = new Bitmap(
                    Math.Max(x.Width, y.Width),
                    Math.Max(x.Height, y.Height)
                    );

                var cm = new ColorMatrix(
                    new float[][] {
            new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f },
            new float[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f },
            new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f },
            new float[] { 0.0f, 0.0f, 0.0f, percent, 0.0f },
            new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }
        }
                );

                using (var imgAttr = new ImageAttributes())
                {
                    imgAttr.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(x, 0, 0, x.Width, x.Height);
                        g.DrawImage(
                            y,
                            new Rectangle(0, 0, y.Width, y.Height),
                            0, 0, y.Width, y.Height,
                            GraphicsUnit.Pixel,
                            imgAttr
                            );
                    }
                }

                return bmp;
            }
        }

        public struct ColorRGB
        {
            public byte R, G, B;

            public ColorRGB(Color value)
            {
                this.R = value.R;
                this.G = value.G;
                this.B = value.B;
            }

            public static implicit operator Color(ColorRGB rgb)
            {
                return Color.FromArgb(rgb.R, rgb.G, rgb.B);
            }

            public static explicit operator ColorRGB(Color c)
            {
                return new ColorRGB(c);
            }

            // Given H,S,L in range of 0-1
            // Returns a Color (RGB struct) in range of 0-255
            public static ColorRGB HSL2RGB(double h, double sl, double l)
            {
                double v;
                double r, g, b;

                r = l;   // default to gray
                g = l;
                b = l;

                v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);

                if (v > 0)
                {
                    double m;
                    double sv;
                    int sextant;
                    double fract, vsf, mid1, mid2;

                    m = l + l - v;
                    sv = (v - m) / v;
                    h *= 6.0;
                    sextant = (int)h;
                    fract = h - sextant;
                    vsf = v * sv * fract;
                    mid1 = m + vsf;
                    mid2 = v - vsf;

                    switch (sextant)
                    {
                        case 0:
                            r = v;
                            g = mid1;
                            b = m;
                            break;
                        case 1:
                            r = mid2;
                            g = v;
                            b = m;
                            break;

                        case 2:
                            r = m;
                            g = v;
                            b = mid1;
                            break;

                        case 3:
                            r = m;
                            g = mid2;
                            b = v;
                            break;

                        case 4:
                            r = mid1;
                            g = m;
                            b = v;
                            break;

                        case 5:
                            r = v;
                            g = m;
                            b = mid2;
                            break;
                    }
                }

                ColorRGB rgb;

                rgb.R = Convert.ToByte(r * 255.0f);
                rgb.G = Convert.ToByte(g * 255.0f);
                rgb.B = Convert.ToByte(b * 255.0f);
                return rgb;
            }

            // Given a Color (RGB Struct) in range of 0-255
            // Return H,S,L in range of 0-1
            public static void RGB2HSL(ColorRGB rgb, out double h, out double s, out double l)
            {
                double r = rgb.R / 255.0;
                double g = rgb.G / 255.0;
                double b = rgb.B / 255.0;
                double v;
                double m;
                double vm;
                double r2, g2, b2;

                h = 0; // default to black
                s = 0;
                l = 0;

                v = Math.Max(r, g);
                v = Math.Max(v, b);

                m = Math.Min(r, g);
                m = Math.Min(m, b);

                l = (m + v) / 2.0;

                if (l <= 0.0)
                    return;

                vm = v - m;
                s = vm;

                if (s > 0.0)
                    s /= (l <= 0.5) ? (v + m) : (2.0 - v - m);
                else
                    return;

                r2 = (v - r) / vm;
                g2 = (v - g) / vm;
                b2 = (v - b) / vm;

                if (r == v)
                    h = (g == m ? 5.0 + b2 : 1.0 - g2);
                else if (g == v)
                    h = (b == m ? 1.0 + r2 : 3.0 - b2);
                else
                    h = (r == m ? 3.0 + g2 : 5.0 - r2);

                h /= 6.0;
            }

            public static Color[] GetPalitraPieces(int count_of_colors)
            {
                Color[] colors = new Color[count_of_colors];
                double step = 1d / (double)count_of_colors;
                int c = 0;

                for (double i = 0; i < 1; i += step, c = c + 1 == colors.Length ? c : c + 1)
                    colors[c] = HSL2RGB(i, 0.5, 0.5);

                return colors;
            }

            public static Color[] GetPalitra(double step)
            {
                Color[] colors = new Color[(int)Math.Round((1d / step), MidpointRounding.AwayFromZero)];
                int c = 0;

                for (double i = 0; i < 1; i += step, c = c + 1 == colors.Length ? c : c + 1)
                    colors[c] = HSL2RGB(i, 0.5, 0.5);

                return colors;
            }
        }

        /*void DrawLinedFon(Graphics gr,int linescount,Color linecolor,bool drawborder)
        {
            float w = (float)gr.VisibleClipBounds.Width / (float)linescount;
            float sp = w / 2f;

            float he = gr.VisibleClipBounds.Height-1;
            int precount = (int)(he/(sp))+1;
            float curr=-sp*precount;

            GraphicsPath path = new GraphicsPath();

            for (int loop1 = -precount; loop1 < linescount; loop1++)
            {
                path.AddLines(new[] { new PointF(curr, he), new PointF(curr + he, 0), new PointF(curr + he + sp, 0), new PointF(curr+sp, he) });
                path.CloseFigure();
                curr += sp*2;
            }

            gr.FillPath(new SolidBrush(linecolor), path);
            gr.DrawPath(Pens.Black,path);
            path.Dispose();
        }*/

        void DrawMonoColorLinedFon(Graphics gr, Color linecolor, bool drawborder)
        {
            float w = 10;
            float sp = w / 2f;
            int linescount = (int)(gr.VisibleClipBounds.Width / w) + 1;

            float he = gr.VisibleClipBounds.Height - 1;
            int precount = (int)(he / (sp)) + 1;
            float curr = -sp * precount;

            GraphicsPath path = new GraphicsPath();

            for (int loop1 = -precount; loop1 < linescount; loop1++)
            {
                path.AddLines(new[] { new PointF(curr, he), new PointF(curr + he, 0), new PointF(curr + he + sp, 0), new PointF(curr + sp, he) });
                path.CloseFigure();
                curr += sp * 2;
            }

            gr.FillPath(new SolidBrush(linecolor), path);
            gr.DrawPath(Pens.Black, path);
            path.Dispose();
        }

        void DrawPolyColorLinedFon(Graphics gr)
        {
            float w = 10;
            float sp = w / 2f;
            int linescount = (int)(gr.VisibleClipBounds.Width / w) + 1;

            float he = gr.VisibleClipBounds.Height - 1;
            int precount = (int)(he / (sp)) + 1;
            float curr = -sp * precount;

            Color[] colors = ColorRGB.GetPalitraPieces(linescount + precount);

            GraphicsPath path;
            SolidBrush brush;

            for (int loop1 = -precount, color = 0; loop1 < linescount; loop1++, color++)
            {
                path = new GraphicsPath();
                path.AddLines(new[] { new PointF(curr, he), new PointF(curr + he, 0), new PointF(curr + he + sp, 0), new PointF(curr + sp, he) });
                path.CloseFigure();

                brush = new SolidBrush(colors[color]);
                gr.FillPath(brush, path);
                gr.DrawPath(Pens.Black, path);

                brush.Dispose();
                path.Dispose();
                curr += sp * 2;
            }
        }

        Bitmap GetFonBitmap()
        {
            Bitmap bfb = new Bitmap(b.Width, b.Height);
            Graphics bfg = Graphics.FromImage(bfb);
            DrawMonoColorLinedFon(bfg, Color.Black, false);
            bfg.Dispose();
            return bfb;
        }

        public enum _InfoPos : int
        {
            BY_PROGRESS = 0,
            BY_CENTER = 1,
            BY_BEGIN = 2,
        }

        public enum _OrientationType : int
        {
            VERTICAL = 0,
            GORIZONTAL = 1,
        }

        public enum _InfoType : int
        {
            NONE = -1,
            PROGRESS = 0,
            TEXT = 1,
            TEXTANDPROGRESS = 2,
        }

        public enum _ProgressDrawType : int
        {
            _SIMPLE = 0,
            _3D = 1,
        }

        Control container = null;
        ToolStripItem itemcontainer = null;

        float val, percentage;
        Bitmap b, fillb = null;
        Graphics g, cg = null;
        Font f;
        bool visible = true,pfon=false;

        /// <summary>При отключении контейнер будет отрисовыватся в первозданом виде</summary>
        public bool Visible { get { return visible; } set { visible = value; Paint(); } }

        /// <summary>Пространственная ориентация</summary>
        public _OrientationType Orientation { get; set; }

        /// <summary>Шрифт отображаемой инфы</summary>
        public Font Font { get { return f; } set { f = value; } }

        /// <summary>Модель смешивания цветов заливки шкалы</summary>
        public LinearGradientMode GradientMode { get; set; }

        /// <summary>Показывать текстовую инфу на шкале</summary>
        public _InfoType InfoType { get; set; }
        /// <summary>Текстовая инфа</summary>
        public string TextInfo { get; set; }
        /// <summary>Позиция текстовой инфы</summary>
        public _InfoPos InfoPosition { get; set; }

        public _ProgressDrawType ProgressDrawType { get; set; }

        /// <summary>Картинка используемая для альтернативного представления шкалы прогресса. При значении null используется стандартная заливка шкалы</summary>
        public Bitmap FillBitmap
        {
            get { return fillb; }
            set
            {
                fillb = new Bitmap(b.Width, b.Height);
                Graphics fillg = Graphics.FromImage(fillb);
                fillg.DrawImage(value, new Rectangle(0, 0, fillb.Width, fillb.Height));
                Paint();
            }
        }

        /// <summary>Цвет текста инфы</summary>
        public Color TextColor { get; set; }
        /// <summary>Цвет фона</summary>
        public Color BackGround { get; set; }
        /// <summary>Первая компонента цвета шкалы</summary>
        public Color GradienBegin { get; set; }
        /// <summary>Вторая компонента цвета шкалы</summary>
        public Color GradienEnd { get; set; }
        /// <summary>Цвет границы</summary>
        public Color BorderColor { get; set; }

        /// <summary>Минимальное значение</summary>
        public float Min { get; set; }
        /// <summary>Максимальное значение</summary>
        public float Max { get; set; }
        /// <summary>Текущее значение</summary>
        public float Value
        {
            get { return val; }
            set
            {
                val = (value > Max) ? Max : (value < Min) ? Min : value;
                Paint();
            }
        }

        /// <summary>Битмап с текущим отображением шкалы</summary>
        public Bitmap ProgressBit { get { return b; } }

        /// <summary>Программная инвертация цвета текста относительно фона</summary>
        public bool PMTextFlag { get; set; }

        /// <summary>Программный фон</summary>
        public bool BackgroundFlag { get { return pfon; } set { pfon = value; if (!pfon) { if (FillBitmap != null) FillBitmap.Dispose(); } else { Bitmap bfb = new Bitmap(b.Width, b.Height); Graphics bfg = Graphics.FromImage(bfb); /*DrawMonoColorLinedFon(bfg, Color.Black, false);*/DrawPolyColorLinedFon(bfg); bfg.Dispose(); FillBitmap = bfb; } } } 

        int left = 0, top = 0;

        void Preinit(int fontheight)
        {
            PMTextFlag = false;

            GradienBegin = Color.DarkGray;//Color.Coral;
            GradienEnd = Color.LightSlateGray;//Color.LightGray;

            BorderColor = Color.DarkBlue;
            BackGround = Color.WhiteSmoke;

            TextColor = Color.Black;

            InfoPosition = _InfoPos.BY_CENTER;

            f = new Font(FontFamily.GenericSerif, fontheight, FontStyle.Bold);

            GradientMode = LinearGradientMode.Vertical;
            Orientation = _OrientationType.GORIZONTAL;
            InfoType = _InfoType.TEXT;
            TextInfo = "";

            ProgressDrawType = _ProgressDrawType._SIMPLE;
            BackgroundFlag = true;
        }

        void container_Resize(object sender, EventArgs e)
        {
            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);
            cg = Graphics.FromHwnd(container.Handle);

            if (FillBitmap != null)
                FillBitmap = GetFonBitmap();
        }
        void container_Paint(object sender, PaintEventArgs e)
        {
            if (visible) Paint();
        }

        public Progress(Control container, float minValue = 0, float maxValue = 100)
        {
            this.container = container;
            container.Resize += new EventHandler(container_Resize);
            container.Paint += new PaintEventHandler(container_Paint);

            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);
            cg = Graphics.FromHwnd(container.Handle);

            Preinit(container.Height > 12 ? 10 : container.Height - 2);
            Min = minValue;
            Max = maxValue;
        }
        public Progress(ToolStripItem container, float minValue = 0, float maxValue = 100)
        {
            itemcontainer = container;
            itemcontainer.Image = null;
            itemcontainer.BackgroundImageLayout = ImageLayout.None;

            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);

            Preinit(container.Height > 12 ? 10 : container.Height - 2);
            Min = minValue;
            Max = maxValue;
        }
        public Progress(int left, int top, Control container, float minValue = 0, float maxValue = 100)
        {
            this.container = container;
            container.Resize += new EventHandler(container_Resize);

            this.left = left;
            this.top = top;

            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);
            cg = Graphics.FromHwnd(container.Handle);

            Preinit(container.Height > 12 ? 10 : container.Height - 2);
            Min = minValue;
            Max = maxValue;
        }
        public Progress(int width, int height, float minValue = 0, float maxValue = 100)
        {
            b = new Bitmap(width, height);
            g = Graphics.FromImage(b);

            Preinit(height > 12 ? 10 : height - 2);
            Min = minValue;
            Max = maxValue;
        }

        ~Progress()
        {
            f.Dispose();
            g.Dispose();
            b.Dispose();

            if (cg != null)
                cg.Dispose();

            if (FillBitmap != null)
                FillBitmap.Dispose();
        }

        /// <summary>Получение текущей границы заполнения шкалы</summary>
        float GetFill() { return (percentage = Value / (Max - Min)) * (float)(Orientation == _OrientationType.GORIZONTAL ? b.Width : b.Height); }

        /// <summary>Отрисовка текстовой инфы</summary>
        void Info(float x)
        {
            StringFormat format = new StringFormat();
            //if(Orientation==OrientationType.VERTICAL)   format.FormatFlags = StringFormatFlags.DirectionVertical;

            SolidBrush textbr = new SolidBrush(TextColor);
            string s = (InfoType == _InfoType.PROGRESS) ? Math.Round(percentage * 100, 2) + " %" : (InfoType == _InfoType.TEXT) ? TextInfo : "( " + Math.Round(percentage * 100, 2) + " % ) " + TextInfo;
            Rectangle lrect = Rectangle.Empty;

            switch (InfoPosition)
            {
                case _InfoPos.BY_BEGIN:
                    if (Orientation == _OrientationType.GORIZONTAL)
                        lrect = new Rectangle(2, b.Height / 2 - f.Height / 2, b.Width, f.Height);
                    else
                        lrect = new Rectangle(b.Width / 2 - (int)(f.Height * .75f), 2, f.Height * 2, b.Height);
                    break;

                case _InfoPos.BY_CENTER:
                    if (Orientation == _OrientationType.GORIZONTAL)
                        lrect = new Rectangle((int)(b.Width / 2 - g.MeasureString(s, f).Width / 2), b.Height / 2 - f.Height / 2, b.Width / 2, b.Height);
                    else
                        lrect = new Rectangle(b.Width / 2 - (int)(f.Height * .75f), b.Height / 2 - f.Height * s.Length / 2, f.Height * 2, b.Height);
                    break;

                case _InfoPos.BY_PROGRESS:
                    if (Orientation == _OrientationType.GORIZONTAL)
                        lrect = new Rectangle((int)(x - g.MeasureString(s, f).Width), b.Height / 2 - f.Height / 2, b.Width, b.Height);
                    else
                        lrect = new Rectangle(b.Width / 2 - (int)(f.Height * .75f), (int)(x - f.Height * s.Length / 2), f.Height * 2, b.Height);
                    break;
            }

            if (!PMTextFlag)
                g.DrawString(s, f, textbr, lrect, format);
            else
            {
                Bitmap temp = new Bitmap(lrect.Width,lrect.Height);
                Graphics tempg = Graphics.FromImage(temp);
                Font nf = new System.Drawing.Font(f.FontFamily,f.Size+3);

                tempg.Clear(Color.FromArgb(0,Color.White));
                tempg.DrawString(s, nf, textbr, 0,0, format);

                ImageEffect.IncludePict(b,temp,new Point(lrect.X,lrect.Y));

                temp.Dispose();
                tempg.Dispose();
            }
            textbr.Dispose();
        }

        /// <summary>Отрисовка шкалы</summary>
        public void Paint()
        {
            if (Visible)
            {
                Pen pen = new Pen(BorderColor);

                g.Clear(BackGround);

                g.DrawRectangle(pen, 0, 0, b.Width - 1, b.Height - 1);

                float x = GetFill();

                if (FillBitmap == null)
                {
                    LinearGradientBrush br = new LinearGradientBrush(new Rectangle(0, 0, b.Width, b.Height), GradienBegin, GradienEnd, GradientMode);
                    if (Orientation == _OrientationType.GORIZONTAL)
                    {
                        g.FillRectangle(br, 0, 0, x, b.Height);

                        if (ProgressDrawType == _ProgressDrawType._3D)
                        {
                            br.Dispose();
                            br = new LinearGradientBrush(new Rectangle((int)x, 0, (int)20, b.Height), GradienBegin, Color.Black, LinearGradientMode.ForwardDiagonal);
                            g.FillRectangle(br, x, 0, 6, b.Height);
                            g.DrawRectangle(pen, x, 0, 6, b.Height);
                        }
                    }
                    else
                    {
                        g.FillRectangle(br, 0, 0, b.Width, x);

                        if (ProgressDrawType == _ProgressDrawType._3D)
                        {
                            br.Dispose();
                            br = new LinearGradientBrush(new Rectangle((int)x, 0, (int)20, b.Height), GradienBegin, Color.Black, LinearGradientMode.ForwardDiagonal);
                            g.FillRectangle(br, 0, x, b.Width, 6);
                            g.DrawRectangle(pen, 0, x, b.Width, 6);
                        }
                    }
                    br.Dispose();
                }
                else
                {
                    TextureBrush texbr = new TextureBrush(FillBitmap);
                    if (Orientation == _OrientationType.GORIZONTAL)
                        g.FillRectangle(texbr, 0, 0, x, b.Height);
                    else
                        g.FillRectangle(texbr, 0, 0, b.Width, x);
                    texbr.Dispose();
                }

                if (InfoType != _InfoType.NONE) Info(x);

                if (cg != null)
                    cg.DrawImage(b, left, top);

                if (itemcontainer != null)
                {
                    itemcontainer.BackgroundImage = b;
                    itemcontainer.Invalidate();
                }

                pen.Dispose();
            }
            else
                if (container != null)
                    container.Invalidate();
        }
    }

    public class Progress2
    {
        const int level_count = 15;
        Control container = null;
        float [] infoval = new float[level_count];
        float[] levels;

        float val, info_val, info_level, p_val = 0;
        int p_info_val = 0, counter = 0;
        Bitmap b = null;
        Graphics g, cg = null;
        bool visible = true;
        Brush[] brushes = { Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Magenta };

        /// <summary>При отключении контейнер будет отрисовыватся в первозданом виде</summary>
        public bool Visible { get { return visible; } set { visible = value; Paint(); } }

        ///// <summary>Цвет фона</summary>
        //public Color BackGround { get; set; }
        /// <summary>Цвет границы</summary>
        public Color BorderColor { get; set; }

        /// <summary>Вертикальный минимум значения</summary>
        int InfoMin { get; set; }
        /// <summary>Вертикальный максимум значения</summary>
        int InfoMax { get; set; }
        /// <summary>Минимальное значение</summary>
        public float Min { get; set; }
        /// <summary>Максимальное значение</summary>
        public float Max { get; set; }
        /// <summary>Текущее значение</summary>
        public float Value
        {
            get { return val; }
            set
            {
                val = (value > Max) ? Max : (value < Min) ? Min : value;
                info_val = InfoMin;
                Paint();
            }
        }
        public void SetValue(float value, int info_value)
        {
            val = (value > Max) ? Max : (value < Min) ? Min : value;
            info_val = info_value;
            Paint();
        }

        /// <summary>Битмап с текущим отображением шкалы</summary>
        public Bitmap ProgressBit { get { return b; } }

        public void Clear() { Preinit(); }

        int left = 0, top = 0;

        void Preinit()
        {
            g.Clear(Color.Gray);
            val = Min;
            info_val = InfoMin;
            p_val = p_info_val = counter = 0;
            //BorderColor = Color.DarkBlue;
            //BackGround = Color.WhiteSmoke;
            info_level = (float)(b.Height - 4) / (float)level_count;
            levels = new float[level_count + 1];
            for (int loop1 = 0; loop1 <= level_count; loop1++)
                levels[loop1] = loop1 * info_level + 2;
            info_level = (InfoMax - InfoMin) / (float)level_count;
        }

        void container_Resize(object sender, EventArgs e)
        {
            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);
            cg = Graphics.FromHwnd(container.Handle);
        }
        void container_Paint(object sender, PaintEventArgs e)
        {
            if (visible) Paint();
        }

        public Progress2(Control container, float minValue = 0, float maxValue = 100, int minInfo = 0, int maxInfo = 100)
        {
            this.container = container;
            container.Resize += new EventHandler(container_Resize);
            container.Paint += new PaintEventHandler(container_Paint);

            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);
            cg = Graphics.FromHwnd(container.Handle);

            Min = minValue;
            Max = maxValue;
            InfoMin = minInfo;
            InfoMax = maxInfo;
            Preinit();
        }
        public Progress2(int left, int top, Control container, float minValue = 0, float maxValue = 100, int minInfo = 0, int maxInfo = 100)
        {
            this.container = container;
            container.Resize += new EventHandler(container_Resize);

            this.left = left;
            this.top = top;

            b = new Bitmap(container.Width, container.Height);
            g = Graphics.FromImage(b);
            cg = Graphics.FromHwnd(container.Handle);

            Min = minValue;
            Max = maxValue;
            InfoMin = minInfo;
            InfoMax = maxInfo;
            Preinit();
        }
        public Progress2(int width, int height, float minValue = 0, float maxValue = 100, int minInfo = 0, int maxInfo = 100)
        {
            b = new Bitmap(width, height);
            g = Graphics.FromImage(b);

            Min = minValue;
            Max = maxValue;
            InfoMin = minInfo;
            InfoMax = maxInfo;
            Preinit();
        }

        ~Progress2()
        {
            g.Dispose();
            b.Dispose();

            if (cg != null)
                cg.Dispose();
        }

        /// <summary>Получение текущей границы заполнения шкалы</summary>
        float GetFill() { return (val / (Max - Min)) * (float)b.Width; }

        /// <summary>Отрисовка шкалы</summary>
        public void Paint()
        {
            if (Visible)
            {
                Pen pen = new Pen(BorderColor);
                g.DrawRectangle(pen, 0, 0, b.Width - 1, b.Height - 1);

                float x = GetFill();
                int lcount = (int)(info_val / info_level);
                if (lcount > level_count) lcount = level_count;
                //g.FillRectangle(brushes[counter % brushes.Length], p_val, 0, x - p_val, levels[lcount]);
                g.DrawLine(Pens.LightGray, x, 0, x, b.Height - 3 - levels[lcount]);
                g.FillRectangle(brushes[counter % brushes.Length], p_val, b.Height - 3 - levels[lcount], x - p_val, b.Height - 3);
                //g.DrawLine(Pens.Black, p_val, levels[p_info_val], x, levels[lcount]);
                p_val = x;
                if (p_info_val != lcount) counter++;
                p_info_val = lcount;

                if (cg != null)
                    cg.DrawImage(b, left, top);

                pen.Dispose();
            }
            else
                if (container != null)
                    container.Invalidate();
        }
    }
}