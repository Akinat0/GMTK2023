using DG.Tweening;
using Lean.Touch;
using UnityEngine;

public enum Direction
{
    None = -1,
    Bottom = 0,
    Right = 1, 
    Top = 2, 
    Left = 3
}

[DisallowMultipleComponent]
[RequireComponent(typeof(LeanDragTranslateAlong), typeof(LeanSelectableByFinger))]
public class GridItem : MonoBehaviour
{
    public static Direction[] Neighbours = new[] { Direction.Bottom, Direction.Right, Direction.Top, Direction.Left };
    
    [SerializeField] int entersCount;
    [SerializeField] int exitsCount;
    [SerializeField] ParamsContainer paramsContainer;

    public int X { get; set; }
    public int Y { get; set; }

    public bool IsOnGrid { get; private set; }

    public ParamsContainer Params => paramsContainer;

    public bool IsValid
    {
        get
        {
            int counter = 0;
            
            foreach (var neighbour in Neighbours)
            {
                var item = GetNeighbour(neighbour);

                if (item != null)
                    counter++;
            }

            return counter == entersCount + exitsCount;
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

        SoundManager.PlaySound("drop");
    }

    public void DetachItem()
    {
        IsOnGrid = false;
        SoundManager.PlaySound("pick");
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
        Grid.DetachItem(X, Y); //detach this
    }

    void OnSelectedFingerUpHandler(LeanFinger finger)
    {
        if (!Grid.TryGetCellCoordinates(transform.position, out int x, out int y)
            || !Grid.TryPlaceItem(this, x, y))
        {
            Grid.TryPlaceItem(this, X, Y);
        }
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
        transform.DOPunchScale(Vector3.one * 0.3f, 1, 5);
    }
}
