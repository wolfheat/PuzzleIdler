using System;
using UnityEngine;


public enum ResearchRewardType { CPS, GPS }

[CreateAssetMenu(menuName = "Datas/ResearchData", fileName = "ResearchData")]
public class ResearchData : ScriptableObject
{
    public string ResearchName;
    public Sprite ResearchImage;
    public BigDoubleStruct baseCost;
    public double costIncrementMultiplier;
    public ResearchRewardType RewardType;
    public float RewardValueInPerStepsInPercent;
    public int steps = 1;
    public bool isPercent = true;

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
