using System.Collections.Generic;
using UnityEngine;

public class SpawnerMenu : MonoBehaviour
{
    List<TroopSpawner> selectedSpawners = new List<TroopSpawner>();

    public void SetSpawnerType(TroopSpawnSettings spawnSettings)
    {
        foreach(var spawner in selectedSpawners)
        {
            spawner.SetNewTroopPrefab(spawnSettings);
        }
    }

    public void SetSelectedSpawners(List<TroopSpawner> spawners)
    {
        selectedSpawners = spawners;
    }
}