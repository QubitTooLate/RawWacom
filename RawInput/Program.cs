using RawInput;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;

const double WACOM_MAX_WIDTH = 7600.0;
const double WACOM_MAX_HEIGHT = 4750.0;
const double MULTIPLIER = ushort.MaxValue;

var screenWidth = Native.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
var screenHeight = Native.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);

{
    using var wacomDevice = WacomTabletDevice.GetWacomTabletDevice();

    var previousIsDown = false;
    var message = default(WacomMessage);
    while (!Console.KeyAvailable && wacomDevice.TryReadMessage(ref message))
    {
        if (message.MessageType is WacomMessageType.PenHovering)
        {
            var penHovering = message.PenHovering;
            var x = (penHovering.X / WACOM_MAX_WIDTH) * MULTIPLIER;
            var y = (penHovering.Y / WACOM_MAX_HEIGHT) * MULTIPLIER;
            var isDown = penHovering.Pressure > 16;

            var update = previousIsDown == !isDown;
            previousIsDown = isDown;

            //Native.SetCursorPos((int)(x * screenWidth), (int)(y * screenHeight));

            Native.mouse_event(
                MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE | 
                MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE | (update ?
                (isDown ? MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN : MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP) : default),
                (int)x,
                (int)y,
                0,
                0
            );

            //Console.WriteLine($"{isDown}, {penHovering.Pressure}, {penHovering.PenHoveringState}, {penHovering.X}, {penHovering.Y}, {penHovering.HoverDistance}");
        }
    }
}

return;

{
    using var rid =
        GetRawInputDeviceInfos()
        .Where(ridi =>
            ridi.DeviceName.Contains("VID_056A") &&
            ridi.Type == RID_DEVICE_INFO_TYPE.RIM_TYPEHID
        )
        .Skip(1)
        .First();

    var buffer = new byte[64].AsSpan();
    while (!Console.KeyAvailable && rid.TryRead(buffer, out var length))
    {
        var messageBuffer = buffer[..length];
        messageBuffer.Reverse();

        ref var message = ref Unsafe.As<byte, WacomMessage>(ref MemoryMarshal.GetReference(messageBuffer));
        if (message.MessageType is WacomMessageType.PenHovering)
        {
            var penHovering = message.PenHovering;
            var x = penHovering.X / WACOM_MAX_WIDTH;
            var y = penHovering.Y / WACOM_MAX_HEIGHT;

            Native.SetCursorPos((int)(x * screenWidth), (int)(y * screenHeight));

            Console.WriteLine($"{penHovering.PenHoveringState}, {penHovering.X}, {penHovering.Y}, {penHovering.HoverDistance}");
        }
    }
}

return;


static unsafe string GetRawInputDeviceName(HANDLE handle)
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

static unsafe RawInputDevice[] GetRawInputDeviceInfos()
{
    var deviceAvailableCount = 0;
    Native.GetRawInputDeviceList(
        null,
        (uint*)&deviceAvailableCount,
        (uint)sizeof(RAWINPUTDEVICELIST)
    );
    var stackRawInputDeviceList = stackalloc RAWINPUTDEVICELIST[deviceAvailableCount];
    var count = Native.GetRawInputDeviceList(
        stackRawInputDeviceList,
        (uint*)&deviceAvailableCount,
        (uint)sizeof(RAWINPUTDEVICELIST)
    );

    var rawInputDeviceInfos = new RawInputDevice[count];

    for (var i = 0; i < count; i++)
    {
    rawInputDeviceInfos[i] = new RawInputDevice(
        stackRawInputDeviceList[i].hDevice,
        stackRawInputDeviceList[i].dwType,
        GetRawInputDeviceName(stackRawInputDeviceList[i].hDevice)
    );
    }

    return rawInputDeviceInfos;
}
