
using System;
using UnityEngine;

[Serializable]
public class ParamsContainer
{
    public ParamsContainer(ParamsContainer other)
    {
        Hp = other.Hp;
        Exp = other.Exp;
        defaultHp = Hp;
    }

    public ParamsContainer()
    {
        defaultHp = Hp;
    }
    
    [SerializeField]
    public float Hp;
    public int Lvl;
    public int Exp;
    
    [NonSerialized]
    public float defaultHp;
    
    public void Apply(ParamsContainer container, Dungeon dungeon)
    {
        Hp = Mathf.Clamp(Hp + container.Hp * dungeon.Multiplier, 0, defaultHp);
        Exp += container.Exp;
    }
}
