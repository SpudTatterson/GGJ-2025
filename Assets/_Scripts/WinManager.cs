using System;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    [SerializeField] List<Damageable> enemySpawners;
    [SerializeField] GameObject winScreen;
    int spawnersLeft;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnersLeft = enemySpawners.Count;
        foreach (var enemy in enemySpawners)
        {
            enemy.OnDeath += SpawnerDied;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnerDied()
    {
        spawnersLeft--;
        if(spawnersLeft <= 0)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        winScreen.SetActive(true);
        Time.timeScale = 0.1f;
    }
}
