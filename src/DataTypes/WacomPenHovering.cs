using System.Runtime.InteropServices;

namespace Qtl.RawWacom.DataTypes;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
internal struct WacomPenHovering
{
    [FieldOffset(8)] public WacomPenHoveringState PenHoveringState;
    [FieldOffset(6)] public short X;
    [FieldOffset(4)] public short Y;
    [FieldOffset(3)] public byte Pressure;
    [FieldOffset(0)] public byte HoverDistance;
}
