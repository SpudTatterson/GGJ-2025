using UnityEngine;

public class TroopSpawner : MonoBehaviour
{
    [SerializeField] TroopSpawnSettings troopSettings;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform spawnDestination;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (troopSettings == null) return;
        if (timer > troopSettings.spawnTime)
        {
            timer = 0;
            BaseTroop troop = Instantiate(troopSettings.prefab, spawnPoint.position, Quaternion.identity).GetComponent<BaseTroop>();
            troop.SendToPosition(spawnDestination.position);
        }
    }

    public void SetNewTroopPrefab(TroopSpawnSettings troopSettings)
    {
        this.troopSettings = troopSettings;
    }
    public void SetSpawnDestination(Vector3 position)
    {
        spawnDestination.transform.position = position;
    }
}
