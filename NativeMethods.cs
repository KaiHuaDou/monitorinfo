using System.Runtime.InteropServices;
using System.Text;
using System;

namespace monitorinfo;

internal sealed class NativeMethods
{
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern uint RegEnumValue(UIntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcValueName, IntPtr lpReserved, IntPtr lpType, IntPtr lpData, ref int lpcbData);

    [DllImport("setupapi.dll")]
    public static extern IntPtr SetupDiGetClassDevsEx(IntPtr ClassGuid, [MarshalAs(UnmanagedType.LPStr)] string enumerator, IntPtr hwndParent, int Flags, IntPtr DeviceInfoSet, [MarshalAs(UnmanagedType.LPStr)] string MachineName, IntPtr Reserved);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern int SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInterfaceData);

    [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern UIntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, int scope, int hwProfile, int parameterRegistryValueKind, int samDesired);

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern int RegCloseKey(UIntPtr hKey);

    public static readonly Guid GUID_CLASS_MONITOR = new(1295444334U, 58149, 4558, 191, 193, 8, 0, 43, 225, 3, 24);

    [Flags]
    public enum DisplayDeviceStateFlags
    {
        AttachedToDesktop = 1,
        MultiDriver = 2,
        PrimaryDevice = 4,
        MirroringDriver = 8,
        VGACompatible = 16,
        Removable = 32,
        ModesPruned = 134217728,
        Remote = 67108864,
        Disconnect = 33554432
    }

    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    public struct SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }
}
