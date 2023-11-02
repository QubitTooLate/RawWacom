
// TODO: add tablet buttons

using Qtl.RawWacom;
using Qtl.RawWacom.DataTypes;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

using var process = new EfficientProcess();
process.SetEfficiencyMode();

using var wacomPenDevice = WacomTabletDevice.GetWacomTabletDevice(1);
var wacomPenState = new WacomPenState();

var message = default(WacomMessage);
while (wacomPenDevice.TryReadMessage(ref message))
{
	wacomPenState.MessageUpdate(ref message);

	if (message.MessageType is WacomMessageType.PenHovering)
	{
		SendMouseInput(wacomPenState);
	}
}

static unsafe void SendMouseInput(WacomPenState penState)
{
	var input = new INPUT
	{
		type = INPUT_TYPE.INPUT_MOUSE,
		Anonymous = new INPUT._Anonymous_e__Union
		{
			mi = new MOUSEINPUT
			{
				dx = (int)(penState.Position.X * ushort.MaxValue),
				dy = (int)(penState.Position.Y * ushort.MaxValue),
				dwFlags =
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE |
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE |
					(penState.PenIsTouchingChanged ?
						(penState.PenIsTouching ?
							MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN :
							MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP
						) :
					default) |
					(penState.PenButton0StateChanged ?
						(penState.PenButton0State ?
							MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN :
							MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP
						) :
					default) |
					(penState.PenButton1StateChanged ?
						(penState.PenButton1State ?
							MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN :
							MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP
						) :
					default)
			}
		}
	};

	_ = Native.SendInput(1, &input, sizeof(INPUT));
}
