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

                portals.Add(fakedDir);
            }
        }

        Sprite sprite = null;

        Quaternion rotation = portals.Count > 0 
            ? Quaternion.Euler(0, 0, 90 * (int)portals[0]) 
            : spriteRenderer.transform.rotation;
        
        switch (item.PortalsCount)
        {
            case 1:
                sprite = sprite0001;
                break;
            case 2:

                bool isTurn = false;
                
                if (portals.Count == 2)
                {
                    int distance = Mathf.Abs((int)portals[0] - (int)portals[1]);
                    isTurn = distance is 1 or 3;

                    if (distance == 3 && portals[0] == Direction.Top)
                    {
                        rotation = Quaternion.Euler(0, 0, -90);
                    }
                }
                
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
        spriteRenderer.color = item.IsRed ? new Color(0.84f, 0.34f, 0.26f) : new Color(0.58f, 0.72f, 0.35f);
    }
}
