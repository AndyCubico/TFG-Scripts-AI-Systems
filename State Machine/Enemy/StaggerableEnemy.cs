using UnityEngine;

public class StaggerableEnemy : Enemy
{
    [Header("Stagger Settings")]
    [SerializeField] private int heavyHitsToBreak = 3;
    private int currentHeavyHits = 0;

    [Header("WaitAttack Backstep Settings")]
    [SerializeField] private float backstepSpeed = 1.0f;
    public string waitAttackStateName = "WaitAttack";
    public bool isInWaitAttack = false;

    public override void Awake()
    {
        base.Awake();
        currentHeavyHits = 0;
    }

    public override void Update()
    {
        base.Update();

        PerformWaitAttack();
    }

    public override void ReceiveDamage(float damage, AttackFlagType attackType)
    {
        base.ReceiveDamage(damage, attackType);

        if ((attackType & AttackFlagType.HEAVY_ATTACK) != 0)
        {
            currentHeavyHits++;
            if (currentHeavyHits >= heavyHitsToBreak)
            {
                Stagger();
            }
        }
    }

    private void Stagger()
    {
        currentHeavyHits = 0;
        SetTransitionAnimation("Stagger");
    }

    public virtual void PerformWaitAttack()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(waitAttackStateName))
        {
            if (!isInWaitAttack)
                isInWaitAttack = true;

            // Move away from the player.
            Vector3 directionAway = (transform.position - chaseSOBaseInstance.playerTransform.position).normalized;
            transform.position += directionAway * backstepSpeed * Time.deltaTime;
        }
        else if (isInWaitAttack)
        {
            isInWaitAttack = false;
        }
    }
}
