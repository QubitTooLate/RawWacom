using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
struct WacomPenHovering
{
    [FieldOffset(8)] public WacomPenHoveringState PenHoveringState;
    [FieldOffset(6)] public short X;
    [FieldOffset(4)] public short Y;
    [FieldOffset(3)] public byte Pressure;
    [FieldOffset(0)] public byte HoverDistance;
}
