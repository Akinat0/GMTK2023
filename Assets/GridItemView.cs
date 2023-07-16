using TMPro;
using UnityEngine;

public class GridItemView : MonoBehaviour
{
    [SerializeField] TextMeshPro modifierText;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GridItem item;

    [SerializeField] Sprite spriteDamage1;
    [SerializeField] Sprite spriteDamage2;
    [SerializeField] Sprite spriteDamage3;
    [SerializeField] Sprite spriteHeal5;
    
    void Start()
    {
        spriteRenderer.enabled = true;
        
        
        switch (item.Params.Hp)
        {
            case -1:
                spriteRenderer.sprite = spriteDamage1;
                break;
            case -2:
                spriteRenderer.sprite = spriteDamage2;
                break;
            case -3:
                spriteRenderer.sprite = spriteDamage3;
                break;
            case 5:
                spriteRenderer.sprite = spriteHeal5;
                break;
            default:
                spriteRenderer.enabled = false;
                break;
        }
        
        modifierText.enabled = item.DungeonOperation.Operator != DungeonMultiplierOperation.Operation.None;
        modifierText.text = item.DungeonOperation.ToString();
    }
}
