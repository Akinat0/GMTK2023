using Lean.Common;
using UnityEngine;

[RequireComponent(typeof(GridItem))]
public class GridSelectableDragSound : LeanSelectableBehaviour
{
    GridItem GridItem { get; set; }

    void Awake()
    {
        GridItem = GetComponent<GridItem>();
    }

    bool shouldPlaySound;
    int X { get; set; }
    int Y { get; set; }

    protected override void OnSelected(LeanSelect select)
    {
        base.OnSelected(select);

        shouldPlaySound = true;
    }

    protected override void OnDeselected(LeanSelect select)
    {
        base.OnDeselected(select);
        
        shouldPlaySound = false;
    }

    void LateUpdate()
    {
        if (!shouldPlaySound)
            return;

        if (!GameScene.Grid.TryGetCellCoordinates(transform.position, out int x, out int y)) 
            return;

        if (X == x && Y == y) 
            return;
        
        SoundManager.PlaySound("tile");
        X = x;
        Y = y;
    }
}
