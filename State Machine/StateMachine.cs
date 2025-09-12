public class StateMachine
{
    public State CurrentState { get; set; }

    public void Initialize(State initialState)
    {
        CurrentState = initialState;
        CurrentState.EnterState();
    }

    public void Transition(State nextState)
    {
        CurrentState.ExitState();
        CurrentState = nextState;
        CurrentState.EnterState();
    }
}
