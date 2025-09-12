using Unity.Mathematics;
using UnityEngine;

public class PatrolSetup : MonoBehaviour
{
    // Script to be able to set patrol positions in the inspector for each enemy.
    // With the usage of SOs, with a shared asset every enemy would patrol the same positions.
    [SerializeField] private Vector2Int patrolStart;
    [SerializeField] private Vector2Int patrolEnd;

    private void Awake()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.stateContext["PatrolStart"] = new int2(patrolStart.x, patrolStart.y);
            enemy.stateContext["PatrolEnd"] = new int2(patrolEnd.x, patrolEnd.y);
        }
    }
}
