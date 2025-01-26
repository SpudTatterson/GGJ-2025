using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Conflicted : MonoSingleton<SelectionManager>
{
    public bool isSelecting = true;
    List<ISelectable> currentSelected = new List<ISelectable>();
    List<ISelectable> Selectables = new List<ISelectable>();
    [SerializeField] float dragDelay = 0.1f;
    [SerializeField] LayerMask selectableLayers;
    [SerializeField] RectTransform selectionBox;
    [SerializeField] CanvasScaler canvasScaler;
    Vector3 mouseStartPos;
    Vector3 mouseEndPos;
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
    }

    #region Selection Logic

    void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            StartSelection();
            Debug.Log("Started Dragging");
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Debug.Log("Finished Dragging");
            EndSelection();
        }
        else if (Input.GetKey(KeyCode.Mouse0) && mouseStartPos != Vector3.zero && IsDragging())
        {
            Debug.Log("Dragging");
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
        //update box positions and size

        if (!selectionBox.gameObject.activeInHierarchy)
            selectionBox.gameObject.SetActive(true);
        Vector2 mouseCurPos = Input.mousePosition;
        float width = mouseCurPos.x - mouseStartPos.x;
        float height = mouseCurPos.y - mouseStartPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height)) / canvasScaler.transform.localScale;
        selectionBox.anchoredPosition =
            ((Vector2)mouseStartPos + new Vector2(width < 0 ? width : 0f, height < 0 ? height : 0f))
            / canvasScaler.transform.localScale;

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
        List<ISelectable> selectables = new List<ISelectable>();

        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        foreach (ISelectable selectable in selectables)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint((selectable as Transform).position);
            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
            Debug.Log("inside");
                selectables.Add(selectable);
            }
            Debug.Log("outside");
        }
        Select(selectables);
    }

    void ResetMouseData()
    {
        mouseStartPos = Vector3.zero;
        mouseEndPos = Vector3.zero;
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
        if (!Selectables.Contains(selectable))
            Selectables.Add(selectable);
    }
    public void SetSelectionType(SelectionType selectionType)
    {
        if (currentSelected.Count > 1)
        {
            // SetSelectionStrategy(new MultipleSelectionStrategy());
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
    }

    #endregion
}
