using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle Static", menuName = "Enemy Logic/Idle/Idle Static")]
public class IdleStatic : IdleSOBase
{
    [SerializeField]
    private float m_CheckInterval = 5f; // Interval in seconds, settable in inspector.
    private float m_TimeSinceLastCheck = 0f;

    private int2 m_StartPosition = new int2();

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);

        m_StartPosition = new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

        // For tall enemies like the elite, since the starting position has to be the floor the transform.position
        // in a large enemy could be in the node above. This ensures it still takes the floor.
        if (!GridManager.Instance.grid.GetValue(m_StartPosition.x, m_StartPosition.y).IsWalkable())
        {
            // Replace the original block with this loop.
            int y = Mathf.FloorToInt(transform.position.y);
            int x = Mathf.FloorToInt(transform.position.x);
            while (!GridManager.Instance.grid.GetValue(x, y).IsWalkable() && y > 0)
            {
                y -= 1;
            }
            m_StartPosition = new int2(x, y);
            m_StartPosition = new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y - 1));
        }

        m_TimeSinceLastCheck = 0f;
    }

    public override void DoEnter()
    {
        base.DoEnter();

        m_TimeSinceLastCheck = 0f;
        CheckReturnToStart();
    }

    public override void DoUpdate()
    {
        base.DoUpdate();

        m_TimeSinceLastCheck += Time.deltaTime;

        if (m_TimeSinceLastCheck >= m_CheckInterval)
        {
            CheckReturnToStart();
            m_TimeSinceLastCheck = 0f;
        }
    }

    private void CheckReturnToStart()
    {
        int2 currentPos = new int2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

        if (!currentPos.Equals(m_StartPosition))
        {
            enemy.pathfollowing.SetPath(currentPos, m_StartPosition);
        }
    }
}
