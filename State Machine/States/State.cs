public class State
{
    protected Enemy enemy;
    protected StateMachine stateMachine;

    public State(Enemy enemy, StateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void EnterState() { }

    public virtual void ExitState() { }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }
}
