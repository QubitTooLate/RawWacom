using System.Runtime.InteropServices;

namespace Qtl.RawWacom.DataTypes;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
public struct WacomPing
{
	[FieldOffset(0)] public byte IsPinging;
}
