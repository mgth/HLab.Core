using System;
using System.Runtime.InteropServices;

namespace HLab.Sys.Windows.API;

/// <summary>
/// CCD (Connecting and Configuring Displays) API. Unlike ChangeDisplaySettingsEx,
/// which addresses an adapter *source* (\\.\DISPLAYn), these functions expose the
/// source -> target paths and allow to (de)activate a display by *monitor* identity.
/// This matters for detached monitors: they are enumerated as candidates under every
/// inactive source, so a source name alone no longer designates a monitor.
/// </summary>
public static partial class WinUser
{
    public const uint QDC_ALL_PATHS = 1;
    public const uint QDC_ONLY_ACTIVE_PATHS = 2;

    public const uint DISPLAYCONFIG_PATH_ACTIVE = 1;
    public const uint DISPLAYCONFIG_PATH_MODE_IDX_INVALID = 0xffffffff;

    public const uint DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
    public const uint DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;

    [Flags]
    public enum SetDisplayConfigFlags : uint
    {
        TopologyInternal = 0x00000001,
        TopologyClone = 0x00000002,
        TopologyExtend = 0x00000004,
        TopologyExternal = 0x00000008,
        UseSuppliedDisplayConfig = 0x00000020,
        Validate = 0x00000040,
        Apply = 0x00000080,
        NoOptimization = 0x00000100,
        SaveToDatabase = 0x00000200,
        AllowChanges = 0x00000400,
        PathPersistIfRequired = 0x00000800,
        ForceModeEnumeration = 0x00001000,
        AllowPathOrderChanges = 0x00002000,
        VirtualModeAware = 0x00008000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Luid
    {
        public uint LowPart;
        public int HighPart;

        public readonly bool Equals(Luid other) => LowPart == other.LowPart && HighPart == other.HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigPathSourceInfo
    {
        public Luid AdapterId;
        public uint Id;
        public uint ModeInfoIdx;
        public uint StatusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigPathTargetInfo
    {
        public Luid AdapterId;
        public uint Id;
        public uint ModeInfoIdx;
        public uint OutputTechnology;
        public uint Rotation;
        public uint Scaling;
        public uint RefreshRateNumerator;
        public uint RefreshRateDenominator;
        public uint ScanLineOrdering;
        [MarshalAs(UnmanagedType.Bool)]
        public bool TargetAvailable;
        public uint StatusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigPathInfo
    {
        public DisplayConfigPathSourceInfo SourceInfo;
        public DisplayConfigPathTargetInfo TargetInfo;
        public uint Flags;
    }

    /// <summary>
    /// The 48-byte union payload (target/source/desktop image mode) is carried opaquely:
    /// it is only round-tripped between QueryDisplayConfig and SetDisplayConfig.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    public struct DisplayConfigModeInfo
    {
        public uint InfoType;
        public uint Id;
        public Luid AdapterId;
        public ulong U0; public ulong U1; public ulong U2;
        public ulong U3; public ulong U4; public ulong U5;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DisplayConfigTargetDeviceName
    {
        public uint Type;
        public uint Size;
        public Luid AdapterId;
        public uint Id;
        public uint Flags;
        public uint OutputTechnology;
        public ushort EdidManufactureId;
        public ushort EdidProductCodeId;
        public uint ConnectorInstance;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string MonitorFriendlyDeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string MonitorDevicePath;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DisplayConfigSourceDeviceName
    {
        public uint Type;
        public uint Size;
        public Luid AdapterId;
        public uint Id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ViewGdiDeviceName;
    }

    [DllImport("user32.dll")]
    public static extern int GetDisplayConfigBufferSizes(uint flags, ref uint numPathArrayElements, ref uint numModeInfoArrayElements);

    [DllImport("user32.dll")]
    public static extern int QueryDisplayConfig(uint flags,
        ref uint numPathArrayElements, [In, Out] DisplayConfigPathInfo[] pathArray,
        ref uint numModeInfoArrayElements, [In, Out] DisplayConfigModeInfo[] modeInfoArray,
        nint currentTopologyId);

    [DllImport("user32.dll")]
    public static extern int SetDisplayConfig(uint numPathArrayElements, DisplayConfigPathInfo[] pathArray,
        uint numModeInfoArrayElements, DisplayConfigModeInfo[] modeInfoArray, SetDisplayConfigFlags flags);

    [DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo")]
    public static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigTargetDeviceName requestPacket);

    [DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo")]
    public static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigSourceDeviceName requestPacket);
}
