using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackerAOE : Attacker
{
    [SerializeField] float aoeRange = 3;
    List<Damageable> targets = new List<Damageable>();
    protected override void SelectTarget(List<Damageable> damageables)
    {
        targets = damageables;
        if(targets.Count > 0) AttackTarget();
     }

    protected override void AttackTarget()
    {
        Damageable closest = targets.OrderByDescending(x => Vector3.Distance(transform.position, x.transform.position)).Last();
        float distanceToTarget = Vector3.Distance(transform.position, closest.transform.position);

        if (distanceToTarget <= attackRange)
        {
            // Check if it's time to attack
            if (timeSinceLastAttack > attackTime)
            {
                Debug.Log("AOE");
                List<Damageable> damageables = ComponentUtility.GetComponentsInRadius<Damageable>(transform.position, aoeRange, enemyLayer);

                foreach (var target in damageables)
                    target.TakeDamage(Random.Range(damageRange.x, damageRange.y));
                timeSinceLastAttack = 0;
                attackTime = Random.Range(attackTimeRange.x, attackTimeRange.y);
            }
        }
        else
        {
            // If the target is outside attack range, move towards it
            troop.SendToPosition(closest.transform.position);
        }
    }
}
