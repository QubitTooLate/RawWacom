using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Versioning;
using Qtl.RawWacom.DataTypes;

namespace Qtl.RawWacom;

[SupportedOSPlatform("windows5.0")]
internal sealed class FixedMoveTimeBasedWacomPenStateTracker : IWacomPenStateTracker
{
	private const float WACOM_MAX_WIDTH = 7600.0f;
	private const float WACOM_MAX_HEIGHT = 4750.0f;
	private const float WACOM_MAX_DEPTH = 255.0f;
	private const float WACOM_MAX_PRESSURE = 255.0f;
	private const float WACOM_PRESSURE_TOUCH_THRESHOLD = 0.01f;
	private const double SECONDS_TIMEOUT = 0.2;

	private BooleanStateTracker _penTouchingStateTracker;
	private BooleanStateTracker _penButton0StateTracker;
	private BooleanStateTracker _penButton1StateTracker;

	private long _penIsTouchingChangedTimestamp = 0;
	private long _timestamp;
	private long _penButton0StateChangedTimestamp;
	private long _penButton1StateChangedTimestamp;

	public Vector3 Position { get; private set; }
	public Vector3 TruePosition { get; private set; }
	public float Pressure { get; private set; }
	public bool PenIsTouching => _penTouchingStateTracker.State;
	public bool PenIsTouchingChanged => _penTouchingStateTracker.StateChanged;
	public bool PenButton0State => _penButton0StateTracker.State;
	public bool PenButton0StateChanged => _penButton0StateTracker.StateChanged;
	public bool PenButton1State => _penButton1StateTracker.State;
	public bool PenButton1StateChanged => _penButton1StateTracker.StateChanged;
	public Vector3 LeftAtPosition { get; private set; }
	public bool HasLeft { get; private set; }
	public bool HasUpdated => PenIsTouchingChanged || PenButton0StateChanged || PenButton1StateChanged || PositionChanged;
	public bool PositionChanged { get; private set; }

	public float ScrollDistance => throw new NotImplementedException();

	public void MessageUpdate(ref WacomMessage message)
	{
		if (message.MessageType is WacomMessageType.PenHovering)
		{
			ref var penHovering = ref message.PenHovering;
			UpdateTimestamp();
			UpdatePosition(ref penHovering);
			UpdatePressure(ref penHovering);
			UpdateButtons(ref penHovering);
		}
	}

	private void UpdateButtons(ref WacomPenHovering penHovering)
	{
		_penButton0StateTracker.UpdateState(penHovering.PenHoveringState is WacomPenHoveringState.PenButton0 or WacomPenHoveringState.PenButton0Touching);
		_penButton1StateTracker.UpdateState(penHovering.PenHoveringState is WacomPenHoveringState.PenButton1 or WacomPenHoveringState.PenButton1Touching);
	}

	private void UpdatePressure(ref WacomPenHovering penHovering)
	{
		if (penHovering.PenHoveringState is WacomPenHoveringState.ComingCloser)
		{
			return;
		}

		Pressure = penHovering.Pressure / WACOM_MAX_PRESSURE;

		_penTouchingStateTracker.UpdateState(Pressure >= WACOM_PRESSURE_TOUCH_THRESHOLD);
	}

	private void UpdatePosition(ref WacomPenHovering penHovering)
	{
		PositionChanged = false;
		TruePosition = new Vector3
		{
			X = penHovering.X / WACOM_MAX_WIDTH,
			Y = penHovering.Y / WACOM_MAX_HEIGHT,
			Z = 1.0f - (penHovering.HoverDistance / WACOM_MAX_DEPTH),
		};

		if (TruePosition.Z >= 1.0f)
		{
			if (!HasLeft)
			{
				LeftAtPosition = Position;
				Console.WriteLine(LeftAtPosition);
				HasLeft = true;
			}

			return;
		}
		else if (HasLeft)
		{
			HasLeft = false;
		}

		if (_penTouchingStateTracker.StateChanged)
		{
			_penIsTouchingChangedTimestamp = _penTouchingStateTracker.State ? GetTimestamp() : 0;
		}

		if (_penButton0StateTracker.StateChanged)
		{
			_penButton0StateChangedTimestamp = _penButton0StateTracker.State ? GetTimestamp() : 0;
		}

		if (_penButton1StateTracker.StateChanged)
		{
			_penButton1StateChangedTimestamp = _penButton1StateTracker.State ? GetTimestamp() : 0;
		}

		if (HasNotPassedTimeout(_penIsTouchingChangedTimestamp) ||
			HasNotPassedTimeout(_penButton0StateChangedTimestamp) ||
			HasNotPassedTimeout(_penButton1StateChangedTimestamp))
		{
			return;
		}

		PositionChanged = true;
		var z = TruePosition.Z;
		Position = (TruePosition - LeftAtPosition);
		Position = new Vector3(Position.X, Position.Y, z);
	}

	private void UpdateTimestamp() => _timestamp = Stopwatch.GetTimestamp();
	private long GetTimestamp() => _timestamp;
	private bool HasNotPassedTimeout(long timestamp) => Stopwatch.GetElapsedTime(timestamp).TotalSeconds < SECONDS_TIMEOUT;
}
