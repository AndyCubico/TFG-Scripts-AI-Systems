public interface IDamagable
{
    void Die();

    float m_MaxHealth { get; set; }
    float currentHealth { get; set; }
}
