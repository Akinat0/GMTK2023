
using System;
using UnityEngine;

[Serializable]
public struct DungeonMultiplierOperation
{
    enum Operation
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
    }
}
