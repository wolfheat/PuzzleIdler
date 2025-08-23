using System;
using BreakInfinity;
using UnityEngine;

[CreateAssetMenu(menuName = "ResearchData", fileName = "Scriptable Objects/")]
public class ResearchData : ScriptableObject
{
    public string ResearchName;
    public Sprite ResearchImage;
    public BigDoubleStruct baseCost;
    public double costIncrementMultiplier;
    public int steps = 1;

    internal int GetCost(int ownedAmt)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public struct BigDoubleStruct
{
    public double Mantissa;
    public long Exponent;
}
