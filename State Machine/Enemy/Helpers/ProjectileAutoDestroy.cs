using UnityEngine;

public class ProjectileAutoDestroy : MonoBehaviour
{
    public float lifetime = 2f;
    public LayerMask m_GroundLayer;

    void Start()
    {
        Destroy(gameObject, lifetime);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & m_GroundLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}