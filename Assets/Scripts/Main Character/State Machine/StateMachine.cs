public class StateMachine
{
    public IPlayerState CurrentState { get; private set; }

    public void Initialize(IPlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(IPlayerState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}