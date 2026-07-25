/*
  HLab.Windows.API
  Copyright (c) 2021 Mathieu GRENET.  All right reserved.

  This file is part of HLab.Windows.API.

    HLab.Windows.API is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HLab.Windows.API is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

// ReSharper disable InconsistentNaming

namespace HLab.Sys.Windows.API;

[global::System.Security.SuppressUnmanagedCodeSecurity]
public static partial class WinReg
{
    public static RegistryKey RegistryKey(nint hKey, int parent = 0) => RegistryKey(GetHKeyName(hKey), parent);
    public static RegistryKey RegistryKey(string path, int parent = 0)
    {
        var keys = path.Split('\\');

        if (keys.Length < 3) throw new InvalidOperationException("path is not valid.");

        var key = keys[2] switch
        {
            "USER" => Registry.CurrentUser,
            "CONFIG" => Registry.CurrentConfig,
            _ => Registry.LocalMachine
        };

        for (var i = 3; i < (keys.Length - parent); i++)
        {
            if (key == null) return key;
            key = key.OpenSubKey(keys[i]);
        }

        return key;
    }

    public static string GetHKeyName(nint hKey)
    {
        var status = Wdm.ZwQueryKey(hKey, Wdm.KeyInformationClass.KeyNameInformation, 0, 0, out var needed);
        if (status != 0xC0000023 /* STATUS_BUFFER_TOO_SMALL */ || needed < sizeof(uint)) return string.Empty;

        var buffer = Marshal.AllocHGlobal(needed);
        try
        {
            var capacity = needed;
            status = Wdm.ZwQueryKey(hKey, Wdm.KeyInformationClass.KeyNameInformation, buffer, capacity, out var returned);
            if (status != 0 /* STATUS_SUCCESS */ || returned < sizeof(uint) || returned > capacity)
                return string.Empty;

            var bytes = new byte[returned];
            Marshal.Copy(buffer, bytes, 0, returned);
            return DecodeKeyNameInformation(bytes);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Decode byte-counted KEY_NAME_INFORMATION without unmanaged over-read.</summary>
    public static string DecodeKeyNameInformation(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < sizeof(uint)) return string.Empty;
        var nameLength = BitConverter.ToUInt32(buffer[..sizeof(uint)]);
        if ((nameLength & 1) != 0 || nameLength > buffer.Length - sizeof(uint))
            throw new InvalidDataException("Invalid KEY_NAME_INFORMATION byte length.");

        return Encoding.Unicode.GetString(
            buffer.Slice(sizeof(uint), checked((int)nameLength)));
    }


    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint RegEnumValue(
        nint hKey,
        uint dwIndex,
        StringBuilder lpValueName,
        ref uint lpcValueName,
        nint lpReserved,
        ref uint lpType,
        nint lpData,
        ref uint lpcbData);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    public static partial int RegCloseKey(nint hKey);
}