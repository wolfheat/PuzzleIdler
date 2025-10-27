using UnityEngine;

[CreateAssetMenu(menuName = "Datas/ResearchReward", fileName = "ResearchReward")]
public class ResearchReward : ScriptableObject
{
    public float totalPercent;
    public ResearchRewardType Type;
}
