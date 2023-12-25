using System.Numerics;
using Qtl.RawWacom.DataTypes;

namespace Qtl.RawWacom;

internal interface IWacomPenStateTracker
{
	Vector3 Position { get; }
	Vector3 TruePosition { get; }
	float Pressure { get; }
	bool PenIsTouching { get; }
	bool PenIsTouchingChanged { get; }
	bool PenButton0State { get; }
	bool PenButton0StateChanged { get; }
	bool PenButton1State { get; }
	bool PenButton1StateChanged { get; }
	Vector3 LeftAtPosition { get; }
	bool HasLeft { get; }
	void MessageUpdate(ref WacomMessage message);
	bool HasUpdated { get; }
	bool PositionChanged { get; }
	float ScrollDistance { get; }
}
