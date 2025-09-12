using UnityEngine;

[CreateAssetMenu(fileName = "Chase Empty", menuName = "Enemy Logic/Chase/Chase Empty")]
public class ChaseEmpty : ChaseSOBase
{
    public override void DoEnter()
    {
        base.DoEnter();
    }

    public override void DoExit()
    {
        base.DoExit();
    }

    public override void DoFixedUpdate()
    {
        base.DoFixedUpdate();
    }

    public override void DoUpdate()
    {
        base.DoUpdate();

        if (!enemy.isInSensor)
        {
            enemy.stateMachine.Transition(enemy.idleState);
            enemy.SetTransitionAnimation("Idle");
        }
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
