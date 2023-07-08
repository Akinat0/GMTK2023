
using System;
using UnityEngine;

[Serializable]
public class ParamsContainer
{
    public ParamsContainer(ParamsContainer other)
    {
        Hp = other.Hp;
    }
    
    [SerializeField]
    public int Hp;

    public void Apply(ParamsContainer container)
    {
        Hp += container.Hp;
    }
}
