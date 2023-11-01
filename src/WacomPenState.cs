using System.Numerics;
using Qtl.RawWacom.DataTypes;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Qtl.RawWacom;

internal sealed class WacomPenState
{
	private const float WACOM_MAX_WIDTH = 7600.0f;
	private const float WACOM_MAX_HEIGHT = 4750.0f;
	private const float WACOM_MAX_DEPTH = 255.0f;
	private const float WACOM_MAX_PRESSURE = 255.0f;
	private const float WACOM_PRESSURE_TOUCH_THRESHOLD = 0.01f;
	private const float PREVENT_DRAG_THRESHOLD = 0.025f;

	private readonly float _ratio;

	private bool _penWasTouching;
	private bool _passedDragThreshold;
	private Vector3 _penStartedTouchingPosition;
	private bool _penButton0StateWasTrue;
	private bool _penButton1StateWasTrue;

	public Vector3 Position { get; private set; }

	public Vector3 TruePosition { get; private set; }

	public float Pressure { get; private set; }

	public bool PenIsTouching { get; private set; }

	public bool PenIsTouchingChanged { get; private set; }

	public bool PenButton0State { get; private set; }

	public bool PenButton0StateChanged { get; private set; }

	public bool PenButton1State { get; private set; }

	public bool PenButton1StateChanged { get; private set; }

	public Vector3 LeftAtPosition { get; private set; }

	public bool HasLeft { get; private set; }

	public WacomPenState()
	{
		var screenWidth = Native.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
		var screenHeight = Native.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
		_ratio = (float)screenWidth / screenHeight;
	}

	public void MessageUpdate(ref WacomMessage message)
	{
		if (message.MessageType is WacomMessageType.PenHovering)
		{
			ref var penHovering = ref message.PenHovering;
			UpdatePosition(ref penHovering);
			UpdatePressure(ref penHovering);
			UpdateButtons(ref penHovering);
		}
	}

	private void UpdateButtons(ref WacomPenHovering penHovering)
	{
		_penButton0StateWasTrue = PenButton0State;
		PenButton0State = penHovering.PenHoveringState is WacomPenHoveringState.PenButton0 or WacomPenHoveringState.PenButton0Touching;
		PenButton0StateChanged = PenButton0State == !_penButton0StateWasTrue;

		_penButton1StateWasTrue = PenButton1State;
		PenButton1State = penHovering.PenHoveringState is WacomPenHoveringState.PenButton1 or WacomPenHoveringState.PenButton1Touching;
		PenButton1StateChanged = PenButton1State == !_penButton1StateWasTrue;
	}

	private void UpdatePressure(ref WacomPenHovering penHovering)
	{
		if (penHovering.PenHoveringState is WacomPenHoveringState.ComingCloser)
		{
			return;
		}

		Pressure = penHovering.Pressure / WACOM_MAX_PRESSURE;

		_penWasTouching = PenIsTouching;
		PenIsTouching = Pressure >= WACOM_PRESSURE_TOUCH_THRESHOLD;
		PenIsTouchingChanged = PenIsTouching == !_penWasTouching;
	}

	private void UpdatePosition(ref WacomPenHovering penHovering)
	{
		TruePosition = new Vector3
		{
			X = penHovering.X / WACOM_MAX_WIDTH,
			Y = penHovering.Y / WACOM_MAX_HEIGHT,
			Z = 1.0f - (penHovering.HoverDistance / WACOM_MAX_DEPTH),
		};

		if (PenIsTouching)
		{
			if (PenIsTouchingChanged)
			{
				_passedDragThreshold = false;
				_penStartedTouchingPosition = TruePosition;
			}

			if (!_passedDragThreshold)
			{
				_passedDragThreshold = (TruePosition - _penStartedTouchingPosition).Length() > PREVENT_DRAG_THRESHOLD;
				if (!_passedDragThreshold)
				{
					return;
				}
			}
		}

		if (TruePosition.Z >= 1.0f)
		{
			if (!HasLeft)
			{
				LeftAtPosition = TruePosition;
				HasLeft = true;
			}
			return;
		}
		else if (HasLeft)
		{
			HasLeft = false;
		}

		Position = TruePosition;
	}
}
