using System;
using Qtl.RawWacom.DataTypes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Input;

namespace Qtl.RawWacom;

internal sealed class WacomTabletDevice : DeviceFile
{
	public static WacomTabletDevice GetWacomTabletDevice(int index)
	{
		var rawInputDevices = RawInputDevices.GetRawInputDevices();
		var wacomIndex = 0;
		for (var i = 0; i < rawInputDevices.Length; i++)
		{
			var rawInputDevice = rawInputDevices[i];
			if (rawInputDevice.dwType is RID_DEVICE_INFO_TYPE.RIM_TYPEHID)
			{
				var deviceName = RawInputDevices.GetRawInputDeviceName(rawInputDevice.hDevice);
				if (deviceName.Contains("VID_056A"))
				{
					if (wacomIndex == index)
					{
						return CreateDeviceFile(deviceName);
					}

					wacomIndex++;
				}
			}
		}

		throw new Exception("Wacom device not found.");
	}

	private static unsafe WacomTabletDevice CreateDeviceFile(string deviceName)
	{
		const uint GENERIC_READ = 0x80000000;

		fixed (char* deviceNamePtr = deviceName)
		{
			var fileHandle = Native.CreateFile(
				deviceNamePtr,
				GENERIC_READ,
				FILE_SHARE_MODE.FILE_SHARE_NONE,
				null,
				FILE_CREATION_DISPOSITION.OPEN_EXISTING,
				FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DEVICE | FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OVERLAPPED,
				HANDLE.Null
			);

			var eventHandle = Native.CreateEvent(
				null,
				true,
				false,
				default(PCWSTR)
			);

			return new(fileHandle, eventHandle);
		}
	}

	private WacomTabletDevice(HANDLE fileHandle, HANDLE eventHandle) : base(fileHandle, eventHandle)
	{

	}

	public unsafe bool TryReadMessage(ref WacomMessage message)
	{
		var length = 10;
		var messageBuffer = stackalloc byte[length];
		if (!TryRead(messageBuffer, &length)) { return false; }

		new Span<byte>(messageBuffer, length).Reverse();

		message = *(WacomMessage*)messageBuffer;
		return true;
	}
}
