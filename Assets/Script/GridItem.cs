using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Touch;
using UnityEngine;

public enum Direction
{
    None = -1,
    Top = 0,
    Right = 1, 
    Bottom = 2, 
    Left = 3
}

[DisallowMultipleComponent]
[RequireComponent(typeof(LeanDragTranslateAlong), typeof(LeanSelectableByFinger))]
public class GridItem : MonoBehaviour
{
    public static readonly IReadOnlyCollection<Direction> Neighbours = new[] { Direction.Top, Direction.Right, Direction.Bottom, Direction.Left };
    
    ParamsContainer paramsContainer;
    DungeonMultiplierOperation dungeonOperation;
    
    #region components
    
    GridItemPortals gridItemPortals;

    public GridItemPortals GridItemPortals => gridItemPortals ? gridItemPortals : gridItemPortals = GetComponent<GridItemPortals>();

    GridItemAlpha gridItemAlpha;
    GridItemAlpha GridItemAlpha => gridItemAlpha ? gridItemAlpha : gridItemAlpha = GetComponent<GridItemAlpha>();
    
    GridItemView gridItemView;
    GridItemView GridItemView => gridItemView ? gridItemView : gridItemView = GetComponent<GridItemView>();

    LeanDragTranslateAlong LeanDragTranslateAlong { get; set; }

    LeanSelectableByFinger LeanSelectableByFinger { get; set; }

    #endregion
    
    public int X { get; set; }
    public int Y { get; set; }

    public int NeighboursCount => Neighbours.Select(GetNeighbour).Count(neighbour => neighbour != null);
    
    public bool IsOnGrid { get; private set; }

    bool isMovable;
    public bool IsMovable
    {
        get => isMovable;
        set
        {
            if(isMovable == value)
                return;

            isMovable = value;
            UpdateDragTranslate();
        }
    }

    public ParamsContainer Params
    {
        get => paramsContainer;
        set
        {
            paramsContainer = value;
            GridItemView.Initialize(this);
        }
    }

    public DungeonMultiplierOperation DungeonOperation
    {
        get => dungeonOperation;
        set => dungeonOperation = value;
    }

    public bool IsRed { get; set; }

    public bool IsFireplace { get; set; }

    int portalsCount;
    
    public int PortalsCount
    {
        get => portalsCount;
        set
        {
            if(portalsCount == value)
                return;
            
            portalsCount = value;
            UpdatePortals();
        }
    }

    bool isLockedOnGrid;
    public bool IsLockedOnGrid
    {
        get => isLockedOnGrid;
        set
        {
            if(isLockedOnGrid == value)
                return;

            isLockedOnGrid = value;
            
            UpdateDragTranslate();
            UpdatePortals();
            // LeanSelectableByFinger.enabled = !isLockedOnGrid;
        }
    }
    

    public bool IsValid
    {
        get
        {
            if (IsRed && Neighbours.Select(GetNeighbour).Any(neighbour => neighbour != null && neighbour.IsRed))
                return false;
                
            int count = NeighboursCount;

            //special case
            if (PortalsCount == 2)
                return count is 1 or 2;
            
            return PortalsCount == count;
        }
    }

    protected GridController Grid { get; set; }

    public void Initialize(ParamsContainer paramsContainer, DungeonMultiplierOperation operation, bool isRed, bool isFireplace, int portalsCount)
    {
        Params = paramsContainer;
        DungeonOperation = operation;
        IsRed = false;
        IsFireplace = isFireplace;
        PortalsCount = portalsCount;
        
        GridItemView.Initialize(this);
    }
    
    public void AttachItem(GridController grid, int x, int y)
    {
        IsOnGrid = true;
        Grid = grid;
        X = x;
        Y = y;
        
        GetComponent<LeanDragTranslateAlong>().ScreenDepth.Object = Grid.Plane;

        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * -0.2f, 0.4f, 0);

        PortalsCount = NeighboursCount;

        SoundManager.PlaySound("placed");
    }

    public void DetachItem()
    {
        if (!IsOnGrid)
            return;

        IsOnGrid = false;
        SoundManager.PlaySound("pick");
        transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 0);
    }
    
    public GridItem GetNeighbour(Direction direction)
    {
        if (Grid == null)
            return null;
        
        return direction switch
        {
            Direction.Right => Grid.GetCellItem(X + 1, Y),
            Direction.Left => Grid.GetCellItem(X - 1, Y),
            Direction.Top => Grid.GetCellItem(X, Y - 1),
            Direction.Bottom => Grid.GetCellItem(X, Y + 1),
            _ => null
        };
    }


    void Awake()
    {
        LeanDragTranslateAlong = GetComponent<LeanDragTranslateAlong>();
        LeanSelectableByFinger = GetComponent<LeanSelectableByFinger>();

        gameObject.AddComponent<GridSelectableDragSound>();

        IsMovable = true;
    }

    void OnEnable()
    {
        LeanSelectableByFinger.OnSelectedFinger.AddListener(OnSelectedFingerHandler);
        LeanSelectableByFinger.OnSelectedFingerUp.AddListener(OnSelectedFingerUpHandler);
    }
    
    void OnDisable()
    {
        LeanSelectableByFinger.OnSelectedFinger.RemoveListener(OnSelectedFingerHandler);
        LeanSelectableByFinger.OnSelectedFingerUp.RemoveListener(OnSelectedFingerUpHandler);
    }
    
    void OnSelectedFingerHandler(LeanFinger finger)
    {
        if (IsLockedOnGrid && GameScene.UnlockableCount > 0 && this != GameScene.Character.ActiveRoom)
        {
            GameScene.UnlockableCount--;
            IsLockedOnGrid = false;
        }
        
        if(Grid != null)
            Grid.InvokeOnSelectFingerDown(this);
    }
    
    void OnSelectedFingerUpHandler(LeanFinger finger)
    {
        if(Grid != null)
            Grid.InvokeOnSelectFingerUp(this);
    }

    public void DisableItemContent()
    {
        if(GridItemAlpha != null)
            GridItemAlpha.SetAlpha(0.3f);
    }

    public void ResetItem()
    {
        if(GridItemAlpha != null)
            GridItemAlpha.SetAlpha(1);
    }
    
    public virtual void Destroy()
    {
        Grid.DetachItem(X, Y);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    
    void OnValidate()
    {
        LeanDragTranslateAlong drag = GetComponent<LeanDragTranslateAlong>();
        drag.ScreenDepth.Conversion = LeanScreenDepth.ConversionType.PlaneIntercept;
        drag.Damping = 20;
    }
    
#endif
    
    public void MarkAsNotValid()
    {
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.3f, 1, 5);
    }

    public void UpdatePortals()
    {
        PortalsCount = NeighboursCount;
        
        if(GridItemPortals != null)
            GridItemPortals.UpdatePortals();
    }

    void UpdateDragTranslate()
    {
        LeanDragTranslateAlong.enabled = !isLockedOnGrid && isMovable;
    }
}
