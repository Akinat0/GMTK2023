
using System;
using UnityEngine;

[Serializable]
public class ParamsContainer
{
    public ParamsContainer(ParamsContainer other)
    {
        Hp = other.Hp;
        Exp = other.Exp;
    }
    
    [SerializeField]
    public float Hp;
    public int Lvl;
    public int Exp;

    public void Apply(ParamsContainer container, Dungeon dungeon)
    {
        Hp += container.Hp * dungeon.Multiplier;
        Exp += container.Exp;
    }
}
