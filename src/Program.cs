
// TODO: add tablet buttons

using Qtl.RawWacom;
using Qtl.RawWacom.DataTypes;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

using var process = new Process();
process.SetEfficiencyMode();

using var wacomPenDevice = WacomTabletDevice.GetWacomTabletDevice(1);
var wacomPenState = new WacomPenState();

var message = default(WacomMessage);
while (wacomPenDevice.TryReadMessage(ref message))
{
	wacomPenState.MessageUpdate(ref message);

	if (message.MessageType is WacomMessageType.PenHovering)
	{
		//Native.SendInput();

		Native.mouse_event(
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE |
			MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE |
			(wacomPenState.PenIsTouchingChanged ?
				(wacomPenState.PenIsTouching ?
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN :
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP
				) :
			default) |
			(wacomPenState.PenButton0StateChanged ?
				(wacomPenState.PenButton0State ?
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN :
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP
				) :
			default) |
			(wacomPenState.PenButton1StateChanged ?
				(wacomPenState.PenButton1State ?
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN :
					MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP
				) :
			default),
			(int)(wacomPenState.Position.X * ushort.MaxValue),
			(int)(wacomPenState.Position.Y * ushort.MaxValue),
			0,
			0
		);
	}
}
