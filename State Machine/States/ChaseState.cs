using UnityEngine;

public class ChaseState : State
{
    public ChaseState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void EnterState()
    {
        base.EnterState();

        enemy.chaseSOBaseInstance.DoEnter();
    }

    public override void ExitState()
    {
        base.ExitState();

        enemy.chaseSOBaseInstance.DoExit();
    }

    public override void Update()
    {
        base.Update();

        enemy.chaseSOBaseInstance.DoUpdate();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        enemy.chaseSOBaseInstance.DoFixedUpdate();
    }
}
