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

    public enum RGB_DATA_PIXELS
    {
        NUM300 = 300,
        NUM600 = 600,
        NUM800 = 800
    }

    private static Bitmap RGBImage = new(ImageSize, ImageSize);
    private static int BorderLeft = 30;
    private static int BorderRight = 15;
    private static int BorderTop = 15;
    private static int BorderBottom = 30;
    private static int ActualSize = Math.Min(ImageSize - BorderLeft - BorderRight, ImageSize - BorderTop - BorderBottom);
    private static double ActualStep = (double) ActualSize / RGB_DATA_ARR_LEN;
    private static int AxeUnit = 10;
    private static Pen Pen;
    private static Font Font = new("Serif", 9f);
    private static Graphics Graphics;
    private static GraphicsPath GraphicsPath;
    private static Region Region;

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

    private static void DrawAxes( )
    {
        Region.MakeEmpty( );
        Region.Union(GraphicsPath);
        Graphics.ExcludeClip(Region);
        SolidBrush solidBrush = new(Color.White);
        Graphics.FillRectangle(solidBrush, new Rectangle(0, ImageSize - BorderBottom - BorderTop - ActualSize, ActualSize + BorderLeft + BorderRight, ActualSize + BorderBottom + BorderTop));
        Graphics.ResetClip( );
        Pen = new Pen(Color.Gray, 1f) { EndCap = LineCap.Flat };
        for (int i = 0; i <= AxeUnit; i++)
        {
            Pen.DashStyle = i == 0 ? DashStyle.Solid : DashStyle.Dash;
            string text = (i / (double) AxeUnit).ToString("f1");
            int AxeStep = ActualSize * i / AxeUnit;
            SizeF sizeF = Graphics.MeasureString(text, Font);

            Graphics.DrawLine(Pen, BorderLeft + AxeStep, ImageSize - BorderBottom, BorderLeft + AxeStep, ImageSize - BorderBottom - ActualSize);
            PointF pointX = new(BorderLeft + AxeStep - sizeF.Width / 2f, ImageSize - BorderBottom);
            Graphics.DrawString(text, Font, Brushes.Gray, pointX);

            Graphics.DrawLine(Pen, BorderLeft, ImageSize - BorderBottom - AxeStep, BorderLeft + ActualSize, ImageSize - BorderBottom - AxeStep);
            PointF pointY = new(BorderLeft - sizeF.Width, ImageSize - BorderBottom - AxeStep - sizeF.Height / 2f);
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
        Pen = new(Color.Blue);
        Graphics.DrawPolygon(Pen, chromaticity);
        GraphicsPath.AddPolygon(chromaticity);
    }

    private static void DrawRegions(Info info)
    {
        Pen.Color = Color.Black;
        Pen.DashStyle = DashStyle.Solid;
        Pen.Width = 2f;
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + 0.67 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.33 * ActualSize), Convert.ToInt16(BorderLeft + 0.21 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.71 * ActualSize));
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + 0.67 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.33 * ActualSize), Convert.ToInt16(BorderLeft + 0.14 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.08 * ActualSize));
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + 0.21 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.71 * ActualSize), Convert.ToInt16(BorderLeft + 0.14 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.08 * ActualSize));

        Pen.Color = Color.White;
        Pen.DashStyle = DashStyle.Dash;
        Pen.Width = 6f;
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesRedX) * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(Convert.ToDouble(info.AxesRedY) * ActualSize), Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesBlueX) * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(Convert.ToDouble(info.AxesBlueY) * ActualSize));
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesRedX) * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(Convert.ToDouble(info.AxesRedY) * ActualSize), Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesGreenX) * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(Convert.ToDouble(info.AxesGreenY) * ActualSize));
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesBlueX) * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(Convert.ToDouble(info.AxesBlueY) * ActualSize), Convert.ToInt16(BorderLeft + Convert.ToDouble(info.AxesGreenX) * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(Convert.ToDouble(info.AxesGreenY) * ActualSize));

        Pen.Color = Color.Black;
        Pen.DashStyle = DashStyle.Dot;
        Pen.Width = 3f;
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + 0.64 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.33 * ActualSize), Convert.ToInt16(BorderLeft + 0.3 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.6 * ActualSize));
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + 0.64 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.33 * ActualSize), Convert.ToInt16(BorderLeft + 0.15 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.06 * ActualSize));
        Graphics.DrawLine(Pen, Convert.ToInt16(BorderLeft + 0.3 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.6 * ActualSize), Convert.ToInt16(BorderLeft + 0.15 * ActualSize), ImageSize - BorderBottom - Convert.ToInt16(0.06 * ActualSize));

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

    private static BitmapSource Bitmap2Image(Bitmap bitmap)
    {
        IntPtr hbitmap = bitmap.GetHbitmap( );
        return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions( ));
    }
}
