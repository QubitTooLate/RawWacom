﻿using System.Runtime.InteropServices;

namespace Qtl.RawWacom.DataTypes;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
public struct WacomMessage
{
	[FieldOffset(9)] public WacomMessageType MessageType;
	[FieldOffset(0)] public WacomPenHovering PenHovering;
}
