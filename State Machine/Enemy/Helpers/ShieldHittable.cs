// Helper component for the shield collider
using UnityEngine;

public class ShieldHittable : MonoBehaviour, IHittableObject
{
    private ShieldedEnemy owner;

    public void Initialize(ShieldedEnemy owner)
    {
        this.owner = owner;
    }

    public void ReceiveDamage(float damage, AttackFlagType flag)
    {
        if (owner != null)
            owner.ReceiveShieldHit(damage, flag);
    }
}