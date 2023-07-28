using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField] Color firstColor;
    [SerializeField] Color secondColor;
    [SerializeField] Color hoverColor;
    [SerializeField] Color blockedColor;
    [SerializeField] SpriteRenderer renderer;
    
    public int X { get; private set; }
    public int Y { get; private set; }

    GridController Grid; 

    public void Init(int x, int y, GridController gridController)
    {
        X = x;
        Y = y;

        Grid = gridController;

        renderer.color = GetBaseColor();
    }

    public Color GetBaseColor()
    {
        return (X + Y) % 2 == 0 ? firstColor : secondColor;
    }
    
    void OnMouseEnter()
    {
        renderer.color = Grid.IsBusyCell(X, Y) ? blockedColor : hoverColor;
    }

    void OnMouseExit()
    {
        renderer.color = GetBaseColor();
    }
    
    
    
}