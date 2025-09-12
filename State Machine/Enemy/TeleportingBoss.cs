using System.Collections.Generic;
using UnityEngine;

public class TeleportingBoss : StaggerableEnemy
{
    [Header("Teleport Settings")]
    [SerializeField] private Vector2[] m_TeleportPositions;
    private int lastTeleportIndex = -1;

    [SerializeField] private ParticleSystem m_TeleportParticles;

    [System.Serializable]
    public class AttackProbabilitySet
    {
        public float[] probabilities;
    }

    // These probabilities correspond to the attack options in RandomAttackCombo, when
    // setting them up, remember how the attackOptions in the scriptable object are ordered.
    [Header("List of the attack probabilities depending on the position")]
    [Tooltip("Probabilities as follows: 50% = 50. Position in list = position in teleport positions")]
    [SerializeField] private List<AttackProbabilitySet> m_AttackProbabilitiesByTeleport;

    public override void PerformWaitAttack()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(waitAttackStateName))
        {
            if (!isInWaitAttack)
            {
                isInWaitAttack = true;
                TeleportToRandomPosition();
            }
        }
        else if (isInWaitAttack)
        {
            isInWaitAttack = false;
        }
    }

    private void TeleportToRandomPosition()
    {
        if (m_TeleportPositions == null || m_TeleportPositions.Length == 0)
            return;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, m_TeleportPositions.Length);
        } while (m_TeleportPositions.Length > 1 && newIndex == lastTeleportIndex);

        transform.position = m_TeleportPositions[newIndex];
        lastTeleportIndex = newIndex;

        // Cast attackSOBaseInstance to RandomAttackCombo and set attack probability.
        if (attackSOBaseInstance is AttackSelector randomAttackCombo)
        {
            if (m_AttackProbabilitiesByTeleport != null && newIndex < m_AttackProbabilitiesByTeleport.Count)
            {
                randomAttackCombo.SetAttackProbability(m_AttackProbabilitiesByTeleport[newIndex].probabilities);
            }
        }
    }

    public void PlayTeleportParticles()
    {
        m_TeleportParticles.Play();
    }

    public void StopTeleportParticles()
    {
        if (m_TeleportParticles != null)
        {
            m_TeleportParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}


