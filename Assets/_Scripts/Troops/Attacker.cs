using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

using Random = UnityEngine.Random;
public class Attacker : MonoBehaviour
{
    // [SerializeField] AttackerType attackerType;

    [SerializeField] protected Vector2 damageRange = new Vector2(8, 12);
    [SerializeField,  Tooltip("Time between attacks")] protected Vector2 attackTimeRange = new Vector2(0.1f, 0.3f);
    [SerializeField] protected float attackRange = 2;
    [SerializeField] protected float lockToTargetRange = 5;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected BaseTroop troop;

    [SerializeField, ReadOnly] Damageable target;

    protected float timeSinceLastAttack;
    protected float attackTime = 0.1f;

    protected virtual void Awake()
    {
        troop = GetComponent<BaseTroop>();
    }
    protected virtual void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        List<Damageable> damageables = ComponentUtility.GetComponentsInRadius<Damageable>(transform.position, lockToTargetRange, enemyLayer);
        if (damageables.Count == 0) return;
        SelectTarget(damageables);
    }

    protected virtual void SelectTarget(List<Damageable> damageables)
    {
        target = damageables.OrderByDescending(x => Vector3.Distance(transform.position, x.transform.position)).Last();
        if (target != null) AttackTarget();
    }

    protected virtual void AttackTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget <= attackRange)
        {
            // Check if it's time to attack
            if (timeSinceLastAttack > attackTime)
            {

                target.TakeDamage(Random.Range(damageRange.x, damageRange.y));
                timeSinceLastAttack = 0;
                attackTime = Random.Range(attackTimeRange.x, attackTimeRange.y);
            }
        }
        else
        {
            // If the target is outside attack range, move towards it
            troop.SendToPosition(target.transform.position);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockToTargetRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

// public enum AttackerType
// {
//     Friendly,
//     Enemy,
// }