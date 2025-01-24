using System.Collections.Generic;
using UnityEngine;

public class BasePlayerTroop : BaseTroop, ISelectable
{
    // public bool IsSelected => throw new System.NotImplementedException();
    [SerializeField] protected SpriteRenderer selectionSprite;
    [SerializeField] protected LineRenderer lineRenderer;

    protected virtual void Start()
    {
        SelectionManager.Instance.AddToSelectables(this);
    }
    protected virtual void Update()
    {
        if (agent.destination != null)
        {
            Vector3 destination = transform.InverseTransformPoint(agent.destination);
            destination = VectorUtility.FlattenVector(destination, 0.1f);
            lineRenderer.SetPosition(1, destination);
        }
    }


    public ISelectionStrategy GetSelectionStrategy()
    {
        return new TroopSelectionStrategy();
    }

    public SelectionType GetSelectionType()
    {
        return SelectionType.Troop;
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
}

public class TroopSelectionStrategy : ISelectionStrategy
{
    public void ApplySelection(List<ISelectable> selectedItems)
    {

    }

    public void EnableButtons()
    {

    }
}