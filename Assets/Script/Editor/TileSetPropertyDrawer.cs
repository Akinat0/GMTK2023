
using System.Diagnostics.Tracing;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(TileSet))]
public class TileSetPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        
        PropertyField mainProperty = new PropertyField(property);
        Box box = new Box { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

        mainProperty.RegisterValueChangeCallback(changeEvent => UpdateTileSet(changeEvent.changedProperty.objectReferenceValue as TileSet));
        
        container.Add(mainProperty);
        container.Add(box);

        return container;

        void UpdateTileSet(TileSet tileSet)
        {
            box.Clear();

            if (tileSet == null)
                return;

            foreach (TileSet.Entrance tile in tileSet.AllowedTiles)
            {
                Texture previewTex = RoomPreview.CreatePreview(tile);
                
                Image preview = new Image { image = previewTex ? previewTex : Texture2D.redTexture };
                VisualElement spacer = new VisualElement { style = { width = 10 } };

                box.Add(preview);
                box.Add(spacer);
            }
            
        }
        
        // void UpdatePreview(TileSet)
        // {
        //     TileSet.Entrance target = (TileSet.Entrance)EditorHelpers.GetTargetObjectOfProperty(serializedProperty);
        //
        //     Texture previewTex = RoomPreview.CreatePreview(target);
        //     preview.image = previewTex != null
        //         ? previewTex
        //         : Texture2D.redTexture;
        // }
    }
    
}
