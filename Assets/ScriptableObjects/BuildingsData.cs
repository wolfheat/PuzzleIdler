using UnityEngine;

[CreateAssetMenu(menuName = "Datas/BuildingData", fileName = "BuildingData")]
public class BuildingsData : ScriptableObject
{
    public string BuildingName;
    public Sprite BuildingImage;
    public int baseCost;
    public double costIncrement;
}
