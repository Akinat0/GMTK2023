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

    GridItemPortals gridItemPortals;

    GridItemPortals GridItemPortals => gridItemPortals ? gridItemPortals : gridItemPortals = GetComponent<GridItemPortals>();

    GridItemAlpha gridItemAlpha;
    GridItemAlpha GridItemAlpha => gridItemAlpha ? gridItemAlpha : gridItemAlpha = GetComponent<GridItemAlpha>();
    

    public int X { get; set; }
    public int Y { get; set; }

    public bool IsOnGrid { get; private set; }

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

    [SerializeField]
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

    Vector3 cachedPos;

    public void AttachItem(GridController grid, int x, int y)
    {
        IsOnGrid = true;
        Grid = grid;
        X = x;
        Y = y;
        
        GetComponent<LeanDragTranslateAlong>().ScreenDepth.Object = Grid.Plane;

        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * -0.2f, 0.4f, 0);

        SoundManager.PlaySound("drop");
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
    
    
    void Update()
    {
        bool isEnabled =
            !GameScene.Instance.started
            && (GameScene.Instance.character == null || GameScene.Instance.character.ActiveRoom != this);
        
        GetComponent<LeanDragTranslateAlong>().enabled = isEnabled;
        GetComponent<LeanSelectableByFinger>().enabled = isEnabled;
    }
    
    void OnEnable()
    {
        GetComponent<LeanSelectableByFinger>().OnSelectedFinger.AddListener(OnSelectedFingerHandler);
        GetComponent<LeanSelectableByFinger>().OnSelectedFingerUp.AddListener(OnSelectedFingerUpHandler);
    }
    
    void OnDisable()
    {
        GetComponent<LeanSelectableByFinger>().OnSelectedFinger.RemoveListener(OnSelectedFingerHandler);
        GetComponent<LeanSelectableByFinger>().OnSelectedFingerUp.RemoveListener(OnSelectedFingerUpHandler);
    }
    
    void OnSelectedFingerHandler(LeanFinger finger)
    {
        if(GridItemAlpha != null)
            GridItemAlpha.SetAlpha(0.4f);
        
        Grid.DetachItem(X, Y); //detach this
    }

    void OnSelectedFingerUpHandler(LeanFinger finger)
    {
        if(GridItemAlpha != null)
            GridItemAlpha.SetAlpha(1);
        
        if (!Grid.TryGetCellCoordinates(transform.position, out int x, out int y)
            || !Grid.TryPlaceItem(this, x, y))
        {
            Grid.TryPlaceItem(this, X, Y);
        }
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
        drag.Damping = 10;

    }
#endif
    
    public void MarkAsNotValid()
    {
        SoundManager.PlaySound("wrong"); //fix activating all at same time
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.3f, 1, 5);
    }

    public void UpdatePortals()
    {
        if(GridItemPortals != null)
            GridItemPortals.UpdatePortals();
    }
}
