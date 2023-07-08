
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Serializable]
    public struct Entrance
    {
        public GridItem item;
        public DungeonMultiplierOperation operation;
        public ParamsContainer paramsContainer;
    }

    [SerializeField] Vector2Int fieldSize;
    [SerializeField] GridItem startRoom;
    [SerializeField] GridItem finalRoom;
    [SerializeField] Entrance[] allowedTiles;

    public Vector2Int FieldSize => fieldSize;

    public GridItem StartRoom => startRoom;
    public GridItem FinalRoom => finalRoom;
    public IReadOnlyCollection<Entrance> AllowedTiles => allowedTiles;

}
