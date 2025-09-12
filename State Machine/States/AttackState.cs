public class AttackState : State
{
    public AttackState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void EnterState()
    {
        base.EnterState();

        enemy.attackSOBaseInstance.DoEnter();
    }

    public override void ExitState()
    {
        base.ExitState();

        enemy.attackSOBaseInstance.DoExit();
    }
    public override void Update()
    {
        base.Update();

        enemy.attackSOBaseInstance.DoUpdate();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        enemy.attackSOBaseInstance.DoFixedUpdate();
    }
}
