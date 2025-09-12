using UnityEngine;

[CreateAssetMenu(fileName = "Ranged attack", menuName = "Enemy Logic/Attack/Ranged attack")]
public class RangedAttack : AttackSOBase
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;
    private float m_attackTimer;

    private bool m_playerInSight = false;

    [SerializeField] private LayerMask m_VisionMask;
    private Vector2 m_Direction;

    [SerializeField] private bool m_CanBeParried = false;

    public override void DoEnter() { }

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

        CheckPlayerSightAndHandleState();

        if (m_playerInSight)
        {
            m_attackTimer += Time.deltaTime;

            if (m_attackTimer >= attackCooldown)
            {
                m_attackTimer = 0;

                enemy.SetTransitionAnimation("Attack");
            }
        }
    }

    public void CheckPlayerSightAndHandleState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, m_VisionMask);

        if (hit.collider != null && hit.collider.transform.root.CompareTag("Player"))
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.green);
            m_playerInSight = true;
        }
        else
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red);
            m_playerInSight = false;
        }
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void OnParried()
    {
        base.OnParried();
    }

    public override void ResetValues()
    {
        base.ResetValues();
        m_attackTimer = 0;
    }

    // Method called in the animator to shoot the projectile.
    public override void PerformAttack()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        EnemyHit hitEnemy = projectile.GetComponent<EnemyHit>();
        hitEnemy.enemy = enemy;
        hitEnemy.canBeParried = m_CanBeParried;
        hitEnemy.damage = damage;

        projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
    }
}
