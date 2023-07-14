using System;
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
    public static Direction[] Neighbours = new[] { Direction.Top, Direction.Right, Direction.Bottom, Direction.Left };
    
    [SerializeField] ParamsContainer paramsContainer;
    [SerializeField] DungeonMultiplierOperation dungeonOperation;
    

    #region components
    
    GridItemPortals gridItemPortals;

    GridItemPortals GridItemPortals => gridItemPortals ? gridItemPortals : gridItemPortals = GetComponent<GridItemPortals>();

    GridItemAlpha gridItemAlpha;
    GridItemAlpha GridItemAlpha => gridItemAlpha ? gridItemAlpha : gridItemAlpha = GetComponent<GridItemAlpha>();

    LeanDragTranslateAlong LeanDragTranslateAlong { get; set; }

    public LeanSelectableByFinger LeanSelectableByFinger { get; private set; }


    #endregion
    
    public int X { get; set; }
    public int Y { get; set; }

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
            LeanDragTranslateAlong.enabled = isMovable;
        }
    }

    public ParamsContainer Params
    {
        get => paramsContainer;
        set => paramsContainer = value;
    }

    public DungeonMultiplierOperation DungeonOperation
    {
        get => dungeonOperation;
        set => dungeonOperation = value;
    }

    public bool IsRed { get; set; }
    
    int portalsCount;
    
    public int PortalsCount
    {
        get => portalsCount;
        set
        {
            portalsCount = value;
            UpdatePortals();
        }
    }

    public bool IsValid
    {
        get
        {
            if (IsRed && Neighbours.Select(GetNeighbour).Any(neighbour => neighbour != null && neighbour.IsRed))
                return false;
                
            int counter = 0;
            
            foreach (var neighbour in Neighbours)
            {
                var item = GetNeighbour(neighbour);

                if (item != null)
                    counter++;
            }

            return counter == PortalsCount;
        }
    }

    protected GridController Grid { get; set; }
    
    public void AttachItem(GridController grid, int x, int y)
    {
        IsOnGrid = true;
        Grid = grid;
        X = x;
        Y = y;
        
        GetComponent<LeanDragTranslateAlong>().ScreenDepth.Object = Grid.Plane;

        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * -0.2f, 0.4f, 0);

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
            GridItemAlpha.SetContentAlpha(0.3f);
    }

    public void ResetItem()
    {
        if(GridItemAlpha != null)
            GridItemAlpha.SetContentAlpha(1);
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
        drag.Damping = 15;

    }
#endif
    
    public void MarkAsNotValid()
    {
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.3f, 1, 5);
    }

    public void UpdatePortals()
    {
        if(GridItemPortals != null)
            GridItemPortals.UpdatePortals();
    }
}
