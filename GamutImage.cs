using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace monitorinfo;

public static class GamutImage
{
    public const int ImageSize = 600;
    private const int RGB_DATA_ARR_LEN = 800;

    private static int AxeUnit = 10;
    private static int BorderBottom = 30;
    private static int BorderLeft = 30;
    private static readonly int ActualWidth = ImageSize - BorderLeft;
    private static readonly int ActualHeight = ImageSize - BorderBottom;
    private static int ActualSize = Math.Min(ActualWidth, ActualHeight);
    private static double ActualStep = (double) ActualSize / RGB_DATA_ARR_LEN;

    private static Font Font = new("Serif", 9f);
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
        Graphics.FillRectangle(solidBrush, new Rectangle(0, ActualHeight - ActualSize, ActualSize + BorderLeft, ActualSize + BorderBottom));
        Graphics.ResetClip( );
        Pen = new Pen(Color.Gray, 1f) { EndCap = LineCap.Flat };
        for (int i = 0; i <= AxeUnit; i++)
        {
            Pen.DashStyle = i == 0 ? DashStyle.Solid : DashStyle.Dash;
            string text = (i / (double) AxeUnit).ToString("f1");
            int AxeStep = ActualSize * i / AxeUnit;
            SizeF sizeF = Graphics.MeasureString(text, Font);

            Graphics.DrawLine(Pen, BorderLeft + AxeStep, ActualHeight, BorderLeft + AxeStep, ActualHeight - ActualSize);
            PointF pointX = new(BorderLeft + AxeStep - sizeF.Width / 2f, ActualHeight);
            Graphics.DrawString(text, Font, Brushes.Gray, pointX);

            Graphics.DrawLine(Pen, BorderLeft, ActualHeight - AxeStep, BorderLeft + ActualSize, ActualHeight - AxeStep);
            PointF pointY = new(BorderLeft - sizeF.Width, ActualHeight - AxeStep - sizeF.Height / 2f);
            Graphics.DrawString(text, Font, Brushes.Gray, pointY);
        }
    }

    private static void DrawChroma( )
    {
        Point[] chromaticity = new Point[81];
        for (int i = 0; i < 81; i++)
        {
            chromaticity[i] = new Point(
                BorderLeft + (int) (ColorSpace.SpectralChromaticity[i, 0] * RGB_DATA_ARR_LEN * ActualStep),
                ActualSize - (int) (ColorSpace.SpectralChromaticity[i, 1] * RGB_DATA_ARR_LEN * ActualStep)
            );
        }
        Pen = new(Color.Black);
        Graphics.DrawPolygon(Pen, chromaticity);
        GraphicsPath.AddPolygon(chromaticity);
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
                int actualX = BorderLeft + (int) (i * ActualStep) - 1;
                int actualY = ImageSize - (int) (j * ActualStep) - 1;
                if (actualX >= RGBImage.Width || actualY >= RGBImage.Height)
                    continue;
                RGBImage.SetPixel(actualX, actualY, Color.FromArgb(R, G, B));
            }
        }
    }

    private static void DrawRegions(Info info)
    {
        Pen.Color = Color.White;
        Pen.Width = 4f;
        Pen.DashStyle = DashStyle.Solid;
        int NTSC_x1 = Convert.ToInt16(BorderLeft + 0.67 * ActualSize);
        int NSTC_y1 = ActualHeight - Convert.ToInt16(0.33 * ActualSize);
        int NTSC_x2 = Convert.ToInt16(BorderLeft + 0.21 * ActualSize);
        int NTSC_y2 = ActualHeight - Convert.ToInt16(0.71 * ActualSize);
        int NTSC_x3 = Convert.ToInt16(BorderLeft + 0.14 * ActualSize);
        int NTSC_y3 = ActualHeight - Convert.ToInt16(0.08 * ActualSize);
        double NTSC_Area = TriangleArea(NTSC_x1, NSTC_y1, NTSC_x2, NTSC_y2, NTSC_x3, NTSC_y3);
        Graphics.DrawLine(Pen, NTSC_x1, NSTC_y1, NTSC_x2, NTSC_y2);
        Graphics.DrawLine(Pen, NTSC_x1, NSTC_y1, NTSC_x3, NTSC_y3);
        Graphics.DrawLine(Pen, NTSC_x2, NTSC_y2, NTSC_x3, NTSC_y3);

        Pen.Color = Color.Black;
        Pen.Width = 2f;
        Pen.DashStyle = DashStyle.Dash;
        int self_x1 = Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesRedX) * ActualSize);
        int self_y1 = ActualHeight - Convert.ToInt16(Convert.ToDouble(info.AxesRedY) * ActualSize);
        int sefl_x2 = Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesBlueX) * ActualSize);
        int self_y2 = ActualHeight - Convert.ToInt16(Convert.ToDouble(info.AxesBlueY) * ActualSize);
        int self_x3 = Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesGreenX) * ActualSize);
        int self_y3 = ActualHeight - Convert.ToInt16(Convert.ToDouble(info.AxesGreenY) * ActualSize);
        double self_Area = TriangleArea(self_x1, self_y1, sefl_x2, self_y2, self_x3, self_y3);
        Graphics.DrawLine(Pen, self_x1, self_y1, sefl_x2, self_y2);
        Graphics.DrawLine(Pen, self_x1, self_y1, self_x3, self_y3);
        Graphics.DrawLine(Pen, sefl_x2, self_y2, self_x3, self_y3);

        Pen.Color = Color.White;
        Pen.Width = 4f;
        Pen.DashStyle = DashStyle.Dot;
        int sRGB_x1 = Convert.ToInt16(BorderLeft + 0.64 * ActualSize);
        int sRGB_y1 = NSTC_y1;
        int sRGB_x2 = Convert.ToInt16(BorderLeft + 0.3 * ActualSize);
        int sRGB_y2 = ActualHeight - Convert.ToInt16(0.6 * ActualSize);
        int sRGB_x3 = Convert.ToInt16(BorderLeft + 0.15 * ActualSize);
        int sRGB_y3 = ActualHeight - Convert.ToInt16(0.06 * ActualSize);
        double sRGB_Area = TriangleArea(sRGB_x1, sRGB_y1, sRGB_x2, sRGB_y2, sRGB_x3, sRGB_y3);
        Graphics.DrawLine(Pen, sRGB_x1, sRGB_y1, sRGB_x2, sRGB_y2);
        Graphics.DrawLine(Pen, sRGB_x1, sRGB_y1, sRGB_x3, sRGB_y3);
        Graphics.DrawLine(Pen, sRGB_x2, sRGB_y2, sRGB_x3, sRGB_y3);

        Graphics.DrawString($"_sRGB: {Math.Round(self_Area / sRGB_Area * 100, 2)}%", new Font("Serif", 11f), Brushes.Black, ActualSize - 150, 10);
        Graphics.DrawString($"_NTSC: {Math.Round(self_Area / NTSC_Area * 100, 2)}%", new Font("Serif", 11f), Brushes.Black, ActualSize - 150, 40);
    }

    private static double TriangleArea(double x1, double y1, double x2, double y2, double x3, double y3)
    {
        return Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0;
    }
}
