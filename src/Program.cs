using System;
using Qtl.RawWacom;
using Qtl.RawWacom.DataTypes;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

{
    using var wacomDevice = WacomTabletDevice.GetWacomTabletDevice();
    var wacomState = new WacomState();

    var message = default(WacomMessage);
    while (!Console.KeyAvailable && wacomDevice.TryReadMessage(ref message))
    {
        wacomState.MessageUpdate(ref message);

        if (message.MessageType is WacomMessageType.PenHovering)
        {
            Native.mouse_event(
                MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE |
                MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE | 
                (wacomState.PenIsTouchingChanged ?
                    (wacomState.PenIsTouching ? 
                        MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN : 
                        MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP
                    ) : 
                default) |
                (wacomState.PenButton0StateChanged ?
                    (wacomState.PenButton0State ?
                        MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN :
                        MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP
                    ) :
                default) |
                (wacomState.PenButton1StateChanged ?
                    (wacomState.PenButton1State ?
                        MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN :
                        MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP
                    ) :
                default),
                (int)(wacomState.Position.X * ushort.MaxValue),
                (int)(wacomState.Position.Y * ushort.MaxValue),
                0,
                0
            );
        }
    }
}
