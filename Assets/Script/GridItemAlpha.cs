using TMPro;
using UnityEngine;
using Lean.Common;

public class GridItemAlpha : LeanSelectableBehaviour
{
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] TextMeshPro[] textRenderers;
    [SerializeField] GameObject contentRenderer;
    
    protected override void Start()
    {
        base.Start();

        UpdateAlpha();
    }
    
    // protected override void OnSelected(LeanSelect select)
    // {
    //     UpdateAlpha();
    // }
    //
    // protected override void OnDeselected(LeanSelect select)
    // {
    //     UpdateAlpha();
    // }

    public void UpdateAlpha()
    {
        var alpha = Selectable != null && Selectable.IsSelected ? 0.4f : 1;
        SetAlpha(alpha);
    }

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
