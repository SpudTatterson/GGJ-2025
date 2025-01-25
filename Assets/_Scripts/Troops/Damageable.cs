using System;
using System.Threading;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    // [SerializeField] float defenseMultiplier;
    public event Action OnDeath;
    float health;

    void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Damage Taken");
        health -= damage;
        if (health < 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject, 2); // if we have time make object pool

        if (TryGetComponent<BaseTroop>(out var troop))
        {
            foreach (var animator in troop.animators)
            {
                animator.SetTrigger("Death");
            }
        }
        if (TryGetComponent<ISelectable>(out var selectable))
        {
            selectable.OnDeselect();
            SelectionManager.Instance.UpdateSelection();
        }
        OnDeath?.Invoke();
    }
}