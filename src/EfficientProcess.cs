using System;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace Qtl.RawWacom;

[SupportedOSPlatform("windows8.0")]
internal readonly struct EfficientProcess : IDisposable
{
	private readonly HANDLE _handle;

	public EfficientProcess()
	{
		var id = Native.GetCurrentProcessId();
		_handle = Native.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_SET_INFORMATION, false, id);
	}

	public unsafe bool SetEfficiencyMode()
	{
		var state = new PROCESS_POWER_THROTTLING_STATE
		{
			ControlMask = 1,
			StateMask = 1,
			Version = 1,
		};

		return
			Native.SetPriorityClass(_handle, PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS) &&
			Native.SetProcessInformation(_handle, PROCESS_INFORMATION_CLASS.ProcessPowerThrottling, &state, (uint)sizeof(PROCESS_POWER_THROTTLING_STATE));
	}

	public void Dispose()
	{
		if (_handle != HANDLE.Null)
		{
			_ = Native.CloseHandle(_handle);
		}
	}
}
