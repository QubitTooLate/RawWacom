namespace Qtl.RawWacom;

internal struct BooleanStateTracker
{
	public BooleanStateTracker(bool initialState = false)
	{
		State = initialState;
		PreviousState = initialState;
		StateChanged = false;
	}

	public bool State { get; private set; }

	public bool PreviousState { get; private set; }

	public bool StateChanged { get; private set; }

	public void UpdateState(bool state)
	{
		PreviousState = State;
		State = state;
		StateChanged = PreviousState != State;
	}
}
