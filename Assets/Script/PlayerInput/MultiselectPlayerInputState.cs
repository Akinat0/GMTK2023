using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Common;
using UnityEngine;

class MultiselectPlayerInputState : PlayerInputState
{
    public MultiselectPlayerInputState(PlayerInput playerInput) : base(playerInput)
    {
    }

    readonly List<GridItem> selectableItems = new List<GridItem>();
    GridItem selected; 

    public override void Enable()
    {
        selected = null;
        PlayerInput.SelectByFinger.OnNothing.AddListener(HandleOnNothing);

        foreach (GridItem item in PlayerInput.SelectByFinger.Selectables.Select(selectable => selectable.GetComponent<GridItem>()))
            selectableItems.Add(item);
        
        PlayerInput.SelectByFinger.OnSelected.AddListener(HandleSelected);
        PlayerInput.SelectByFinger.OnDeselected.AddListener(HandleDeselected);

        PlayerInput.Grid.SetMovableAll(true);
        
        GameScene.OnStartRun += HandleStartRun;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            SetState<BoxSelectionPlayerInputState>();
    }

    public override void Disable()
    {
        selected = null;
        selectableItems.Clear();
        
        PlayerInput.SelectByFinger.OnSelected.RemoveListener(HandleSelected);
        PlayerInput.SelectByFinger.OnDeselected.RemoveListener(HandleDeselected);
        
        PlayerInput.SelectByFinger.OnNothing.RemoveListener(HandleOnNothing);
        
        GameScene.OnStartRun -= HandleStartRun;
    }

    void HandleStartRun()
    {
        SetState<SpectatePlayerInputState>();
    }
    
    void HandleSelected(LeanSelectable leanSelected)
    {
        PlayerInput.SelectByFinger.DeselectWithFingers = true;
        selected = leanSelected.GetComponent<GridItem>();
        
        PlayerInput.Grid.SetMovableAll(false);
        PlayerInput.Grid.SetMovable(selected.X, selected.Y, true);

        foreach (var selectable in selectableItems)
        {
            PlayerInput.Grid.DetachItem(selectable.X, selectable.Y);
            
            if (selectable != selected)
            {
                selectable.transform.DOComplete();
                selectable.transform.parent = selected.transform;
                selectable.GridItemPortals.IsForceOutlineEnabled = true;
            }

        }
    }

    void HandleDeselected(LeanSelectable leanSelected)
    {
        GridItem deselectedItem = leanSelected.GetComponent<GridItem>();
        
        if(deselectedItem == null || deselectedItem != selected)
            return;

        deselectedItem.transform.DOComplete();
        
        bool isValid = selectableItems.All(
            item => PlayerInput.Grid.TryGetCellCoordinates(item.transform.position, out int x, out int y) 
                    && !PlayerInput.Grid.IsBusyCell(x, y));

        if (isValid)
        {
            foreach (GridItem selectableItem in selectableItems)
            {
                selectableItem.transform.parent = null;
                
                if (!PlayerInput.Grid.TryGetCellCoordinates(selectableItem.transform.position, out int x, out int y)
                    || !PlayerInput.Grid.TryPlaceItem(selectableItem, x, y)) // if can't place on new location
                {
                    PlayerInput.Grid.TryPlaceItem(selectableItem, selectableItem.X, selectableItem.Y); //bring back to old one
                }
            }
        }
        else
        {
            foreach (GridItem selectableItem in selectableItems)
            {
                selectableItem.transform.parent = null;
                PlayerInput.Grid.TryPlaceItem(selectableItem, selectableItem.X, selectableItem.Y);
            }
        }

        foreach (GridItem selectable in selectableItems)
            selectable.GridItemPortals.IsForceOutlineEnabled = false;
        
        
        SetState<CommonPlayerInputState>();
    }
    
    void HandleOnNothing()
    {
        SetState<CommonPlayerInputState>();
    }
}