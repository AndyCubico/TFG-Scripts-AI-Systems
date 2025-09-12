using UnityEngine;

public class AttackDistanceCheck : MonoBehaviour
{
    private Enemy m_Enemy;
    private BoxCollider2D m_BoxCollider;

    private void Awake()
    {
        m_Enemy = GetComponentInParent<Enemy>();   
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            m_Enemy.SetWithinAttackRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            m_Enemy.SetWithinAttackRange(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_BoxCollider == null)
            m_BoxCollider = GetComponent<BoxCollider2D>();

        if (m_BoxCollider == null)
            return;

        Gizmos.color = Color.red;

        Vector2 worldCenter = (Vector2)transform.position + (Vector2)(transform.rotation * Vector3.Scale(m_BoxCollider.offset, transform.lossyScale));
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, m_BoxCollider.size);

        Gizmos.matrix = oldMatrix;
    }
}
