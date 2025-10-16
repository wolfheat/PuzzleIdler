using System;
using UnityEngine;
public enum UpgradeType{IncomeBoosters,GemBoosters};

[Serializable][CreateAssetMenu(menuName = "UpgradeData",fileName = "Scriptable Objects/")]
public class UpgradeData : ScriptableObject
{
    public string UpgradeName;
    public Sprite Image;
    public UpgradeType type= UpgradeType.IncomeBoosters;
    public int cost;
    public float UpgradeValue = 1;

    [TextArea(15, 20)]
    public string Description;

}
