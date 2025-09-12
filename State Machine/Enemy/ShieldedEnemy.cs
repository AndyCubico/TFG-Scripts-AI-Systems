using UnityEngine;

public class ShieldedEnemy : Enemy
{
    [SerializeField] private bool shieldActive = true;
    [SerializeField] private ShieldHittable shieldObject; 

    public override void Awake()
    {
        base.Awake();

        if (shieldObject != null)
        {
            shieldObject.Initialize(this);
        }
    }

    public void ReceiveShieldHit(float damage, AttackFlagType attackType)
    {
        // This is called when the shield collider is hit.
        if (!shieldActive)
        {
            base.ReceiveDamage(damage, attackType);
            return;
        }
        else
        {
            if ((attackType & AttackFlagType.HEAVY_ATTACK) == 0)
                return;
        }

        BreakShield();
    }

    private void BreakShield()
    {
        shieldActive = false;

        SetTransitionAnimation("Stagger");
    }

    public void DeactivateShield()
    {
        if (shieldObject != null)
        {
            // Allow all attacks to affect this enemy after shield breaks, not optimal but works for now.
            attackFlagMask = AttackFlagType.ALL;

            shieldObject.gameObject.SetActive(false);
        }
    }
}
