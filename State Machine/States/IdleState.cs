public class IdleState : State
{
    public IdleState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void EnterState()
    {
        base.EnterState();

        enemy.idleSOBaseInstance.DoEnter();
    }

    public override void ExitState()
    {
        base.ExitState();

        enemy.idleSOBaseInstance.DoExit();
    }

    public override void Update()
    {
        base.Update();

        enemy.idleSOBaseInstance.DoUpdate();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        enemy.idleSOBaseInstance.DoFixedUpdate();
    }
}
