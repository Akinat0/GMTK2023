
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [SerializeField] Vector2Int fieldSize;
    [SerializeField] GridItem startRoom;
    [SerializeField] GridItem finalRoom;
    [SerializeField] GridItem[] allowedTiles;

    public Vector2Int FieldSize => fieldSize;

    public GridItem StartRoom => startRoom;
    public GridItem FinalRoom => finalRoom;
    public IReadOnlyCollection<GridItem> AllowedTiles => allowedTiles;

}
