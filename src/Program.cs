
// TODO: add tablet buttons

using System;
using System.Threading;
using Qtl.RawWacom;
using Qtl.RawWacom.DataTypes;
using Qtl.Snippets;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

if (!OperatingSystem.IsWindowsVersionAtLeast(8))
{
	Console.WriteLine("The RawWacom library is not supported on your platform.");
	return;
}

using var singletonMutex = new Mutex(true, "0355C7C5-0302-4439-BCB9-F47EC236EFBC", out var isSingleInstance);
if (!isSingleInstance)
{
	return;
}

using var processInformation = new ProcessInformation();
_ = processInformation.SetEfficiencyMode();

using var wacomPenDevice = WacomTabletDevice.GetWacomTabletDevice(1);
var wacomPenState = new TimeBasedWacomPenStateTracker();

var message = default(WacomMessage);
while (wacomPenDevice.TryReadMessage(ref message))
{
	wacomPenState.MessageUpdate(ref message);

	if (message.MessageType is WacomMessageType.PenHovering)
	{
		SendMouseInput(wacomPenState);
	}
}

return;

unsafe void SendMouseInput(IWacomPenStateTracker penState)
{
	if (!penState.HasUpdated)
	{
		return;
	}

	var flags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE | MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE;

	if (penState.PenIsTouchingChanged)
	{
		flags |= penState.PenIsTouching ?
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN :
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP;
	}

	if (penState.PenButton0StateChanged)
	{
		flags |= penState.PenButton0State ?
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN :
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP;
	}

	if (penState.PenButton1StateChanged)
	{
		flags |= penState.PenButton1State ?
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN :
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP;
	}

	var input = new INPUT
	{
		type = INPUT_TYPE.INPUT_MOUSE,
		Anonymous = new INPUT._Anonymous_e__Union
		{
			mi = new MOUSEINPUT
			{
				dx = (int)(penState.Position.X * ushort.MaxValue),
				dy = (int)(penState.Position.Y * ushort.MaxValue),
				dwFlags = flags
			}
		}
	};

	_ = Native.SendInput(1, &input, sizeof(INPUT));
}
