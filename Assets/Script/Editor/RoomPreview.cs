
using System.Collections.Generic;
using System.Reflection;
using Abu.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class RoomPreview
{
    static GridItem gridItem;
    
    static GridItem GridItem =>
        gridItem ? gridItem : gridItem = AssetDatabase.LoadAssetAtPath<GridItem>("Assets/Prefab/Room.prefab");
    
    static readonly Dictionary<TileSet.Entrance, Texture> CachedPreviewImages = new ();
    
    public static Texture CreatePreview(TileSet.Entrance target)
    {
        if (CachedPreviewImages.TryGetValue(target, out Texture previewTexture))
            return previewTexture;
        
        GridItem item = Object.Instantiate(GridItem);
        
        item.IsRed = target.IsRed;
        item.Params = target.ParamsContainer;
        item.DungeonOperation = target.Operation;
        item.PortalsCount = target.PortalsCount;
            
        EditorApplication.QueuePlayerLoopUpdate();
        SceneView.RepaintAll();

        Texture preview = item.GridItemPortals.SpriteRenderer.sprite != null
            ? CaptureUtility.Capture(item.GridItemPortals.SpriteRenderer, false)
            : null;
            
        if (CachedPreviewImages.ContainsKey(target))
        {
            //destroy old texture 
            Texture texture = CachedPreviewImages[target];
            
            if(texture != null)
                Object.DestroyImmediate(texture);
            
            CachedPreviewImages[target] = preview;
        }
        else
        {
            CachedPreviewImages.Add(target, preview);
        }

        Object.DestroyImmediate(item.gameObject);

        return preview;
    }
}

