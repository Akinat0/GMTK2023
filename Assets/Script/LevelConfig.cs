using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] TileSet startTileSet;
        [SerializeField] TileSet[] randomTileSets;

        public TileSet StartTileSet => startTileSet;
        public IReadOnlyCollection<TileSet> RandomTileSets => randomTileSets;
    }
}