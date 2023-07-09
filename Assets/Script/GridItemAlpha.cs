
using TMPro;
using UnityEngine;
using System.Collections;

public class GridItemAlpha : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] TextMeshPro[] textRenderers;
    [SerializeField] GameObject contentRenderer;

    public void SetAlpha(float alpha)
    {

        foreach (var spriteRenderer in spriteRenderers)
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        
        foreach (var textRenderer in textRenderers)
        {
            var color = textRenderer.color;
            color.a = alpha;
            textRenderer.color = color;
        }
    }


    public void SetContentAlpha(float alpha)
    {
        if(contentRenderer == null)
            return;
        
        if (contentRenderer.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        else if (contentRenderer.TryGetComponent(out TextMeshPro textRenderer))
        {
            var color = textRenderer.color;
            color.a = alpha;
            textRenderer.color = color;
        }
    }


}
