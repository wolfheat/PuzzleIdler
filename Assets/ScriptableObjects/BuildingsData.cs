using UnityEngine;

[CreateAssetMenu(menuName = "BuildingData",fileName = "Scriptable Objects/")]
public class BuildingsData : ScriptableObject
{
    public string BuildingName;
    public Sprite BuildingImage;
    public int baseCost;
    public double costIncrement;
}
