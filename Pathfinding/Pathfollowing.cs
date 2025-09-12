using System;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Pathfollowing : MonoBehaviour
{
    private NativeList<int2> m_Path;
    private int m_PathIndex = 0; // Index to keep track of the next step to follow.
    private Vector3 m_TargetPosition;
    private Vector3 m_MoveDirection;

    [Header("Movement stats")]
    [SerializeField] private float m_Speed;
    [SerializeField] private float m_JumpForce;
    [SerializeField] private float m_HorizontalJump = 0.5f;

    [Header("Jump management")]
    private Vector3 m_PreviousPosition; // Required for the step back performed before jumping.
    [SerializeField] private float m_JumpWait = 1.0f;
    [SerializeField] private float m_MaxCliffHeight = 10.0f;

    public bool isJumping = false;
    private bool m_JumpCoroutineExecution = false;

    [Header("Collision management")]
    private Rigidbody2D m_rb;
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private float m_GroundCheckRadius = 0.1f;

    // Jump in cliff parameters.
    [SerializeField] private Transform m_RightCliffCheck;
    [SerializeField] private float m_RightCliffCheckRadius = 0.1f;
    [SerializeField] private Transform m_LeftCliffCheck;
    [SerializeField] private float m_LeftCliffCheckRadius = 0.1f;
    private bool m_IsCliff = false;

    [Header("Stuck management")]
    // Manage the agent if it gets stuck.
    private float m_Timer = 0.0f;
    [SerializeField] private float m_StuckTime = 2.0f;
    private int m_LastPathIndex = 0;

    // Needed to compare int2 values.
    private Helper.Int2Comparer m_Comparer;

    // Debug
    [SerializeField] private LineRenderer lineRenderer;

    [Tooltip("Manage enemy facing, uncheck if it should be facing left at the beginning")]
    public bool isFacingRight = true;
    public bool isPathValid = true;

    // Store the jump coroutine reference.
    private Coroutine m_JumpCoroutine;

    [SerializeField] private bool showDebug = false;

    private void Awake()
    {
        m_Path = new NativeList<int2>(Allocator.Persistent);

        m_rb = GetComponent<Rigidbody2D>();

        m_Comparer = new Helper.Int2Comparer();

        lineRenderer = GetComponent<LineRenderer>();

        if (!isFacingRight)
        {
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;
        }
    }

    void Update()
    {
        if (showDebug)
        {
            DrawPath();
        }
    }

    private void FixedUpdate()
    {
        // Perform pathfollowing if the path is valid and has not finished.
        if (m_PathIndex >= 0 && !m_Path.IsEmpty)
        {
            // Get the world position of the node to go.
            m_TargetPosition = new Vector3(m_Path[m_PathIndex].x + 0.5f, m_Path[m_PathIndex].y + 0.5f, 0);
            m_MoveDirection = m_TargetPosition - transform.position;

            // Check if the next node is a cliff, needed to perform the jump.
            m_IsCliff = GridManager.Instance.grid.GetValue(Mathf.FloorToInt(m_Path[m_PathIndex].x), m_Path[m_PathIndex].y).IsCliff();

            // If agent is not jumping...
            if (!m_JumpCoroutineExecution)
            {
                // Check if it should jump if the target node is on higher ground.
                if (CheckJump(m_TargetPosition))
                {
                    isJumping = true;
                    m_JumpCoroutineExecution = true;

                    m_JumpCoroutine = StartCoroutine(Jump(m_JumpWait));
                }
                // Check if it should jump if the next node is a cliff.
                else if (!m_JumpCoroutineExecution
                    && m_IsCliff && CheckIsGrounded(m_GroundCheck, m_GroundCheckRadius) &&
                    !CheckHeight(m_TargetPosition) &&
                    (!CheckIsGrounded(m_RightCliffCheck, m_RightCliffCheckRadius) ||
                    !CheckIsGrounded(m_LeftCliffCheck, m_LeftCliffCheckRadius)))
                {
                    isJumping = true;
                    m_JumpCoroutineExecution = true;

                    m_JumpCoroutine = StartCoroutine(Jump(m_JumpWait, m_HorizontalJump)); // 0.5f in the forceX parameter to jump more horizontally.
                }
                // Move if it is not jumping.
                else if (!isJumping)
                {
                    MoveToX(m_MoveDirection);
                }
                // Check when it lands on the ground after jumping.
                else
                {
                    isJumping = !CheckIsGrounded(m_GroundCheck, m_GroundCheckRadius);
                }

                // Check if the agent is stuck in a node.
                if (m_PathIndex == m_LastPathIndex)
                {
                    m_Timer += Time.deltaTime;

                    // If the agent is stuck, cancel path and return to origin.
                    if (m_Timer >= m_StuckTime)
                    {
                        m_Timer = 0;

                        if (m_PathIndex + 1 < m_Path.Length)
                        {
                            SetPath(new int2(m_Path[m_PathIndex + 1].x, m_Path[m_PathIndex + 1].y), new int2(m_Path[m_Path.Length - 1].x, m_Path[m_Path.Length - 1].y));
                        }
                    }
                }
            }

            // Check if the agent has reached the current target node.
            if (Mathf.Abs(transform.position.x - m_TargetPosition.x) < 0.1f)
            {
                // Set the current position to the previous position.
                m_PreviousPosition = new Vector3(m_Path[m_PathIndex].x + 0.5f, m_Path[m_PathIndex].y + 0.5f, 0);

                // Go to the next node.
                m_PathIndex--;
                m_LastPathIndex = m_PathIndex;

                // Reset the timer to check if it gets stuck.
                m_Timer = 0;

                // If it is the last node of the path, stop.
                if (m_PathIndex == -1)
                {
                    m_rb.linearVelocityX = 0;

                    // Fix path recalculation in chase state.
                    isJumping = false;
                }
            }
        }
    }

    /// <summary>
    /// Set the path for the agent to follow.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetPath(int2 start, int2 end)
    {
        NativeList<int2> path = new NativeList<int2>(Allocator.TempJob);

        PathfindingManager.Instance.StartPathfinding(path, start, end);

        // Delete the starting position if it is the same as the previous path taken,
        // to avoid the agent restarting their path each time the target changes position.
        if (m_Path.Length != 0 && path.Length != 0 && m_Comparer.Equals(m_Path[m_Path.Length - 1], path[path.Length - 1]))
        {
            path.RemoveAt(path.Length - 1);
        }

        if (m_Path.IsCreated)
            m_Path.Clear();

        // Make sure the path is valid. If there is no path or
        // the final position is a cliff, invalid.
        if (path.Length != 0)
        {
            for (int i = 0; i < path.Length; i++)
            {
                m_Path.Add(path[i]);
            }

            // Ignoring the first node of the path (the one where from where the agent starts) makes
            // the pathfollowing work better.
            m_PathIndex = m_Path.Length - 2; 
            m_LastPathIndex = m_PathIndex;
            m_PreviousPosition = new Vector3(m_Path[m_Path.Length - 1].x + 0.5f, m_Path[m_Path.Length - 1].y + 0.5f, 0);

            isPathValid = true;
        }
        else
        {
            Debug.Log("Path is not valid.");
            isPathValid = false;
        }

        path.Dispose();
    }

    /// <summary>
    /// Move towards a given direction in the X axis.
    /// </summary>
    /// <param name="direction"></param>
    private void MoveToX(Vector3 direction)
    {
        m_rb.linearVelocityX = m_Speed * MathF.Sign(direction.x);

        // Check facing 
        CheckFacing(m_rb.linearVelocityX);
    }

    /// <summary>
    /// Check if the agent should jump, checking if it is grounded and if the 
    /// target node position is above the current one.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private bool CheckJump(Vector3 targetPosition)
    {
        return targetPosition.y > m_PreviousPosition.y + 0.1f && CheckIsGrounded(m_GroundCheck, m_GroundCheckRadius); // Add 0.1f to avoid jumping when the difference in y is too small. 
    }

    /// <summary>
    /// Check if the agent is touching the ground, using a child GameObject 
    /// to determine if it is colliding with the ground.
    /// </summary>
    /// <param name="check"> Transform of the GameObject that checks the ground. </param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private bool CheckIsGrounded(Transform check, float radius)
    {
        return Physics2D.OverlapCircle(check.position, radius, m_GroundLayer);
    }

    /// <summary>
    /// Check if there is enough height to jump.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private bool CheckHeight(Vector3 targetPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(targetPosition, Vector2.down, m_MaxCliffHeight, m_GroundLayer);

        Debug.DrawRay(targetPosition, Vector2.down * m_MaxCliffHeight, Color.red);

        return hit.collider != null;
    }

    /// <summary>
    /// Coroutine to allow delays for the jumping action, allowing to perform 
    /// a small step back before jumping, making it feel more realistic.
    /// It will first wait x time, then move slightly back to prepare for the jump,
    /// wait again, and finally jump.
    /// </summary>
    /// <param name="waitTime"> Time to wait between actions before jumping. </param>
    /// <param name="forceX"> Parameter to control the jump force in the X axis. </param>
    /// <returns></returns>
    private IEnumerator Jump(float waitTime, float forceX = 0.35f)
    {
        // If it is a cliff jump and the agent is not moving,
        // it needs to be just in the edge to perform the jump correctly,
        // so it will move forward a little bit.
        if (m_rb.linearVelocityX == 0 && m_IsCliff)
        {
            m_rb.linearVelocityX = m_Speed * MathF.Sign(m_MoveDirection.x);

            CheckFacing(m_rb.linearVelocityX);
        }

        // First wait, very short.
        yield return new WaitForSeconds(waitTime / 2);

        // Get the position from where it should perform the jump, one node
        // before the current one.
        Vector3 targetPosition = new Vector3(m_PreviousPosition.x, m_PreviousPosition.y, 0);
        Vector3 moveDirection = targetPosition - transform.position;

        CheckFacing(m_MoveDirection.x);

        // If it is not close enough, go to the step back position.
        if (Mathf.Abs(transform.position.x - m_PreviousPosition.x) > 0.01f) // 0.1 before
        {
            // Go to the opposite direction of the next node to do the step back.
            m_rb.linearVelocityX = m_Speed * MathF.Sign(-m_MoveDirection.x);

            // Wait before jumping, create the illusion of the agent repositioning
            // itself and preparing to jump.
            yield return new WaitForSeconds(waitTime);
        }

        // Make the agent be completely still.
        m_rb.linearVelocity = Vector2.zero;
        m_rb.angularVelocity = 0f;

        // Apply jump with an impulse, with the forceX multiplier if needed.
        m_rb.AddForce(new Vector2(Mathf.Sign(m_MoveDirection.x) * m_JumpForce * forceX, m_JumpForce), ForceMode2D.Impulse);

        // Coroutine finished.
        m_JumpCoroutine = null;
    }

    public void CheckFacing(float velocity)
    {
        if (isFacingRight && velocity < 0f) Flip();
        else if (!isFacingRight && velocity > 0f) Flip();
    }

    public void Flip()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        isFacingRight = !isFacingRight;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & m_GroundLayer) != 0 && m_JumpCoroutineExecution)
        {
            // Only reset if actually grounded
            if (m_JumpCoroutine == null)
            {
                m_JumpCoroutineExecution = false;
                isJumping = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (m_Path.IsCreated)
            m_Path.Dispose();
    }

    // Debug
    private void OnDrawGizmos()
    {
        if (m_GroundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_GroundCheck.position, m_GroundCheckRadius);
        }

        if (m_RightCliffCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_RightCliffCheck.position, m_RightCliffCheckRadius);
        }

        if (m_LeftCliffCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_LeftCliffCheck.position, m_LeftCliffCheckRadius);
        }
    }

    // Debug
    private void DrawPath()
    {
        if (m_Path.Length == 0 || lineRenderer == null)
            return;

        lineRenderer.positionCount = m_PathIndex + 1;

        for (int i = m_PathIndex; i >= 0; i--)
        {
            Vector3 pos = new Vector3(m_Path[i].x + 0.5f, m_Path[i].y + 0.5f, 0);
            lineRenderer.SetPosition(m_PathIndex - i, pos);
        }
    }

    // Helpers for the AI behaviours
    public bool IsPathFinished()
    {
        return m_PathIndex == -1 || m_Path.IsEmpty;
    }

    /// <summary>
    /// If the path is not valid, try to find the nearest node to the original target.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="maxRadius"></param>
    /// <param name="maxDistanceFromTarget"></param>
    /// <returns></returns>
    public int2 FindNearestWalkableTile(int2 origin, int maxRadius = 5, int maxDistanceFromTarget = 5)
    {
        Grid<GridNode> grid = GridManager.Instance.grid;
        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());

        NativeList<int2> candidates = new NativeList<int2>(Allocator.Temp);

        for (int dx = -maxRadius; dx <= maxRadius; dx++)
        {
            for (int dy = -maxRadius; dy <= maxRadius; dy++)
            {
                int2 checkPos = origin + new int2(dx, dy);

                // Direct grid bounds check instead of calling IsPositionInGrid
                if (checkPos.x < 0 || checkPos.y < 0 ||
                    checkPos.x >= gridSize.x || checkPos.y >= gridSize.y ||
                    (dx == 0 && dy == 0))
                {
                    continue;
                }

                if (grid.GetValue(checkPos.x, checkPos.y).IsWalkable())
                {
                    float distance = math.distance(origin, checkPos);

                    if (distance <= maxDistanceFromTarget)
                    {
                        candidates.Add(checkPos);
                    }
                }
            }
        }

        if (candidates.Length > 0)
        {
            int2 best = candidates[0];
            float bestDistance = math.distance(origin, best);

            for (int i = 1; i < candidates.Length; i++)
            {
                float d = math.distance(origin, candidates[i]);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = candidates[i];
                }
            }

            candidates.Dispose();
            return best;
        }

        candidates.Dispose();
        return origin; // fallback
    }

    /// <summary>
    /// Stops the current pathfinding and clears the path.
    /// </summary>
    public void FinishPath()
    {
        if (m_Path.IsCreated)
        {
            m_Path.Clear();
        }

        m_PathIndex = -1;
        isPathValid = true;

        // Reset jump state
        isJumping = false;
        m_JumpCoroutineExecution = false;

        // Clear debug line
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    public void CancelJump()
    {
        if (m_JumpCoroutine != null)
        {
            StopCoroutine(m_JumpCoroutine);
            m_JumpCoroutine = null;
        }

        isJumping = false;
        m_JumpCoroutineExecution = false;
        if (m_rb != null)
        {
            m_rb.linearVelocity = Vector2.zero;
            m_rb.angularVelocity = 0f;
        }
    }
}


