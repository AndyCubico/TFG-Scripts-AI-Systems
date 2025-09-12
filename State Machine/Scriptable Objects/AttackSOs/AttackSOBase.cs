using UnityEngine;

public class AttackSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected Transform playerTransform;
    public bool isParried;
    public bool isAttacking;

    public AnimationClip attackClip;

    public float damage = 10f;

    public EnemyHit attackEnemyHit;
    [SerializeField] string weaponName; 

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;

        this.enemy = enemy;

        if (weaponName != null)
            attackEnemyHit = enemy.GetWeaponHitByName(weaponName);

        isParried = false;
        isAttacking = false;

        playerTransform = GameObject.Find("HangEdgeCheck").transform;
    }

    public virtual void DoEnter()
    {
        enemy.SetTransitionAnimation("Attack");
    }

    public virtual void DoExit() { ResetValues(); }

    public virtual void DoUpdate()
    {
        if (!enemy.isWithinAttackRange && 
            !isParried && !enemy.attackSOBaseInstance.isAttacking)
        {
            enemy.stateMachine.Transition(enemy.chaseState);
            enemy.SetTransitionAnimation("Chase");
        }
    }

    public virtual void DoFixedUpdate() { }
    public virtual void ResetValues() { }
    public virtual void OnParried() { }
    public virtual void FinishParry() { }
    public virtual void PerformAttack() { }
}
