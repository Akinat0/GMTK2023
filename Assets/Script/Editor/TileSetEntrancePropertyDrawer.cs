using System.Reflection;
using Abu.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(TileSet.Entrance), true)]
public class TileSetEntrancePropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        
        PropertyField mainProperty = new PropertyField(property);
        Image preview = new Image();
        
        preview.style.alignSelf = Align.FlexStart;
        
        container.Add(preview);
        container.Add(mainProperty);

        container.schedule.Execute(() => UpdatePreview(property)).Every(500);

        void UpdatePreview(SerializedProperty serializedProperty)
        {
            TileSet.Entrance target = (TileSet.Entrance)EditorHelpers.GetTargetObjectOfProperty(serializedProperty);

            Texture previewTex = RoomPreview.CreatePreview(target);
            preview.image = previewTex != null
                ? previewTex
                : Texture2D.redTexture;
        }
        
        return container;
    }
    
}
#endif