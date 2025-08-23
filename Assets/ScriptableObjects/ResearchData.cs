using System;
using UnityEngine;


public enum ResearchRewardType { CPS, GPS }

[CreateAssetMenu(menuName = "ResearchData", fileName = "Scriptable Objects/")]
public class ResearchData : ScriptableObject
{
    public string ResearchName;
    public Sprite ResearchImage;
    public BigDoubleStruct baseCost;
    public double costIncrementMultiplier;
    public int steps = 1;
    public ResearchRewardType RewardType;
    public float RewardValueInPercent;

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
