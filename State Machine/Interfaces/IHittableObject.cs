using UnityEngine;

public interface IHittableObject
{
    public virtual void ReceiveDamage(float damage, AttackFlagType flag) { }
    public virtual void PushEnemy(GameObject player) { }
}
