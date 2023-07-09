using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GridItemPortals : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GridItem item;

    [SerializeField] Sprite sprite0001;
    [SerializeField] Sprite sprite0011;
    [SerializeField] Sprite sprite0101;
    [SerializeField] Sprite sprite0111;
    [SerializeField] Sprite sprite1111;

    readonly List<Direction> portals = new List<Direction>(4);

    public void UpdatePortals()
    {
        portals.Clear();

        foreach (var direction in GridItem.Neighbours)
        {
            if (item.GetNeighbour(direction) != null)
            {
                var fakedDir = direction;
                
                
                //hack
                if (direction == Direction.Bottom)
                    fakedDir = Direction.Top;
                else if (direction == Direction.Top)
                    fakedDir = Direction.Bottom;
                
                portals.Add(fakedDir);
            }
        }
        
        
        
        // for (int i = 0; i < Mathf.Min(item.PortalsCount, 4); i++)
        // {
        //     var direction = GridItem.Neighbours[i];
        //     
        //     if (item.GetNeighbour(direction) != null)
        //     {
        //         portals.Add(direction);
        //     }
        // }

        Sprite sprite = null;

        Quaternion rotation = portals.Count > 0 
            ? Quaternion.Euler(0, 0, 90 * (int)portals[0]) 
            : Quaternion.identity;
        
        switch (item.PortalsCount)
        {
            case 1:
                sprite = sprite0001;
                break;
            case 2:
                bool isTurn = portals.Count == 2 && Mathf.Abs((int)portals[0] - (int)portals[1]) == 1;
                
                sprite = isTurn ? sprite0011 : sprite0101;
                break;
            case 3:
                sprite = sprite0111;
                break;
            case 4:
                sprite = sprite1111;
                break;
        }

        spriteRenderer.sprite = sprite;
        spriteRenderer.transform.rotation = rotation;
    }
}
