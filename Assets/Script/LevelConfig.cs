
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Serializable]
    public class Entrance
    {
        public DungeonMultiplierOperation operation;
        public ParamsContainer paramsContainer;
        public int portalsCount = 2;
        public bool isRed;
    }

    [SerializeField] Vector2Int fieldSize;
    [SerializeField] Entrance[] allowedTiles;

    public Vector2Int FieldSize => fieldSize;

    public IReadOnlyCollection<Entrance> AllowedTiles => allowedTiles;

}
