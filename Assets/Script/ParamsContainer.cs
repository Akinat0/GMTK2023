
using System;
using UnityEngine;

[Serializable]
public struct ParamsContainer
{
    public ParamsContainer(ParamsContainer other)
    {
        Hp = other.Hp;
        defaultHp = Hp;
    }

    [SerializeField]
    public float Hp;

    [NonSerialized]
    public float defaultHp;
    
    
    public void Apply(ParamsContainer container, Dungeon dungeon)
    {
        Hp = Mathf.Clamp(Hp + container.Hp * dungeon.Multiplier, 0, defaultHp);
    }
}
