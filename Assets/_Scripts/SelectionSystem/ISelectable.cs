using UnityEngine;

public interface ISelectable
{
    public void OnSelect();
    public void OnDeselect();
    public SelectionType GetSelectionType();
    public ISelectionStrategy GetSelectionStrategy();
    public Vector3 GetPosition();
    // public bool IsSelected { get; }
}
