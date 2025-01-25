using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionManager : MonoSingleton<SelectionManager>
{
    public bool isSelecting = true;
    List<ISelectable> currentSelected = new List<ISelectable>();
    List<ISelectable> selectables = new List<ISelectable>();
    [SerializeField] float dragDelay = 0.1f;
    [SerializeField] LayerMask selectableLayers;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] RectTransform selectionBox;
    [SerializeField] CanvasScaler canvasScaler;
    [SerializeField] Material selectionMat;
    Vector3 mouseStartPos;
    Vector3 startWorldPos;
    Vector3 endWorldPos;
    GameObject selectionMesh;
    float mouseDownTime;
    [SerializeField] SelectionAction selectionAction;
    ISelectionStrategy selectionStrategy;
    enum SelectionAction
    {
        Default,
        Add,
        Remove,
    }

    bool setToUpdate;

    void Update()
    {
        if (!isSelecting) return;

        HandleSelectionAction();

        HandleDeselectionInput();

        HandleSelectionInput();

        if (setToUpdate)
        {
            if (currentSelected.Count != 0)
            {
                SetSelectionType(currentSelected[0].GetSelectionType());
                // UIManager.Instance.selectionPanel.SetActive(true);
            }
            else
                ResetSelection();

            setToUpdate = false;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Vector3 troopDestination = VectorUtility.ScreeToWorldPosition(Input.mousePosition, groundLayer);
            Debug.DrawLine(endWorldPos, endWorldPos + Vector3.up, Color.black, 20);
            SetTroopDestination(troopDestination);
        }
    }

    #region Selection Logic

    void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            StartSelection();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            EndSelection();
        }
        else if (Input.GetKey(KeyCode.Mouse0) && mouseStartPos != Vector3.zero && IsDragging())
        {
            DragSelection();
        }
    }

    void StartSelection()
    {
        mouseStartPos = Input.mousePosition;
        mouseDownTime = Time.unscaledTime;
    }

    void EndSelection()
    {
        startWorldPos = VectorUtility.ScreeToWorldPosition(mouseStartPos, groundLayer);
        Debug.DrawLine(startWorldPos, startWorldPos + Vector3.up, Color.black, 2);
        endWorldPos = VectorUtility.ScreeToWorldPosition(Input.mousePosition, groundLayer);
        Debug.DrawLine(endWorldPos, endWorldPos + Vector3.up, Color.black, 2);

        if (!IsDragging())
        {
            ClickSelection();
        }
        else if (IsDragging())
        {
            BoxSelection();
        }
        ResetDrag();
    }

    bool IsDragging()
    {
        return mouseDownTime + dragDelay < Time.unscaledTime;
    }

    void DragSelection()
    {

        startWorldPos = VectorUtility.ScreeToWorldPosition(mouseStartPos, groundLayer);
        endWorldPos = VectorUtility.ScreeToWorldPosition(Input.mousePosition, groundLayer);
        //update box positions and size

        // if (!selectionBox.gameObject.activeInHierarchy)
        //     selectionBox.gameObject.SetActive(true);
        // Vector2 mouseCurPos = Input.mousePosition;
        // float width = mouseCurPos.x - mouseStartPos.x;
        // float height = mouseCurPos.y - mouseStartPos.y;

        // selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height)) / canvasScaler.transform.localScale;
        // selectionBox.anchoredPosition =
        //     ((Vector2)mouseStartPos + new Vector2(width < 0 ? width : 0f, height < 0 ? height : 0f))
        //     / canvasScaler.transform.localScale;

        if (selectionMesh != null) Destroy(selectionMesh);
        selectionMesh = MeshUtility.CreatePlaneMesh(startWorldPos, endWorldPos, "SelectionPlane", selectionMat);
    }


    void ClickSelection()
    {
        List<ISelectable> selectables = new List<ISelectable>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.SphereCast(ray, 0.2f, out RaycastHit hit, Mathf.Infinity, selectableLayers) &&
        hit.transform.TryGetComponent(out ISelectable selectable))
        {
            selectables.Add(selectable);
        }
        Select(selectables);
    }

    void BoxSelection()
    {
        // List<ISelectable> selectables = new List<ISelectable>();

        // Bounds bounds = new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        // foreach (ISelectable selectable in this.selectables)
        // {
        //     Vector3 screenPos = Camera.main.WorldToScreenPoint(selectable.GetPosition());
        //     if (screenPos.x > bounds.min.x && screenPos.x < bounds.max.x
        //     && screenPos.y > bounds.min.y && screenPos.y < bounds.max.y)
        //     {
        //         selectables.Add(selectable);
        //     }
        // }
        // Select(selectables);


        Box box = VectorUtility.CalculateBoxSize(startWorldPos, endWorldPos);
        List<ISelectable> selectables = ComponentUtility.GetComponentsInBox<ISelectable>(box.center, box.halfExtents);
        ExtendedDebug.DrawBox(box.center, box.halfExtents * 2, Quaternion.identity, 4f);

        Select(selectables);

    }

    void ResetMouseData()
    {
        mouseStartPos = Vector3.zero;
        startWorldPos = Vector3.zero;
        endWorldPos = Vector3.zero;
        mouseDownTime = 0;
    }

    void HandleSelectionAction()
    {
        if (selectionAction == SelectionAction.Default || selectionAction == SelectionAction.Add || selectionAction == SelectionAction.Remove)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                selectionAction = SelectionAction.Add;
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                selectionAction = SelectionAction.Remove;
            else
                selectionAction = SelectionAction.Default;
        }
    }

    void HandleDeselectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetSelection();
        }
    }

    void Select(List<ISelectable> selectables)
    {
        if (selectables.Count == 0 && selectionAction == SelectionAction.Default)
        {
            ResetSelection();
            return;
        }

        if (selectionAction == SelectionAction.Add)
        {
            AddSelection(selectables);
        }
        else if (selectionAction == SelectionAction.Remove)
        {
            RemoveSelection(selectables);
        }
        else if (selectionAction == SelectionAction.Default)
        {
            DefaultSelection(selectables);
        }

    }

    private void AddSelection(List<ISelectable> selectables)
    {
        foreach (var selectable in selectables)
        {
            if (!currentSelected.Contains(selectable))
            {
                selectable.OnSelect();
            }
        }
        UpdateSelection();
    }

    void DefaultSelection(List<ISelectable> selectables)
    {
        DeselectAll();

        foreach (var selectable in selectables)
        {
            selectable.OnSelect();
        }

        UpdateSelection();
    }

    void RemoveSelection(List<ISelectable> selectables)
    {
        foreach (var selectable in selectables)
        {
            selectable.OnDeselect();
        }
    }

    // bool CheckForSpecialSelectionCase(ISelectable selectable, out SelectionType selectionType)
    // {
    //     foreach (SelectionType type in specialSelectionTypes)
    //     {
    //         if (selectable.GetSelectionType() == type)
    //         {
    //             selectionType = type;
    //             return true;
    //         }
    //     }
    //     selectionType = default;
    //     return false;
    // }

    #endregion

    #region Public methods

    public void AddToSelectables(ISelectable selectable)
    {
        if (!selectables.Contains(selectable))
            selectables.Add(selectable);
    }
    public void SetSelectionType(SelectionType selectionType)
    {
        if (currentSelected.Count > 1)
        {
            SetSelectionStrategy(new MultipleSelectionStrategy());
        }
        else
        {
            var selectable = currentSelected[0];
            SetSelectionStrategy(selectable.GetSelectionStrategy());
        }
        selectionStrategy.ApplySelection(currentSelected);
    }
    void SetSelectionStrategy(ISelectionStrategy selectionStrategy)
    {
        this.selectionStrategy?.CleanUp();
        this.selectionStrategy = selectionStrategy;
    }
    public void AddToCurrentSelected(ISelectable selectable)
    {
        currentSelected.Add(selectable);
    }
    public void RemoveFromCurrentSelected(ISelectable selectable)
    {
        currentSelected.Remove(selectable);
    }
    public void UpdateSelection()
    {
        setToUpdate = true;
    }
    public ISelectable GetMainSelectable()
    {
        if (currentSelected.Count >= 0)
            return currentSelected[0];
        else
            return null;
    }

    #endregion

    #region Button Actions


    public void SetTroopDestination(Vector3 destination)
    {
        foreach (var selectable in currentSelected)
        {
            if (selectable is BasePlayerTroop troop)
            {
                troop.SendToPosition(destination);
            }
            if(selectable is TroopSpawner troopSpawner)
            {
                troopSpawner.SetSpawnDestination(destination);
            }
        }
        // List<Cell> cells = new List<Cell>();
        // firstCell = GridManager.Instance.GetCellFromPosition(destination);
        // if (firstCell.IsFreeAndExists())
        // {
        //     cells.Add(firstCell);
        //     firstCell.inUse = true;
        // }
        // else
        //     cells.Add(firstCell.GetClosestEmptyCell());

        // for (int i = 0; i < currentSelected.Count; i++)
        // {
        //     ISelectable selectable = currentSelected[i];
        //     if (selectable is ColonistData colonist && colonist.brainState == EBrainState.Drafted)
        //     {
        //         if (i != 0)
        //         {
        //             Cell currentCell = firstCell.GetClosestEmptyCell();
        //             currentCell.inUse = true;
        //             cells.Add(currentCell);
        //         }

        //         colonist.SetDestination(cells[i].position);
        //     }
        // }
        // foreach (Cell cell in cells)
        // {
        //     cell.inUse = false;
        // }
    }


    // public void FocusCameraToSelected()
    // {
    //     List<Vector3> selectedPositions = new List<Vector3>();
    //     foreach (ISelectable selectable in currentSelected)
    //     {
    //         selectedPositions.Add((selectable as MonoBehaviour).transform.position);
    //     }

    //     Vector3 center = VectorUtility.CalculateCenter(selectedPositions);
    //     StartCoroutine(CameraController.Instance.SendCameraToTarget(center));
    // }

    #endregion

    #region Cleanup
    public void ResetSelection()
    {
        selectionAction = SelectionAction.Default;
        DeselectAll();
        ResetDrag();

        // UIManager.Instance.SetAllSelectionUIInactive();
        // UIManager.Instance.selectionPanel.SetActive(false);
        // UIManager.Instance.SelectionActionCanvas.SetActive(false);
    }

    void DeselectAll()
    {
        List<ISelectable> selectedCopy = new List<ISelectable>(currentSelected);

        foreach (ISelectable selectable in selectedCopy)
        {
            selectable.OnDeselect();
        }
        currentSelected.Clear();

        selectionStrategy?.CleanUp();
        UIManager.Instance.DisableAllSelectionMenus();
    }
    void DeselectAllAcceptType(SelectionType type)
    {
        List<ISelectable> selectedCopy = new List<ISelectable>(currentSelected);

        foreach (ISelectable selectable in selectedCopy)
        {
            if (selectable.GetSelectionType() != type)
                selectable.OnDeselect();
        }
    }

    void ResetDrag()
    {
        ResetMouseData();
        selectionBox.gameObject.SetActive(false);
        if (selectionMesh != null) Destroy(selectionMesh);
    }

    #endregion
}

class MultipleSelectionStrategy : ISelectionStrategy
{
    public void ApplySelection(List<ISelectable> selectedItems)
    {
    }

    public void EnableButtons()
    {

    }
}