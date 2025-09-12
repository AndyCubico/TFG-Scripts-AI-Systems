using UnityEngine;

public class ChaseSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    public Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        playerTransform = GameObject.Find("HangEdgeCheck").transform;
    }

    public virtual void DoEnter()
    {
        enemy.StopDangerParticles();
    }

    public virtual void DoExit() { ResetValues(); }

    public virtual void DoUpdate()
    {
        // Fix in case the animator gets stuck with the shooting animation.
        // Not adding transition from shooting to idle so that the animation is not suddenly cut.
        if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
        {
            enemy.SetTransitionAnimation("Chase");
        }

        if (enemy.isWithinAttackRange && !enemy.pathfollowing.isJumping)
        {
            enemy.stateMachine.Transition(enemy.attackState);
        }
    }

    public virtual void DoFixedUpdate() { }
    public virtual void ResetValues() { }
}
