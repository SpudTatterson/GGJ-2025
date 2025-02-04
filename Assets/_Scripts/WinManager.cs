using System;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    [SerializeField] List<Damageable> enemySpawners;
    [SerializeField] List<Damageable> playerSpawners;
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject loseScreen;

    [SerializeField] bool isPaused;

    private int spawnersLeft;
    private int playersSpawnersLeft;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnersLeft = enemySpawners.Count;
        Debug.Log(spawnersLeft);
        foreach (var enemy in enemySpawners)
        {
            enemy.OnDeath += SpawnerDied;
            

        }

        playersSpawnersLeft = playerSpawners.Count;
        Debug.Log(playersSpawnersLeft);
        foreach (var player in playerSpawners)
        {
            player.OnDeath += PlayerSpawnerDied;
        }
    }

    // Update is called once per frame
    void Update()
    {
       PauseAndResume();
       
       //god tools - Ron
       if (Input.GetKeyDown(KeyCode.H))
       {
           SpawnerDied();
           Debug.Log(spawnersLeft);
       }
       if (Input.GetKeyDown(KeyCode.J))
       {
           PlayerSpawnerDied();
           Debug.Log(playersSpawnersLeft);
       }
    }

    private void SpawnerDied()
    {
        spawnersLeft--;
        if(spawnersLeft <= 0)
        {
            TriggerWin();
        }
    }

    private void PlayerSpawnerDied()
    {
        playersSpawnersLeft--;
        if (playersSpawnersLeft <= 0)
        {
            TriggerLose();
        }
    }
    private void TriggerWin()
    {
        winScreen.SetActive(true);
        Time.timeScale = 0.1f;
    }

    private void TriggerLose()
    {
        loseScreen.SetActive(true);
        Time.timeScale = 0.1f;
    }

    public void PauseScreen()
    {
        if (isPaused)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0.0f;
        }
        else if (isPaused == false)
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1.0f;
        }
   
    }

    public void PauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseSwitch();
        }
    }

    public void PauseSwitch()
    {
        isPaused = !isPaused;
        PauseScreen();
    }
}
