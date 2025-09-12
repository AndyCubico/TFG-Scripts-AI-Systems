using UnityEngine;

[CreateAssetMenu(fileName = "Idle Empty", menuName = "Enemy Logic/Idle/Idle Empty")]
public class IdleEmpty : IdleSOBase
{
    public override void DoEnter()
    {
        base.DoEnter();
    }

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
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
