namespace Qtl.RawWacom.DataTypes;

internal enum WacomPenHoveringState : byte
{
	Nothing = 0,
	HoveringFar = 32,
	Left = 128,
	GettingFurther = 160,
	ComingCloser = 194,
	HoveringNear = 224,
	Touching = 225,
	PenButton0 = 226,
	PenButton0Touching = 227,
	PenButton1 = 228,
	PenButton1Touching = 229,
}
