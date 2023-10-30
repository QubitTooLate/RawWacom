using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
struct WacomMessage
{
    [FieldOffset(9)] public WacomMessageType MessageType;
    [FieldOffset(0)] public WacomPenHovering PenHovering;
}
