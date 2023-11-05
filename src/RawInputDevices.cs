using System;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input;

[SupportedOSPlatform("windows5.1.2600")]
internal static class RawInputDevices
{
	public static unsafe int GetRawInputDeviceCount()
	{
		var count = 0;
		_ = Native.GetRawInputDeviceList(
			null,
			(uint*)&count,
			(uint)sizeof(RAWINPUTDEVICELIST)
		);
		return count;
	}

	public static unsafe int WriteRawInputDevicesToSpan(Span<RAWINPUTDEVICELIST> rawInputDevices)
	{
		var count = rawInputDevices.Length;
		fixed (RAWINPUTDEVICELIST* rawInputDevicesPtr = rawInputDevices)
		{
			return (int)Native.GetRawInputDeviceList(
				rawInputDevicesPtr,
				(uint*)&count,
				(uint)sizeof(RAWINPUTDEVICELIST)
			);
		}
	}

	public static Span<RAWINPUTDEVICELIST> GetRawInputDevices()
	{
		var count = GetRawInputDeviceCount();
		var rawInputDevices = new RAWINPUTDEVICELIST[count].AsSpan();
		count = WriteRawInputDevicesToSpan(rawInputDevices);
		return rawInputDevices[..count];
	}

	public static unsafe string GetRawInputDeviceName(HANDLE handle)
	{
		var bufferLength = 128;
		var buffer = stackalloc char[bufferLength];
		var length = Native.GetRawInputDeviceInfo(
			handle,
			RAW_INPUT_DEVICE_INFO_COMMAND.RIDI_DEVICENAME,
			buffer,
			(uint*)&bufferLength
		);

		return new string(buffer, 0, (int)length);
	}
}
