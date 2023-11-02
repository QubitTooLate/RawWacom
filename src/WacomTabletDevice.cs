using System;
using System.IO;
using Qtl.RawWacom.DataTypes;
using Windows.Win32.UI.Input;

namespace Qtl.RawWacom;

public sealed class WacomTabletDevice : IDisposable
{
	private const int ERROR_DEVICE_NOT_CONNECTED = -2147023729;

	public static unsafe WacomTabletDevice GetWacomTabletDevice(int index)
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
						var deviceFileStream = DeviceFile.OpenRead(deviceName);
						return new(deviceFileStream);
					}

					wacomIndex++;
				}
			}
		}

		throw new IOException("The device is not connected.", ERROR_DEVICE_NOT_CONNECTED);
	}

	private readonly FileStream _fileStream;

	public WacomTabletDevice(FileStream fileStream)
	{
		_fileStream = fileStream;
	}

	public unsafe bool TryReadMessage(ref WacomMessage message)
	{
		var length = 10;
		var messageBuffer = stackalloc byte[length];
		var messageSpan = new Span<byte>(messageBuffer, length);

		try
		{
			_fileStream.ReadExactly(messageSpan);
		}
		catch (IOException e) when (e.HResult is ERROR_DEVICE_NOT_CONNECTED)
		{
			return false;
		}

		messageSpan.Reverse();

		message = *(WacomMessage*)messageBuffer;
		return true;
	}

	public void Dispose()
	{
		_fileStream.Dispose();
	}
}
