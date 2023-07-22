using TMPro;
using UnityEngine;

public class GridItemView : MonoBehaviour
{
    [SerializeField] TextMeshPro modifierText;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite spriteDamage1;
    [SerializeField] Sprite spriteDamage2;
    [SerializeField] Sprite spriteDamage3;
    [SerializeField] Sprite spriteHeal5;
    [SerializeField] Sprite spriteFireplace;
    [SerializeField] Sprite spriteSqrt;
    
    public void Initialize(GridItem item)
    {
        spriteRenderer.enabled = true;
        modifierText.enabled = false;

        if (item.IsFireplace)
        {
            spriteRenderer.sprite = spriteFireplace;
            return;
        }

        if (item.DungeonOperation.Operator == DungeonMultiplierOperation.Operation.Sqrt)
        {
            spriteRenderer.sprite = spriteSqrt;
            return;
        }
        
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
