using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Common;
using Lean.Touch;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] int gridHeight = 3;
    [SerializeField] int gridWidth = 3;
    [SerializeField] float cellSize = 1;

    [SerializeField] GridTile gridTilePrefab;
    
    public event Action<GridItem> OnSelectFingerDown;
    public event Action<GridItem> OnSelectFingerUp;
        
    LeanPlane plane;
    GridItem[] items;

    public int Width => gridWidth;
    public int Height => gridHeight;
    public float CellSize => cellSize;
    

    public IReadOnlyCollection<GridItem> Items => items;

    public LeanPlane Plane => plane;
    BoxCollider cameraBounds;

    public void Build(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        
        plane = new GameObject("Grid").AddComponent<LeanPlane>();

        Vector3 gridPos = plane.transform.position;
        gridPos.z = -0.5f;
        plane.transform.position = gridPos;

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
                GridTile gridTile = Instantiate(gridTilePrefab, new Vector3(i * cellSize, j * cellSize, 2), Quaternion.identity);
                gridTile.Init(i, j, this);
            }
        }

        items = new GridItem[gridHeight * gridWidth];

        // itemsMovable = new BitArray(gridHeight * gridWidth, true);
    }

    public bool IsBusyCell(int x, int y)
    {
        if (!IsCellOnGrid(x, y))
            return false;
        
        return items[GetCellIndex(x, y)] != null;
    }

    public GridItem DetachItem(int x, int y)
    {
        int index = GetCellIndex(x, y);
        
        GridItem item = items[index];
        item.DetachItem();
        items[index] = null;
        UpdateNeighbourPortals(item);
        return item;
    }

    public bool TryPlaceItem(GridItem item, int x, int y, Action onComplete = null)
    {
        if (IsBusyCell(x, y))
            return false;

        if (item.IsOnGrid)
            DetachItem(item.X, item.Y);

        item.AttachItem(this, x, y);
        
        items[GetCellIndex(x, y)] = item;
        
        item.UpdatePortals();
        UpdateNeighbourPortals(item);

        TryGetCellPosition(x, y, out Vector3 pos);

        item.transform.DOMove(pos, 0.2f).OnComplete(() => onComplete?.Invoke());

        return true;
    }

    void UpdateNeighbourPortals(GridItem item)
    {
        foreach (var direction in GridItem.Neighbours)
        {
            var neighbour = item.GetNeighbour(direction);
            
            if(neighbour != null)
                neighbour.UpdatePortals();
        }
    }
    
    public bool TryGetCellCoordinates(Vector3 pos, out int x, out int y)
    {
        x = Mathf.FloorToInt(pos.x / cellSize + cellSize / 2);
        y = Mathf.FloorToInt(pos.y / cellSize + cellSize / 2);

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
    
    public void SetMovable(int x, int y, bool isMovable)
    {
        items[GetCellIndex(x, y)].IsMovable = isMovable;
    }

    public void SetMovableAll(bool isMovable)
    {
        if(items == null || items.Length == 0)
            return;
        
        foreach (var item in items.Where(item => item != null))
            item.IsMovable = isMovable;
    }

    public void SetLockedOnGrid(int x, int y, bool isLocked)
    {
        items[GetCellIndex(x, y)].IsLockedOnGrid = isLocked;
    }
    
    public void SetLockedOnGridAll(bool isLocked)
    {
        if(items == null || items.Length == 0)
            return;

        foreach (var item in items.Where(item => item != null))
        {
            if (item == GameScene.Character.ActiveRoom)
            {
                item.IsLockedOnGrid = true;
                continue;
            }

            if (item.IsFireplace)
            {
                item.IsLockedOnGrid = false;
                continue;
            }
            
            
            item.IsLockedOnGrid = isLocked;
        }
    }

    public void InvokeOnSelectFingerDown(GridItem gridItem)
    {
        OnSelectFingerDown?.Invoke(gridItem);
    }
    
    public void InvokeOnSelectFingerUp(GridItem gridItem)
    {
        OnSelectFingerUp?.Invoke(gridItem);
    }

}
