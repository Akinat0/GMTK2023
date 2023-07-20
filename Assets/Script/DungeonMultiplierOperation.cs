
using System;
using UnityEngine;

[Serializable]
public struct DungeonMultiplierOperation
{
    public enum Operation
    {
        None, 
        Multiply,
        Add,
        Subtract,
        Divide,
        Power,
        Sqrt
    }

    [SerializeField] Operation operation;
    [SerializeField] float value;

    public Operation Operator => operation;
    public float Value => value;

    public void Apply(Dungeon dungeon)
    {
        switch (operation)
        {
            case Operation.Add:
                dungeon.Multiplier += value;
                break;
            case Operation.Subtract:
                dungeon.Multiplier -= value;
                break;
            case Operation.Multiply:
                dungeon.Multiplier *= value;
                break;
            case Operation.Divide:
                dungeon.Multiplier /= value;
                break;
            case Operation.Power:
                dungeon.Multiplier = Mathf.Pow(dungeon.Multiplier, value);
                break;
            case Operation.Sqrt:
                dungeon.Multiplier = Mathf.Sqrt(dungeon.Multiplier);
                break;
        }

        dungeon.Multiplier = Mathf.Max(dungeon.Multiplier, 1);
    }

    public override string ToString()
    {
        switch (operation)
        {
            case Operation.Add:
                return $"+{value}";
            case Operation.Subtract:
                return $"-{value}";
            case Operation.Multiply:
                return $"x{value}";
            case Operation.Divide:
                return $"/{value}";
            case Operation.Power:
                return $"^{value}";
            case Operation.Sqrt:
                return $"√";
        }

        return string.Empty;
    }
    
    
    public bool Equals(DungeonMultiplierOperation other)
    {
        return operation == other.operation && value.Equals(other.value);
    }

    public override bool Equals(object obj)
    {
        return obj is DungeonMultiplierOperation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)operation, value);
    }
}
