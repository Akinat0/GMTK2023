using TMPro;
using UnityEngine;

public class DungeonMultiplierText : MonoBehaviour
{
    [SerializeField] TextMeshPro modifierText;
    [SerializeField] GridItem item;
    
    void Start()
    {
        if (modifierText != null && item != null)
        {
            modifierText.text = item.DungeonOperation.ToString();
        }
    }
}
