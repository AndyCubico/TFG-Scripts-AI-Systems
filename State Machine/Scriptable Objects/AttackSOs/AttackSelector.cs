using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackSelector", menuName = "Enemy Logic/Attack/Attack Selector")]
public class AttackSelector : AttackSOBase
{
    [SerializeField] private AttackSOBase[] attackOptions;

    [Tooltip("Probabilities work like this: 50% = 50")]
    [SerializeField] float[] m_AttackProbability;

    // Runtime, per-enemy cloned options.
    private AttackSOBase[] runtimeAttackOptions;

    // Per-enemy runtime state.
    private AttackSOBase chosenAttack;
    private AnimatorOverrideController overrideController;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);

        // Clone attack options per enemy.
        runtimeAttackOptions = new AttackSOBase[attackOptions.Length];
        for (int i = 0; i < attackOptions.Length; i++)
        {
            runtimeAttackOptions[i] = Instantiate(attackOptions[i]);
            runtimeAttackOptions[i].Initialize(gameObject, enemy);
        }
    }

    public override void DoEnter() => ChooseAttack();

    public override void DoUpdate() => chosenAttack?.DoUpdate();
    public override void DoFixedUpdate() => chosenAttack?.DoFixedUpdate();
    public override void DoExit() => chosenAttack?.DoExit();
    public override void ResetValues() => chosenAttack?.ResetValues();
    public override void OnParried() => chosenAttack?.OnParried();
    public override void FinishParry() => chosenAttack?.FinishParry();

    public override void PerformAttack()
    {
        if (runtimeAttackOptions != null && runtimeAttackOptions.Length > 0)
        {
            ChooseAttack();
        }
    }

    private void ChooseAttack()
    {
        enemy.StopDangerParticles();

        int newIndex;
        if (runtimeAttackOptions.Length <= 1)
        {
            newIndex = 0;
        }
        else
        {
            if (m_AttackProbability != null && m_AttackProbability.Length == runtimeAttackOptions.Length)
            {
                // Weighted random.
                newIndex = Choose(m_AttackProbability);
            }
            else
            {
                // Uniform random, avoid repeating previous attack.
                int previousIndex = -1;
                if (chosenAttack != null)
                {
                    for (int i = 0; i < runtimeAttackOptions.Length; i++)
                    {
                        if (runtimeAttackOptions[i] == chosenAttack)
                        {
                            previousIndex = i;
                            break;
                        }
                    }
                }

                do
                {
                    newIndex = Random.Range(0, runtimeAttackOptions.Length);
                } while (newIndex == previousIndex);
            }
        }

        chosenAttack = runtimeAttackOptions[newIndex];

        // Create override controller per enemy if not already done.
        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(enemy.animator.runtimeAnimatorController);
            enemy.animator.runtimeAnimatorController = overrideController;
        }

        // Get current overrides.
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        overrideController.GetOverrides(overrides);

        // Replace the default attack clip.
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key.name == "Attack_Default")
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, chosenAttack.attackClip);
            }
        }

        // Apply overrides.
        overrideController.ApplyOverrides(overrides);

        // Trigger attack.
        attackEnemyHit = chosenAttack.attackEnemyHit;
        attackEnemyHit.damage = chosenAttack.damage;
        chosenAttack.DoEnter();
    }

    // Function to determine which attack to choose. https://docs.unity3d.com/2019.3/Documentation/Manual/RandomNumbers.html
    private int Choose(float[] probs)
    {
        float total = 0;

        foreach (float elem in probs)
        {
            total += elem;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                randomPoint -= probs[i];
            }
        }
        return probs.Length - 1;
    }

    public void SetAttackProbability(float[] probabilities)
    {
        m_AttackProbability = probabilities;
    }
}
