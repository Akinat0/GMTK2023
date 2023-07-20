using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileSet", menuName = "TileSet")]
public class TileSet : ScriptableObject
{
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
        
        
        protected bool Equals(Entrance other)
        {
            return operation.Equals(other.operation) 
                   && paramsContainer.Equals(other.paramsContainer) 
                   && portalsCount == other.portalsCount 
                   && isRed == other.isRed;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Entrance)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(operation, paramsContainer, portalsCount, isRed);
        }
    }
    
     [SerializeField] Entrance[] entrances;

    public IReadOnlyCollection<Entrance> AllowedTiles => entrances;

}
