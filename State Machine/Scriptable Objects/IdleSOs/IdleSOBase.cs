using UnityEngine;

public class IdleSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        playerTransform = GameObject.Find("HangEdgeCheck").transform;
    }

    public virtual void DoEnter() { }
    public virtual void DoExit() { ResetValues(); }

    public virtual void DoUpdate()
    {
        // Fix in case the animator gets stuck with the attack animation.
        if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            enemy.SetTransitionAnimation("Idle");
        }

        if (enemy.isInSensor)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            LayerMask visionMask = LayerMask.GetMask("Player", "Ground");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, visionMask);

            // Debug ray
            Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, Color.red);

            if (hit.collider != null && hit.collider.transform.root.CompareTag("Player"))
            {
                enemy.stateMachine.Transition(enemy.chaseState);
                enemy.SetTransitionAnimation("Chase");
            }
        }
    }

    public virtual void DoFixedUpdate() { }
    public virtual void ResetValues() { }

}
