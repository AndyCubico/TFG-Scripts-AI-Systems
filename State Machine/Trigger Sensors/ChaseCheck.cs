using UnityEngine;

public class ChaseCheck : MonoBehaviour
{
    private Enemy m_EnemyCS;
    private BoxCollider2D m_BoxCollider;
    public bool isBack = false;

    private void Awake()
    {
        m_EnemyCS = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            m_EnemyCS.SetInSensor(true);

            if (isBack)
            {
                m_EnemyCS.Flip();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (isBack && m_EnemyCS.isInSensor)
            {
                m_EnemyCS.Flip();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            m_EnemyCS.SetInSensor(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_BoxCollider == null)
            m_BoxCollider = GetComponent<BoxCollider2D>();

        if (m_BoxCollider == null)
            return;

        Gizmos.color = Color.yellow;

        Vector2 worldCenter = (Vector2)transform.position + (Vector2)(transform.rotation * Vector3.Scale(m_BoxCollider.offset, transform.lossyScale));
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, m_BoxCollider.size);

        Gizmos.matrix = oldMatrix;
    }
}
