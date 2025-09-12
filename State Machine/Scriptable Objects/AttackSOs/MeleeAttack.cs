using UnityEngine;

[CreateAssetMenu(fileName = "Melee Attack", menuName = "Enemy Logic/Attack/Melee Attack")]
public class MeleeAttack : AttackSOBase
{
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnter()
    {
        base.DoEnter();

        // Not in the base class because some enemies may not have a pathfollowing behaviour.
        enemy.pathfollowing.CancelJump();
        attackEnemyHit.canBeParried = true;

        enemy.StopDangerParticles();
    }

    public override void DoExit()
    {
        base.DoExit();
    }

    public override void DoUpdate()
    {
        base.DoUpdate();
    }

    public override void DoFixedUpdate()
    {
        base.DoFixedUpdate();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
