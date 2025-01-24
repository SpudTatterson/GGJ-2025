using System;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    // [SerializeField] float defenseMultiplier;

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
        Destroy(gameObject); // if we have time make object pool
    }
}