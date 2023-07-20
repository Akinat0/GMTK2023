
using System;
using System.Collections.Generic;
using System.Reflection;
using Abu.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "TileSet", menuName = "TileSet")]
public class TileSet : ScriptableObject
{
    // [Serializable]
    // public class Entrances
    // {
    //     public Entrance[] allowedTiles;
    // }
    
    [Serializable]
    public class Entrance
    {
        public DungeonMultiplierOperation Operation => operation;
        public ParamsContainer ParamsContainer => paramsContainer;
        public int PortalsCount => portalsCount;
        public bool IsRed => isRed;

        [SerializeField] DungeonMultiplierOperation operation;
        [SerializeField] ParamsContainer paramsContainer;
        [SerializeField] int portalsCount = 2;
        [SerializeField] bool isRed;
    }
    
     [SerializeField] Entrance[] entrances;

    public IReadOnlyCollection<Entrance> AllowedTiles => entrances;

}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(TileSet.Entrance), true)]
public class TileSetEntrancePropertyDrawer : PropertyDrawer
{
    static GridItem gridItem;
    
    static GridItem GridItem =>
        gridItem ? gridItem : gridItem = AssetDatabase.LoadAssetAtPath<GridItem>("Assets/Prefab/Room.prefab");
    
    readonly Dictionary<TileSet.Entrance, Texture> cachedPreviewImages = new ();

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
            TileSet.Entrance target = (TileSet.Entrance)GetTargetObjectOfProperty(serializedProperty);
        
            CreatePreview(target);
        
            preview.image = cachedPreviewImages.TryGetValue(target, out Texture previewTexture) && previewTexture != null
                ? previewTexture
                : Texture2D.redTexture;
        }
        
        return container;
    }

    void CreatePreview(TileSet.Entrance target)
    {
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
            
        if (cachedPreviewImages.ContainsKey(target))
        {
            //destroy old texture 
            Texture texture = cachedPreviewImages[target];
            
            if(texture != null)
                Object.DestroyImmediate(texture);
            
            cachedPreviewImages[target] = preview;
        }
        else
        {
            cachedPreviewImages.Add(target, preview);
        }

        Object.DestroyImmediate(item.gameObject);
    }

    static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }
    
    static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }
    
    static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }
    
    
}



internal static class ArrayPropertyFieldUtility
    {
        internal delegate void OnResize(int newSize);
 
        internal delegate void OnElementChanged(int changedIndex, SerializedProperty newElement);
 
        /// <summary>
        /// Detects size change and property change of each element.
        /// </summary>
        /// <param name="listenerParent">Spawn invisible listeners to this parent.</param>
        /// <param name="listenerChildren">Allocates a list to remember invisible listeners.</param>
        /// <param name="pf">Visual element drawing serialized array that you want to register callback.</param>
        /// <param name="sp">The serialized array which is bound to <paramref name="pf"/>.</param>
        /// <param name="onAnyChange">Called on both array size change and array element change.</param>
        internal static void RegisterArrayChangeCallback(
            VisualElement listenerParent,
            IList<VisualElement> listenerChildren,
            PropertyField pf,
            SerializedProperty sp,
            Action onAnyChange) =>
            RegisterArrayChangeCallback(listenerParent, listenerChildren, pf, sp, size => onAnyChange(),
                (index, element) => onAnyChange());
 
        /// <summary>
        /// Detects size change and property change of each element.
        /// </summary>
        /// <param name="listenerParent">Spawn invisible listeners to this parent.</param>
        /// <param name="listenerChildren">Allocates a list to remember invisible listeners.</param>
        /// <param name="pf">Visual element drawing serialized array that you want to register callback.</param>
        /// <param name="sp">The serialized array which is bound to <paramref name="pf"/>.</param>
        /// <param name="onResize">Call when array size changed.</param>
        /// <param name="onElementChanged">Call when array element changed.</param>
        internal static void RegisterArrayChangeCallback(
            VisualElement listenerParent,
            IList<VisualElement> listenerChildren,
            PropertyField pf,
            SerializedProperty sp,
            OnResize onResize,
            OnElementChanged onElementChanged)
        {
            if (!sp.isArray)
            {
                throw new Exception("Property is not serializing an array.");
            }
 
            var basePath = pf.bindingPath;
            var arraySizePath = basePath + ".size";
 
            var invisibleArraySizeListener = new PropertyField
            {
                name = "INVISIBLE-LISTENER-SIZE",
                bindingPath = arraySizePath,
                style =
                {
                    display = DisplayStyle.None
                }
            };
            // Array size listener is permanently added to the parent.
            // Element listener gets added and removed depending on the current size.
            invisibleArraySizeListener.RegisterValueChangeCallback(evt =>
            {
                var newSize = evt.changedProperty.intValue;
                onResize(newSize);
                // Automatically increase/decrease the invisible listeners.
                // Size changed is triggered for sure on the first render?
                EnsureListenersToSize(newSize, listenerParent, listenerChildren, sp, onElementChanged);
            });
            invisibleArraySizeListener.Bind(sp.serializedObject);
            listenerParent.Add(invisibleArraySizeListener);
 
            static void EnsureListenersToSize(
                int maxSize,
                VisualElement listenerParent,
                IList<VisualElement> current,
                SerializedProperty sp,
                OnElementChanged onElementChanged)
            {
                if (current.Count == maxSize)
                {
                    return;
                }
 
                if (current.Count > maxSize)
                {
                    var reduce = current.Count - maxSize;
                    for (var i = 0; i < reduce; i++)
                    {
                        current[current.Count - 1].RemoveFromHierarchy();
                        current.RemoveAt(current.Count - 1);
                    }
                }
                else
                {
                    var increase = maxSize - current.Count;
                    var startIndex = current.Count;
                    for (var i = 0; i < increase; i++)
                    {
                        var prop = sp.GetArrayElementAtIndex(i);
                        var invisibleElementListener = new PropertyField(prop)
                        {
                            name = $"INVISIBLE-LISTENER-{startIndex + i}",
                            style =
                            {
                                display = DisplayStyle.None
                            }
                        };
                        var i1 = i;
                        invisibleElementListener.RegisterValueChangeCallback(evt =>
                        {
                            var newProp = evt.changedProperty;
                            onElementChanged(i1, newProp);
                        });
                        invisibleElementListener.Bind(sp.serializedObject);
                        listenerParent.Add(invisibleElementListener);
                        current.Add(invisibleElementListener);
                    }
                }
            }
        }
    }
#endif
