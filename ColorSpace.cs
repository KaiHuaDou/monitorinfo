using System;

namespace monitorinfo;

internal static class ColorSpace
{
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

    private const double xW_E = 1.0 / 3.0;
    private const double yW_E = 1.0 / 3.0;
    private const double zW_E = 1.0 / 3.0;
    private const double xRed = 0.7355;
    private const double yRed = 0.2645;
    private const double zRed = 1.0 - xRed - yRed;
    private const double xGreen = 0.2658;
    private const double yGreen = 0.7243;
    private const double zGreen = 1.0 - xGreen - yGreen;
    private const double xBlue = 0.1669;
    private const double yBlue = 0.0085;
    private const double zBlue = 1.0 - xBlue - yBlue;

    public static double[,] SpectralChromaticity = new double[,]
    {
        { 0.1741, 0.005 },
        { 0.174, 0.005 },
        { 0.1738, 0.0049 },
        { 0.1736, 0.0049 },
        { 0.1733, 0.0048 },
        { 0.173, 0.0048 },
        { 0.1726, 0.0048 },
        { 0.1721, 0.0048 },
        { 0.1714, 0.0051 },
        { 0.1703, 0.0058 },
        { 0.1689, 0.0069 },
        { 0.1669, 0.0086 },
        { 0.1644, 0.0109 },
        { 0.1611, 0.0138 },
        { 0.1566, 0.0177 },
        { 0.151, 0.0227 },
        { 0.144, 0.0297 },
        { 0.1355, 0.0399 },
        { 0.1241, 0.0578 },
        { 0.1096, 0.0868 },
        { 0.0913, 0.1327 },
        { 0.0687, 0.2007 },
        { 0.0454, 0.295 },
        { 0.0235, 0.4127 },
        { 0.0082, 0.5384 },
        { 0.0039, 0.6548 },
        { 0.0139, 0.7502 },
        { 0.0389, 0.812 },
        { 0.0743, 0.8338 },
        { 0.1142, 0.8262 },
        { 0.1547, 0.8059 },
        { 0.1929, 0.7816 },
        { 0.2296, 0.7543 },
        { 0.2658, 0.7243 },
        { 0.3016, 0.6923 },
        { 0.3373, 0.6589 },
        { 0.3731, 0.6245 },
        { 0.4087, 0.5896 },
        { 0.4441, 0.5547 },
        { 0.4788, 0.5202 },
        { 0.5125, 0.4866 },
        { 0.5448, 0.4544 },
        { 0.5752, 0.4242 },
        { 0.6029, 0.3965 },
        { 0.627, 0.3725 },
        { 0.6482, 0.3514 },
        { 0.6658, 0.334 },
        { 0.6801, 0.3197 },
        { 0.6915, 0.3083 },
        { 0.7006, 0.2993 },
        { 0.7079, 0.292 },
        { 0.714, 0.2859 },
        { 0.719, 0.2809 },
        { 0.723, 0.277 },
        { 0.726, 0.274 },
        { 0.7283, 0.2717 },
        { 0.73, 0.27 },
        { 0.7311, 0.2689 },
        { 0.732, 0.268 },
        { 0.7327, 0.2673 },
        { 0.7334, 0.2666 },
        { 0.734, 0.266 },
        { 0.7344, 0.2656 },
        { 0.7346, 0.2654 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 },
        { 0.7347, 0.2653 }
    };
}
