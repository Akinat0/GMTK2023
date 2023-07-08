
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Common;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] int gridHeight = 3;
    [SerializeField] int gridWidth = 3;
    [SerializeField] float cellSize = 1;

    [SerializeField] GridTile gridTilePrefab;
    
    LeanPlane plane;
    GridItem[] items;

    public int Width => gridWidth;
    public int Height => gridHeight;

    public IReadOnlyCollection<GridItem> Items => items;

    public LeanPlane Plane => plane;

    public void Build(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        
        plane = new GameObject("Grid").AddComponent<LeanPlane>();

        plane.ClampX = true;
        plane.ClampY = true;

        plane.SnapX = cellSize;
        plane.SnapY = cellSize;
        
        
        plane.MinX = 0;
        plane.MaxX = (gridHeight - 1) * cellSize;
        plane.MinY = 0;
        plane.MaxY = (gridWidth - 1) * cellSize;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                GridTile gridTile = Instantiate(gridTilePrefab, new Vector2(i * cellSize, j * cellSize), Quaternion.identity);
                gridTile.Init(i, j, this);
            }
        }

        items = new GridItem[gridHeight * gridWidth];
    }

    public bool IsBusyCell(int x, int y)
    {
        return items[GetCellIndex(x, y)] != null;
    }

    public GridItem DetachItem(int x, int y)
    {
        int index = GetCellIndex(x, y);
        
        GridItem item = items[index];
        item.DetachItem();
        items[index] = null;
        return item;
    }
    
    public bool TryPlaceItem(GridItem item, int x, int y)
    {
        if (IsBusyCell(x, y))
            return false;

        if (item.IsOnGrid) //it can be first placement for this item
            DetachItem(item.X, item.Y);

        item.AttachItem(this, x, y);
        
        items[GetCellIndex(x, y)] = item;

        TryGetCellPosition(x, y, out Vector3 pos);

        item.transform.DOMove(pos, 0.2f);

        return true;
    }

    public bool TryGetCellCoordinates(Vector3 pos, out int x, out int y)
    {
        x = (int) (pos.x / cellSize + cellSize / 2);
        y = (int) (pos.y / cellSize + cellSize / 2);

        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
            return false;

        return true;
    }

    public bool TryGetCellPosition(int x, int y, out Vector3 pos)
    {
        pos = new Vector3(x * cellSize, y * cellSize, -1);

        if (x <= gridWidth || y <= gridHeight)
            return false;

        return true;
    }

    public bool IsCellOutOfGrid(int x, int y)
    {
        return x < 0 || y < 0 || x >= gridWidth || y >= gridHeight;
    }
    
    public bool IsCellOnGrid(int x, int y)
    {
        return !IsCellOutOfGrid(x, y);
    }
    
    public int GetCellIndex(int x, int y)
    {
        return y * gridWidth + x;
    }

    public GridItem GetCellItem(int x, int y)
    {
        return IsCellOnGrid(x, y) ? items[GetCellIndex(x, y)] : null;
    }

}
