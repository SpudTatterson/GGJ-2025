using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class AIManager : MonoBehaviour
{
    [SerializeField] List<TroopSpawnSettings> availableTroops = new List<TroopSpawnSettings>();
    [SerializeField] List<TroopSpawner> spawners = new List<TroopSpawner>();
    [SerializeField] Vector2 changeSpawnerTypeTimeRange = new Vector2(30, 50);

    [SerializeField] Sector initialSector;

    float timeSinceTypeChange;
    float currentTimeToChangeType = 1;

    Sector sectorToCapture;

    void Awake()
    {
        sectorToCapture = initialSector;

        SetDestinationsToSector();

        sectorToCapture.OnBubbleCapture += GoToNextSector;
        // sectorToCapture.OnGermCapture += 
    }

    private void SetDestinationsToSector()
    {
        foreach (var spawner in spawners)
        {
            Vector3 destination = new Vector3(sectorToCapture.transform.position.x, sectorToCapture.transform.position.y, spawner.transform.position.z);
            spawner.SetSpawnDestination(destination);
        }
    }

    private void GoToNextSector()
    {
        sectorToCapture.OnBubbleCapture -= GoToNextSector;
        sectorToCapture = sectorToCapture.GetNextSector(false);

        sectorToCapture.OnBubbleCapture += GoToNextSector;
        SetDestinationsToSector();

    }

    void Update()
    {
        ControlSpawnerTypes();

    }

    private void ControlSpawnerTypes()
    {
        timeSinceTypeChange += Time.deltaTime;
        if (timeSinceTypeChange >= currentTimeToChangeType)
        {
            int randomSpawnerIndex = Random.Range(0, spawners.Count);
            TroopSpawnSettings randomTroop = GetTroop();
            Debug.Log(randomTroop.prefab.gameObject);
            spawners[randomSpawnerIndex].SetNewTroopPrefab(randomTroop);
            currentTimeToChangeType = Random.Range(changeSpawnerTypeTimeRange.x, changeSpawnerTypeTimeRange.y);
            timeSinceTypeChange = 0;
        }
    }

    private TroopSpawnSettings GetTroop()
    {
        float rnd = Random.Range(0f, 1f);
        Debug.Log(rnd);
        if (rnd <= 0.3)
        {
            return availableTroops[0];
        }
        else if (rnd <= 0.6)
        {
            return availableTroops[1];
        }
        else
        {
            return availableTroops[2];
        }
    }
}
