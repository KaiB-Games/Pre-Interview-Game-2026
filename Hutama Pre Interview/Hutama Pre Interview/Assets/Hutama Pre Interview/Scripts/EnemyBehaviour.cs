using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public enum EnemyType { SmallBoi, FlyingBoi, ScratchyBoi, FireRainBoi, FinalBoss }

    [Header("Enemy Identity")]
    public string enemyName = "Monster";
    public EnemyType type;
    public int baseDamage = 15;

    [Header("Animation Setup")]
    [SerializeField] private Animator animator;
    [Tooltip("How long the attack animation clip lasts before returning to idle")]
    [SerializeField] private float attackAnimationDuration = 1.0f;
    [Tooltip("The time during the animation when the damage should hit (e.g., mid-swing)")]
    [SerializeField] private float damageImpactDelay = 0.5f;

    [Header("Final Boss Settings")]
    [Tooltip("If this is the final boss, it will cycle through or pick randomly from these attack types")]
    [SerializeField] private List<EnemyType> bossAttackPool = new List<EnemyType> { EnemyType.SmallBoi, EnemyType.FlyingBoi, EnemyType.ScratchyBoi, EnemyType.FireRainBoi };
    private int currentBossAttackIndex = 0;

    [Header("Fire Rain Visual VFX Setup")]
    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private float projectileFallSpeed = 8f;
    [SerializeField] private int totalMeteorsToDrop = 5;

    public IEnumerator ExecuteAttackSequence(CombatManager manager, CombatManager.CharacterStats target)
    {
        if (target.currentHP <= 0) yield break;

        int calculatedDamage = baseDamage;
        if (target.isDefending) calculatedDamage /= 2;

        if (animator == null) animator = GetComponent<Animator>();

        if (type == EnemyType.FinalBoss)
        {
            yield return StartCoroutine(ExecuteBossTurn(manager, target, calculatedDamage));
        }
        else
        {
            yield return StartCoroutine(ExecuteSingleAttack(type, manager, target, calculatedDamage));
        }
    }

    private IEnumerator ExecuteBossTurn(CombatManager manager, CombatManager.CharacterStats target, int damage)
    {
        if (bossAttackPool == null || bossAttackPool.Count == 0)
        {
            Debug.LogError($"{enemyName} has no attacks assigned in its pool!");
            yield break;
        }

        EnemyType chosenAttack = bossAttackPool[currentBossAttackIndex];

        yield return StartCoroutine(ExecuteSingleAttack(chosenAttack, manager, target, damage));

        currentBossAttackIndex = (currentBossAttackIndex + 1) % bossAttackPool.Count;
    }

    private IEnumerator ExecuteSingleAttack(EnemyType attackStyle, CombatManager manager, CombatManager.CharacterStats target, int damage)
    {
        string triggerName = "";

        switch (attackStyle)
        {
            case EnemyType.SmallBoi:
                Debug.Log($"{enemyName} uses Special Attack: BASH!");
                triggerName = "AttackBash";
                break;

            case EnemyType.FlyingBoi:
                Debug.Log($"{enemyName} uses Special Attack: EYE BEAM!");
                triggerName = "AttackEyeBeam";
                break;

            case EnemyType.ScratchyBoi:
                Debug.Log($"{enemyName} uses Special Attack: SCRATCH!");
                triggerName = "AttackScratch";
                break;

            case EnemyType.FireRainBoi:
                Debug.Log($"{enemyName} summons a torrential FIRE RAIN!");
                triggerName = "AttackFireRain";
                if (fireProjectilePrefab != null)
                {
                    StartCoroutine(DropFireRainVisuals(target));
                }
                break;

        }

        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }

        yield return new WaitForSeconds(damageImpactDelay);
        ApplyDamageToPlayer(target, damage, manager);

        float remainingTime = Mathf.Max(0, attackAnimationDuration - damageImpactDelay);
        yield return new WaitForSeconds(remainingTime);
    }

    private void ApplyDamageToPlayer(CombatManager.CharacterStats target, int damage, CombatManager manager)
    {
        target.currentHP = Mathf.Max(0, target.currentHP - damage);
        Debug.Log($"[COMBAT] {enemyName} dealt {damage} DMG to {target.name}!");
        manager.UpdateStatusUI();
    }

    private IEnumerator DropFireRainVisuals(CombatManager.CharacterStats target)
    {
        Vector3 enemyPosition = transform.position;

        Vector3 playerPos = GameObject.Find("Player").transform.position;

        for (int i = 0; i < totalMeteorsToDrop; i++)
        {
            float randomXOffset = Random.Range(-0.5f, 1.5f);
            float randomYOffset = Random.Range(3.5f, 5.0f);
            Vector3 spawnPosition = enemyPosition + new Vector3(randomXOffset, randomYOffset, 0);

            GameObject fireInstance = Instantiate(fireProjectilePrefab, spawnPosition, Quaternion.identity, null);

            FireProjectile projectileScript = fireInstance.GetComponent<FireProjectile>();
            if (projectileScript != null)
            {
                projectileScript.InitializeDrop(playerPos, projectileFallSpeed);
            }

            yield return new WaitForSeconds(0.15f);
        }
    }
}