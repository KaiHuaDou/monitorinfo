using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static monitorinfo.NativeMethods;

namespace monitorinfo;

public class Edid(byte[] data)
{
    public readonly byte[] monitorEDID = data;

    public static List<Edid> Get( )
    {
        List<Edid> list = [];
        IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GUID_CLASS_MONITOR));
        Marshal.StructureToPtr(GUID_CLASS_MONITOR, intPtr, false);
        SetupDiGetClassDevsEx(intPtr, null, IntPtr.Zero, 2, IntPtr.Zero, null, IntPtr.Zero);
        DISPLAY_DEVICE display_DEVICE = default;
        display_DEVICE.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
        uint num = 0U;
        bool flag = false;
        while (EnumDisplayDevices(null, num, ref display_DEVICE, 0U) && !flag)
        {
            DISPLAY_DEVICE display_DEVICE2 = default;
            display_DEVICE2.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            uint num2 = 0U;
            while (EnumDisplayDevices(display_DEVICE.DeviceName, num2, ref display_DEVICE2, 0U) && !flag)
            {
                if ((display_DEVICE2.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) != 0 && (display_DEVICE2.StateFlags & DisplayDeviceStateFlags.MirroringDriver) == 0)
                {
                    flag = GetActualEDID(out _, list);
                }
                num2 += 1U;
                display_DEVICE2 = default;
                display_DEVICE2.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            }
            display_DEVICE = default;
            display_DEVICE.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            num += 1U;
        }
        return list;
    }

    private static bool GetActualEDID(out string DeviceID, List<Edid> lsi)
    {
        IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GUID_CLASS_MONITOR));
        Marshal.StructureToPtr(GUID_CLASS_MONITOR, intPtr, false);
        IntPtr intPtr2 = SetupDiGetClassDevsEx(intPtr, null, IntPtr.Zero, 2, IntPtr.Zero, null, IntPtr.Zero);
        DeviceID = string.Empty;
        int num = 0;
        while (Marshal.GetLastWin32Error( ) != 259)
        {
            SP_DEVINFO_DATA sp_DEVINFO_DATA = default;
            sp_DEVINFO_DATA.cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
            if (SetupDiEnumDeviceInfo(intPtr2, num, ref sp_DEVINFO_DATA) > 0)
            {
                UIntPtr uintPtr = SetupDiOpenDevRegKey(intPtr2, ref sp_DEVINFO_DATA, 1, 0, 1, 131097);
                Edid edid = PullEDID(uintPtr);
                if (edid != null)
                {
                    lsi.Add(edid);
                }
                RegCloseKey(uintPtr);
            }
            num++;
        }
        Marshal.FreeHGlobal(intPtr);
        return true;
    }

    private static Edid PullEDID(UIntPtr hDevRegKey)
    {
        Edid edid = null;
        StringBuilder stringBuilder = new(128);
        uint num = 128U;
        byte[] array = new byte[1024];
        IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
        Marshal.Copy(array, 0, intPtr, array.Length);
        int num2 = 1024;
        uint num3 = 0U;
        uint num4 = 0U;
        while (num4 != 259U)
        {
            num4 = RegEnumValue(hDevRegKey, num3, stringBuilder, ref num, IntPtr.Zero, IntPtr.Zero, intPtr, ref num2);
            string text = stringBuilder.ToString( );
            if (num4 == 0U && text.Contains("EDID") && num2 >= 1)
            {
                byte[] array2 = new byte[num2];
                Marshal.Copy(intPtr, array2, 0, num2);
                edid = new Edid(array2);
            }
            num3 += 1U;
        }
        Marshal.FreeHGlobal(intPtr);
        return edid;
    }
}
