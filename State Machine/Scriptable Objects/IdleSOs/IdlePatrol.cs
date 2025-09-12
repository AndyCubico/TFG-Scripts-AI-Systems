using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle Patrol", menuName = "Enemy Logic/Idle/Idle Patrol")]
public class IdlePatrol : IdleSOBase
{
    [SerializeField] private float m_IdleWaitTime = 2.0f;

    [Header("WARNING, TO SET UP THE PATROL POSITIONS ATTACH PatrolSetup SCRIPT TO THE ENEMY.")]
    private int2 m_StartPosition = new int2();
    private int2 m_EndPosition = new int2();

    private int2 m_Destination = new int2();
    private float m_Timer = 0f;
    private bool m_IsWaiting = false;

    public override void DoEnter()
    {
        base.DoEnter();

        int2 currentPos = new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

        m_StartPosition = (int2)enemy.stateContext["PatrolStart"];
        m_EndPosition = (int2)enemy.stateContext["PatrolEnd"];

        // Start by going to end position.
        m_Destination = m_EndPosition;
        enemy.pathfollowing.SetPath(currentPos, m_Destination);
    }

    public override void DoExit()
    {
        base.DoExit();
    }

    public override void DoUpdate()
    {
        base.DoUpdate();
    }

    public override void DoFixedUpdate()
    {
        base.DoFixedUpdate();

        // Check if path has finished.
        if (enemy.pathfollowing.IsPathFinished())
        {
            if (!m_IsWaiting)
            {
                m_IsWaiting = true;
                m_Timer = 0f;
            }
            else
            {
                m_Timer += Time.fixedDeltaTime;

                if (m_Timer >= m_IdleWaitTime)
                {
                    SetDestination();
                    m_IsWaiting = false;
                }
            }
        }
    }
    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void SetDestination()
    {
        int2 currentPos = new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

        // Bounce between start and end positions.
        m_Destination = (m_Destination.Equals(m_EndPosition)) ? m_StartPosition : m_EndPosition;

        enemy.pathfollowing.SetPath(currentPos, m_Destination);
    }
}
