using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace monitorinfo;

public class Info : INotifyPropertyChanged
{
    private string bX;
    private string bY;
    private string gX;
    private string gY;
    private string rX;
    private string rY;
    private string wX;
    private string wY;
    private string colorFormat;
    private string edidVersion;
    private string manufacturerName;
    private Dictionary<string, string> manufacturerNameDic = new( )
    {
        { "ACR", "宏碁" },
        { "AOC", "冠捷科技(AOC)" },
        { "AUO", "友达光电" },
        { "AUS", "华硕" },
        { "BOE", "京东方" },
        { "CMN", "奇美" },
        { "CMO", "奇美" },
        { "DEL", "戴尔" },
        { "DLL", "戴尔" },
        { "ENV", "易美逊" },
        { "FDR", "方正" },
        { "GRG", "现代e派" },
        { "HKC", "惠科" },
        { "HPN", "惠普" },
        { "HYN", "现代" },
        { "JDI", "JDI" },
        { "LEN", "联想" },
        { "LGD", "LG Display" },
        { "LNV", "联想" },
        { "NEC", "NEC" },
        { "PHL", "飞利浦" },
        { "ROW", "乐华" },
        { "SAM", "三星" },
        { "SDC", "三星" },
        { "SHP", "夏普" },
        { "SNY", "索尼" },
        { "VSC", "优派" },
        { "XMD", "小米" },
    };
    private string monitorDataString;
    private string ntscGamut;
    private string physicalSize;
    private string productionDate;
    private string srgbGamut;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string BX
    {
        get => bX;
        set { bX = value; OnPropertyChanged(nameof(BX)); }
    }

    public string BY
    {
        get => bY;
        set { bY = value; OnPropertyChanged(nameof(BY)); }
    }

    public string GX
    {
        get => gX;
        set { gX = value; OnPropertyChanged(nameof(GX)); }
    }

    public string GY
    {
        get => gY;
        set { gY = value; OnPropertyChanged(nameof(GY)); }
    }

    public string RX
    {
        get => rX;
        set { rX = value; OnPropertyChanged(nameof(RX)); }
    }

    public string RY
    {
        get => rY;
        set { rY = value; OnPropertyChanged(nameof(RY)); }
    }

    public string WX
    {
        get => wX;
        set { wX = value; OnPropertyChanged(nameof(WX)); }
    }

    public string WY
    {
        get => wY;
        set { wY = value; OnPropertyChanged(nameof(WY)); }
    }

    public string ColorFormat
    {
        get => colorFormat;
        set { colorFormat = value; OnPropertyChanged(nameof(ColorFormat)); }
    }

    public string EDIDVersion
    {
        get => edidVersion;
        set { edidVersion = value; OnPropertyChanged(nameof(EDIDVersion)); }
    }

    public string ManufacturerName
    {
        get => manufacturerName;
        set { manufacturerName = value; OnPropertyChanged(nameof(ManufacturerName)); }
    }

    public string MonitorDataString
    {
        get => monitorDataString;
        set { monitorDataString = value; OnPropertyChanged(nameof(MonitorDataString)); }
    }

    public string NTSCGamut
    {
        get => ntscGamut;
        set { ntscGamut = value; OnPropertyChanged(nameof(NTSCGamut)); }
    }

    public string PhysicalSize
    {
        get => physicalSize;
        set { physicalSize = value; OnPropertyChanged(nameof(PhysicalSize)); }
    }

    public string ProductionDate
    {
        get => productionDate;
        set { productionDate = value; OnPropertyChanged(nameof(ProductionDate)); }
    }

    public string SRGBGamut
    {
        get => srgbGamut;
        set { srgbGamut = value; OnPropertyChanged(nameof(SRGBGamut)); }
    }

    public void Get(byte[] monitorEDID)
    {
        MonitorDataString = GetMonitorDataString(monitorEDID);
        EDIDVersion = "v" + monitorEDID[18].ToString( ) + "." + monitorEDID[19].ToString( );
        ProductionDate = (monitorEDID[17] + 1990).ToString( ) + "年" + ((monitorEDID[16] != 0) ? ("(第" + monitorEDID[16].ToString( ) + "周)") : "");
        ManufacturerName = GetMonitorManufacturerName(monitorEDID);
        PhysicalSize = GetMonitorPhysicalSize(monitorEDID);
        ColorFormat = GetColorFormat(monitorEDID);
        string[] monitorColorInfo = GetMonitorColorInfo(monitorEDID);
        RX = monitorColorInfo[0];
        RY = monitorColorInfo[1];
        GX = monitorColorInfo[2];
        GY = monitorColorInfo[3];
        BX = monitorColorInfo[4];
        BY = monitorColorInfo[5];
        WX = monitorColorInfo[6];
        WY = monitorColorInfo[7];
        NTSCGamut = monitorColorInfo[8];
        SRGBGamut = monitorColorInfo[9];
    }
    private string GetColorFormat(byte[] monitorEDID)
    {
        int num;
        string colorType;
        if ((monitorEDID[20] & 128) == 0)
        {
            num = monitorEDID[24] & 48;
            colorType = num != 0 ? num != 8 ? num != 16 ? "" : "(非RGB)" : "(RGB)" : "(黑白/灰度)";
            return "模拟信号 " + colorType;
        }
        num = monitorEDID[20] & 112;
        string colorDepth = num switch
        {
            16 => "6-bit",
            32 => "8-bit",
            48 => "10-bit",
            64 => "12-bit",
            80 => "14-bit",
            96 => "16-bit",
            _ => "",
        };
        colorType = (monitorEDID[24] & 48) == 0 ? "RGB " : "RGB/YCrCb ";
        return colorType + colorDepth;
    }

    private string[] GetMonitorColorInfo(byte[] monitorEDID)
    {
        double num = (4 * monitorEDID[27] + (monitorEDID[25] & 3)) / 1023.0;
        double num2 = (4 * monitorEDID[28] + (monitorEDID[25] & 3)) / 1023.0;
        double num3 = (4 * monitorEDID[29] + (monitorEDID[25] & 3)) / 1023.0;
        double num4 = (4 * monitorEDID[30] + (monitorEDID[25] & 3)) / 1023.0;
        double num5 = (4 * monitorEDID[31] + (monitorEDID[26] & 3)) / 1023.0;
        double num6 = (4 * monitorEDID[32] + (monitorEDID[26] & 3)) / 1023.0;
        double num7 = (4 * monitorEDID[33] + (monitorEDID[26] & 3)) / 1023.0;
        double num8 = (4 * monitorEDID[34] + (monitorEDID[26] & 3)) / 1023.0;
        double[] array = [num, num2, num3, num4, num5, num6, num7, num8];
        string NTSCPercent = (Math.Abs(array[0] * array[3] + array[2] * array[5] + array[4] * array[1] - array[0] * array[5] - array[2] * array[1] - array[4] * array[3]) / 2.0 / 0.1582).ToString("P") + " NTSC";
        string sRGBPercent = (Math.Abs(array[0] * array[3] + array[2] * array[5] + array[4] * array[1] - array[0] * array[5] - array[2] * array[1] - array[4] * array[3]) / 2.0 / 0.1582 / 0.72).ToString("P") + " sRGB";
        return
        [
            num.ToString("0.000"),
            num2.ToString("0.000"),
            num3.ToString("0.000"),
            num4.ToString("0.000"),
            num5.ToString("0.000"),
            num6.ToString("0.000"),
            num7.ToString("0.000"),
            num8.ToString("0.000"),
            NTSCPercent,
            sRGBPercent
        ];
    }

    private string GetMonitorDataString(byte[] monitorEDID)
    {
        string text;
        if (monitorEDID[111] == 254)
        {
            byte[] array = new byte[13];
            Array.Copy(monitorEDID, 113, array, 0, 13);
            text = Encoding.ASCII.GetString(array);
        }
        else
        {
            text = GetMonitorManufacturerID(monitorEDID) + Convert.ToString(monitorEDID[11], 16) + Convert.ToString(monitorEDID[10], 16);
        }
        return text.Trim( );
    }

    private string GetMonitorManufacturerID(byte[] monitorEDID)
    {
        byte[] array =
        [
            (byte)(((monitorEDID[8] & 124) >> 2) + 64),
            (byte)(((monitorEDID[8] & 3) << 3) + ((monitorEDID[9] & 224) >> 5) + 64),
            (byte) ((monitorEDID[9] & 31) + 64)
        ];
        return Encoding.ASCII.GetString(array);
    }

    private string GetMonitorManufacturerName(byte[] monitorEDID)
    {
        string monitorManufacturerID = GetMonitorManufacturerID(monitorEDID);
        return manufacturerNameDic.ContainsKey(monitorManufacturerID) ? manufacturerNameDic[monitorManufacturerID]
            : monitorManufacturerID;
    }

    private string GetMonitorPhysicalSize(byte[] monitorEDID)
    {
        return (Math.Sqrt(Math.Pow(monitorEDID[21], 2.0) + Math.Pow(monitorEDID[22], 2.0)) / 2.54).ToString("0.0") + "英寸";
    }
}
