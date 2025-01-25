using System.Collections.Generic;
using UnityEngine;

public class TroopSpawner : MonoBehaviour, ISelectable
{
    [SerializeField] TroopSpawnSettings troopSettings;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform spawnDestination;

    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] SpriteRenderer selectionSprite;
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
    void Awake()
    {
        if (lineRenderer)
            SetSpawnDestination(spawnDestination.position);
    }

    public void SetNewTroopPrefab(TroopSpawnSettings troopSettings)
    {
        this.troopSettings = troopSettings;
    }
    public void SetSpawnDestination(Vector3 position)
    {
        spawnDestination.transform.position = position;
        if (lineRenderer)
        {
            Vector3 destination = transform.InverseTransformPoint(position);
            destination = VectorUtility.FlattenVector(destination, 0.1f);
            lineRenderer.SetPosition(1, destination);
        }
    }

    public void OnDeselect()
    {
        SelectionManager.Instance.RemoveFromCurrentSelected(this);
        selectionSprite?.gameObject.SetActive(false);
       lineRenderer.enabled = false;
    }

    public void OnSelect()
    {
        SelectionManager.Instance.AddToCurrentSelected(this);
        selectionSprite?.gameObject.SetActive(true);
        lineRenderer.enabled = true;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public SelectionType GetSelectionType()
    {
        return SelectionType.Spawner;
    }

    public ISelectionStrategy GetSelectionStrategy()
    {
        return new SpawnerSelectionStrategy();

    }
}
internal class SpawnerSelectionStrategy : ISelectionStrategy
{
    public void ApplySelection(List<ISelectable> selectedItems)
    {
        UIManager.Instance.DisableAllSelectionMenus();
        List<TroopSpawner> spawners = new List<TroopSpawner>();
        foreach (ISelectable item in selectedItems)
        {
            if (item is TroopSpawner spawner)
                spawners.Add(spawner);

        }
        UIManager.Instance.spawnerMenu.SetSelectedSpawners(spawners);
        UIManager.Instance.spawnerMenu.gameObject.SetActive(true);
    }

    public void EnableButtons()
    {

    }
}