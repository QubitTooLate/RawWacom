﻿using System.IO;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

namespace Qtl.RawWacom;

public static class DeviceFile
{
	public static unsafe FileStream OpenRead(string deviceName)
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
				FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DEVICE |
				FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OVERLAPPED,
				HANDLE.Null
			);

			return new FileStream(
				new SafeFileHandle(fileHandle, true),
				FileAccess.Read
			);
		}
	}
}
