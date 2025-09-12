using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IHittableObject, IDamagable, IMovement, ITrigger, ITransition
{
    #region Damagable variables

    [field: SerializeField] public float m_MaxHealth { get; set; } = 100f;
    [field: SerializeField] public float currentHealth { get; set; }

    #endregion

    #region Movement variables

    public Pathfollowing pathfollowing { get; set; }

    #endregion

    #region Trigger variables

    public bool isInSensor { get; set; }
    public bool isWithinAttackRange { get; set; }

    #endregion

    #region State Machine variables

    public StateMachine stateMachine { get; set; }
    public IdleState idleState { get; set; }
    public ChaseState chaseState { get; set; }
    public AttackState attackState { get; set; }

    #endregion

    #region Scriptable object variables

    [SerializeField] private IdleSOBase m_IdleSOBase;
    [SerializeField] private ChaseSOBase m_ChaseSOBase;
    [SerializeField] private AttackSOBase m_AttackSOBase;

    // Instances so that the scriptable object does not modify every instance in the project.
    public IdleSOBase idleSOBaseInstance { get; set; }
    public ChaseSOBase chaseSOBaseInstance { get; set; }
    public AttackSOBase attackSOBaseInstance { get; set; }
    public Animator animator { get; set; }

    #endregion

    public Dictionary<string, object> stateContext { get; private set; } = new Dictionary<string, object>();

    public AttackFlagType attackFlagMask;

    [Header("Weapon GameObjects")]
    [SerializeField] private List<GameObject> m_WeaponObjects = new List<GameObject>();
    private Dictionary<string, EnemyHit> m_WeaponHitDict;

    public RuntimeAnimatorController animatorController;

    [SerializeField] private ParticleSystem m_DangerParticles;

    [SerializeField] private LayerMask m_DeathLayerMask;

    private bool canFlip { get; set; } = true; 

    public virtual void Awake()
    {
        idleSOBaseInstance = Instantiate(m_IdleSOBase);
        chaseSOBaseInstance = Instantiate(m_ChaseSOBase);
        attackSOBaseInstance = Instantiate(m_AttackSOBase);

        stateMachine = new StateMachine();

        // Initialize states
        idleState = new IdleState(this, stateMachine);
        chaseState = new ChaseState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);

        pathfollowing = GetComponent<Pathfollowing>();
        animator = GetComponent<Animator>();
        animatorController = animator.runtimeAnimatorController;

        canFlip = true;

        // Build dictionary using GameObject name as key.
        // Needed for the case of multiple weapons, like
        // the boss that shoots projectiles and has a melee attack.
        m_WeaponHitDict = new Dictionary<string, EnemyHit>();
        foreach (var obj in m_WeaponObjects)
        {
            if (obj != null)
            {
                var hit = obj.GetComponent<EnemyHit>();
                if (hit != null)
                    m_WeaponHitDict[obj.name] = hit;
            }
        }
    }

    protected virtual void Start()
    {
        currentHealth = m_MaxHealth;
        pathfollowing = GetComponent<Pathfollowing>();

        idleSOBaseInstance.Initialize(gameObject, this);
        chaseSOBaseInstance.Initialize(gameObject, this);
        attackSOBaseInstance.Initialize(gameObject, this);

        // Call starting state of the State Machine.
        stateMachine.Initialize(idleState);
    }

    public virtual void Update()
    {
        stateMachine.CurrentState.Update();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.CurrentState.FixedUpdate();
    }


    #region Basic functions

    public virtual void ReceiveDamage(float damage, AttackFlagType attackType)
    {
        if ((attackType & attackFlagMask) != 0)
        {
            currentHealth -= damage;

            GameManagerEvents.eSpawnDamageText(new Vector3(gameObject.transform.position.x, 
                gameObject.transform.position.y + gameObject.transform.localScale.y / 2, 
                transform.position.z), damage);

            if (currentHealth <= 0) Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public virtual void PerformAttack()
    {
        attackSOBaseInstance.PerformAttack();
    }

    public virtual void FinishParry()
    {
        attackSOBaseInstance.FinishParry();
    }

    public EnemyHit GetWeaponHitByName(string weaponName)
    {
        if (m_WeaponHitDict != null && m_WeaponHitDict.TryGetValue(weaponName, out var hit))
        {
            return hit;
        }

        return null;
    }

    #endregion

    #region Movement Functions

    public void CheckFacing(Vector2 velocity)
    {
        if (pathfollowing.isFacingRight && velocity.x < 0f) Flip();
        else if (!pathfollowing.isFacingRight && velocity.x > 0f) Flip();
    }

    public void Flip()
    {
        if (canFlip)
        {
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;
            pathfollowing.isFacingRight = !pathfollowing.isFacingRight;
        }
    }

    #endregion

    #region Trigger Functions

    public void SetInSensor(bool isInSensor)
    {
        this.isInSensor = isInSensor;
    }

    public void SetWithinAttackRange(bool isWithinAttackRange)
    {
        this.isWithinAttackRange = isWithinAttackRange;
    }

    #endregion

    #region Animation Transitions & Triggers

    public void SetTransitionAnimation(string trigger)
    {
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Chase");
        animator.ResetTrigger("WaitAttack");

        if (trigger == "Attack")
        {
            attackSOBaseInstance.isAttacking = true;
        }
        else if (attackSOBaseInstance.isAttacking)
        {
            attackSOBaseInstance.isAttacking = false;
        }

        animator.SetTrigger(trigger);
    }

    // Function to set whether the enemy can flip or not from an animation event.
    public void SetCanFlip(int isAttacking)
    {
        canFlip = Convert.ToBoolean(isAttacking);
    }

    #endregion

    #region State Functions 

    public void IdleTransition()
    {
        stateMachine.Transition(idleState);
        SetTransitionAnimation("Idle");
    }

    public void ChaseTransition()
    {
        stateMachine.Transition(chaseState);
        SetTransitionAnimation("Chase");
    }

    public void AttackTransition()
    {
        stateMachine.Transition(attackState);
        SetTransitionAnimation("Attack");
    }

    #endregion

    // Functions to manage the danger particles if the enemy has any.
    // Required here to be able to call from animation events.
    #region Particles
    public void PlayDangerParticles()
    {
        m_DangerParticles.Play();
    }

    public void StopDangerParticles()
    {
        if (m_DangerParticles != null)
        {
            m_DangerParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collided object's layer is in m_DeathLayerMask.
        if ((m_DeathLayerMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            Die();
        }
    }
}
