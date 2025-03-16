using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace monitorinfo;

public static class GamutImage
{
    public const int ImageSize = 900;
    private const int RGB_DATA_ARR_LEN = 1600;
    private const int AxeUnit = 10;
    private const int Border = 60;
    private const int Size = ImageSize - Border;
    private const double Step = (double) Size / RGB_DATA_ARR_LEN;

    private static Font Font = new("Serif", 12f);
    private static Graphics Graphics;
    private static GraphicsPath GraphicsPath;
    private static Pen Pen;
    private static Region Region;
    private static Bitmap RGBImage = new(ImageSize, ImageSize);

    public static BitmapSource Draw(Info info)
    {
        DrawColorSpace( );

        Graphics = Graphics.FromImage(RGBImage);
        Region = new( );
        Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        GraphicsPath = new( );
        GraphicsPath.Reset( );

        DrawChroma( );
        DrawAxes( );
        DrawRegions(info);

        Graphics.DrawImage(RGBImage, 0, 0);
        Pen.Dispose( );
        Graphics.Dispose( );

        return Bitmap2Image(RGBImage);
    }

    private static BitmapSource Bitmap2Image(Bitmap bitmap)
    {
        IntPtr hbitmap = bitmap.GetHbitmap( );
        return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions( ));
    }

    private static void DrawAxes( )
    {
        Region.MakeEmpty( );
        Region.Union(GraphicsPath);
        Graphics.ExcludeClip(Region);
        SolidBrush solidBrush = new(Color.White);
        Graphics.FillRectangle(solidBrush, new Rectangle(0, Size - Size, Size + Border, Size + Border));
        Graphics.ResetClip( );
        Pen = new Pen(Color.Gray, 1f) { EndCap = LineCap.Flat };
        for (int i = 0; i <= AxeUnit; i++)
        {
            Pen.DashStyle = i == 0 ? DashStyle.Solid : DashStyle.Dash;
            string text = (i / (double) AxeUnit).ToString("f1");
            int AxeStep = Size * i / AxeUnit;
            SizeF sizeF = Graphics.MeasureString(text, Font);

            Graphics.DrawLine(Pen, Border + AxeStep, Size, Border + AxeStep, Size - Size);
            PointF pointX = new(Border + AxeStep - sizeF.Width / 2f, Size);
            Graphics.DrawString(text, Font, Brushes.Gray, pointX);

            Graphics.DrawLine(Pen, Border, Size - AxeStep, Border + Size, Size - AxeStep);
            PointF pointY = new(Border - sizeF.Width, Size - AxeStep - sizeF.Height / 2f);
            Graphics.DrawString(text, Font, Brushes.Gray, pointY);
        }
    }

    private static void DrawChroma( )
    {
        HashSet<Point> chromaticity = [];
        foreach (double k in ColorSpace.SpectralChromaticity.Keys)
        {
            chromaticity.Add(new Point(
                Border + (int) (k * RGB_DATA_ARR_LEN * Step),
                Size - (int) (ColorSpace.SpectralChromaticity[k] * RGB_DATA_ARR_LEN * Step)
            ));
        }
        Pen = new(Color.Black);
        Graphics.DrawPolygon(Pen, chromaticity.ToArray( ));
        GraphicsPath.AddPolygon(chromaticity.ToArray( ));
    }

    private static void DrawColorSpace( )
    {
        double[,] Rf = new double[RGB_DATA_ARR_LEN, RGB_DATA_ARR_LEN];
        double[,] Gf = new double[RGB_DATA_ARR_LEN, RGB_DATA_ARR_LEN];
        double[,] Bf = new double[RGB_DATA_ARR_LEN, RGB_DATA_ARR_LEN];
        for (int i = 0; i < RGB_DATA_ARR_LEN; i++)
        {
            for (int j = 0; j < RGB_DATA_ARR_LEN; j++)
            {
                double x = i / (double) RGB_DATA_ARR_LEN;
                double y = j / (double) RGB_DATA_ARR_LEN;
                double z = 1.0 - x - y;
                ColorSpace.XYZ2RGB(x, y, z, ref Rf[i, j], ref Gf[i, j], ref Bf[i, j]);
                ColorSpace.ConstrainRGB(ref Rf[i, j], ref Gf[i, j], ref Bf[i, j]);
                ColorSpace.NormalizeRGB(ref Rf[i, j], ref Gf[i, j], ref Bf[i, j]);
                int R = Convert.ToInt32(Rf[i, j] * 255.0);
                int G = Convert.ToInt32(Gf[i, j] * 255.0);
                int B = Convert.ToInt32(Bf[i, j] * 255.0);
                if (R < 0 || R >= 256 || G < 0 || G >= 256 || B < 0 || B >= 256)
                    continue;
                int actualX = Border + (int) (i * Step) - 1;
                int actualY = ImageSize - (int) (j * Step) - 1;
                if (actualX >= RGBImage.Width || actualY >= RGBImage.Height)
                    continue;
                RGBImage.SetPixel(actualX, actualY, Color.FromArgb(R, G, B));
            }
        }
    }

    private static void DrawRegions(Info info)
    {
        Pen.DashStyle = DashStyle.Solid;

        double custom_Area = DrawTriangle(Convert.ToDouble(info.RX), Convert.ToDouble(info.RY), Convert.ToDouble(info.BX), Convert.ToDouble(info.BY), Convert.ToDouble(info.GX), Convert.ToDouble(info.GY), Color.White);
        double sRGB_Area = DrawTriangle(0.64, 0.33, 0.30, 0.60, 0.15, 0.06, Color.Black);
        double DCIP3_Area = DrawTriangle(0.68, 0.32, 0.265, 0.69, 0.15, 0.06, Color.OrangeRed);
        double NTSC_Area = DrawTriangle(0.67, 0.33, 0.21, 0.71, 0.14, 0.08, Color.MediumBlue);
        //double BT2020_Area = DrawTriangle(0.708, 0.292, 0.170, 0.797, 0.131, 0.046, Color.Green);

        Graphics.DrawString($"sRGB(黑): {Math.Round(custom_Area / sRGB_Area * 100, 2)}%", Font, Brushes.Black, Size - 175, 10);
        Graphics.DrawString($"DCI-P3(红): {Math.Round(custom_Area / DCIP3_Area * 100, 2)}%", Font, Brushes.Black, Size - 175, 40);
        Graphics.DrawString($"NTSC(蓝): {Math.Round(custom_Area / NTSC_Area * 100, 2)}%", Font, Brushes.Black, Size - 175, 70);
        //Graphics.DrawString($"BT.2020(绿): {Math.Round(custom_Area / BT2020_Area * 100, 2)}%", Font, Brushes.Black, Size - 175, 100);
    }

    private static double DrawTriangle(double x1, double y1, double x2, double y2, double x3, double y3, Color color)
    {
        Point p1 = new(Convert.ToInt16(Border + x1 * Size), Size - Convert.ToInt16(y1 * Size));
        Point p2 = new(Convert.ToInt16(Border + x2 * Size), Size - Convert.ToInt16(y2 * Size));
        Point p3 = new(Convert.ToInt16(Border + x3 * Size), Size - Convert.ToInt16(y3 * Size));
        double area = TriangleArea(p1, p2, p3);
        Pen.Color = Color.White;
        Pen.Width = 5f;
        Graphics.DrawLine(Pen, p1, p2);
        Graphics.DrawLine(Pen, p1, p3);
        Graphics.DrawLine(Pen, p2, p3);
        Pen.Color = color;
        Pen.Width = 2f;
        Graphics.DrawLine(Pen, p1, p2);
        Graphics.DrawLine(Pen, p1, p3);
        Graphics.DrawLine(Pen, p2, p3);

        return area;
    }

    private static double TriangleArea(Point a, Point b, Point c)
    {
        return Math.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2.0;
    }
}
