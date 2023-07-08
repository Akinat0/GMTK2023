
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
    public int Hp;
    public int Lvl;
    public int Exp;

    public void Apply(ParamsContainer container)
    {
        Hp += container.Hp;
        Exp += container.Exp;
    }
}
