using TMPro;
using UnityEngine;

public class GridItemAlpha : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TextMeshPro textRenderer;


    public void SetAlpha(float alpha)
    {
        var color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
        
        color = textRenderer.color;
        color.a = alpha;
        textRenderer.color = color;
    }

}
