using System;
using System.Collections.Generic;
using System.IO;

namespace monitorinfo;

internal static class ColorSpace
{
    public static Dictionary<double, double> SpectralChromaticity = [];

    static ColorSpace( )
    {
        LoadSpectralChromaticity("chroma.csv");
    }

    private static void LoadSpectralChromaticity(string filePath)
    {
        StreamReader reader = new(filePath);
        while (true)
        {
            string line = reader.ReadLine( );
            if (line is null) break;
            string[] values = line.Split(',');
            SpectralChromaticity.Add(double.Parse(values[0]), double.Parse(values[1]));
        }
        reader.Dispose( );
    }

    private const double xRed = 0.7355;
    private const double yRed = 0.2645;
    private const double zRed = 1.0 - xRed - yRed;
    private const double xGreen = 0.2658;
    private const double yGreen = 0.7243;
    private const double zGreen = 1.0 - xGreen - yGreen;
    private const double xBlue = 0.1669;
    private const double yBlue = 0.0085;
    private const double zBlue = 1.0 - xBlue - yBlue;
    private const double xW_E = 1.0 / 3.0;
    private const double yW_E = 1.0 / 3.0;
    private const double zW_E = 1.0 / 3.0;

    public static int ConstrainRGB(ref double r, ref double g, ref double b)
    {
        double num = (0.0 < r) ? 0.0 : r;
        num = (num < g) ? num : g;
        num = (num < b) ? num : b;
        num = -num;
        if (num > 0.0)
        {
            r += num;
            g += num;
            b += num;
            return 1;
        }
        return 0;
    }

    public static void NormalizeRGB(ref double r, ref double g, ref double b)
    {
        double num = Math.Max(r, Math.Max(g, b));
        if (num > 0.0)
        {
            r /= num;
            g /= num;
            b /= num;
        }
    }

    public static void XYZ2RGB(double xc, double yc, double zc, ref double r, ref double g, ref double b)
    {
        double yzbg = yGreen * zBlue - yBlue * zGreen;
        double xzbg = xBlue * zGreen - xGreen * zBlue;
        double xybg = xGreen * yBlue - xBlue * yGreen;
        double yzbr = yBlue * zRed - yRed * zBlue;
        double zxbr = xRed * zBlue - xBlue * zRed;
        double xybr = xBlue * yRed - xRed * yBlue;
        double yzrg = yRed * zGreen - yGreen * zRed;
        double xzrg = xGreen * zRed - xRed * zGreen;
        double xyrg = xRed * yGreen - xGreen * yRed;
        double bg = (yzbg * xW_E + xzbg * yW_E + xybg * zW_E) / yW_E;
        double br = (yzbr * xW_E + zxbr * yW_E + xybr * zW_E) / yW_E;
        double rg = (yzrg * xW_E + xzrg * yW_E + xyrg * zW_E) / yW_E;
        yzbg /= bg;
        xzbg /= bg;
        xybg /= bg;
        yzbr /= br;
        zxbr /= br;
        xybr /= br;
        yzrg /= rg;
        xzrg /= rg;
        xyrg /= rg;
        r = yzbg * xc + xzbg * yc + xybg * zc;
        g = yzbr * xc + zxbr * yc + xybr * zc;
        b = yzrg * xc + xzrg * yc + xyrg * zc;
    }
}
