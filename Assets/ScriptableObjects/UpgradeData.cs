using System;
using BreakInfinity;
using UnityEngine;
public enum UpgradeType{IncomeBoosters,GemBoosters};

[Serializable][CreateAssetMenu(menuName = "UpgradeData",fileName = "Scriptable Objects/")]
public class UpgradeData : ScriptableObject
{
    public string UpgradeName;
    public Sprite Image;
    public UpgradeType type= UpgradeType.IncomeBoosters;
    public int cost;
    public BigDouble value = 1;
    public bool unlocked = false;

    [TextArea(15, 20)]
    public string Description;

}
