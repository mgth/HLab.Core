using System.Runtime.InteropServices;
using System.Text;

namespace HLab.Sys.Windows.API.MonitorConfiguration;

public static partial class LowLevelMonitorConfiguration
{
    [DllImport("dxva2.dll", EntryPoint = "GetCapabilitiesStringLength", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCapabilitiesStringLength(
        [In] nint hMonitor, ref uint pdwLength);

    [DllImport("dxva2.dll", EntryPoint = "CapabilitiesRequestAndCapabilitiesReply", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CapabilitiesRequestAndCapabilitiesReply(
        [In] nint hMonitor, StringBuilder pszString, uint dwLength);

    public static bool GetCapabilitiesString(nint hMonitor, out string capabilities)
    {
        uint length = 0;
        if (GetCapabilitiesStringLength(hMonitor, ref length))
        {
            var sb = new StringBuilder((int)length);
            if (CapabilitiesRequestAndCapabilitiesReply(hMonitor, sb, length))
            {
                capabilities = sb.ToString();
                return true;
            }
        }
        capabilities = string.Empty;
        return false;
    }

    public enum VcpCode : uint
    {
       Power = 0xD6,
       PowerAlternate = 0xE1,
       InputSource = 0x60,
       Brightness = 0x10,
       Contrast = 0x12,
       RedGain = 0x16,
       GreenGain = 0x18,
       BlueGain = 0x1A,
       RedDrive = 0x52,
       GreenDrive = 0x54,
       BlueDrive = 0x56,
    }



    [DllImport("dxva2.dll", EntryPoint = "GetVCPFeatureAndVCPFeatureReply", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetVCPFeatureAndVCPFeatureReply(
        [In] nint hMonitor, [In] VcpCode dwVCPCode, out uint pvct, out uint pdwCurrentValue, out uint pdwMaximumValue);


    [DllImport("dxva2.dll", EntryPoint = "SetVCPFeature", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetVCPFeature(
        [In] nint hMonitor, VcpCode dwVCPCode, uint dwNewValue);

}