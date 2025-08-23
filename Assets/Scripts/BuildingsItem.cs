using BreakInfinity;
using TMPro;
using UnityEngine;

public class BuildingsItem : BaseItem
{
    [Header("BUILDINGS")]

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI upgradeGain;
    [SerializeField] private TextMeshProUGUI upgradeAmoutText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;    
    BuildingsData data;
    int index;
    int amt = 1;
    public BuildingsData Data => data;    

    public void ClickedUpgrade()
    {
        Debug.Log("Clicked to Upgrade this building "+data?.BuildingName);
        Buildings.Instance.RequestUpgrade(index,amt);
    }

    // Send in a SO here?
    public void SetAllInfo(BuildingsData buildingsData, int i)
    {
        // Set Data Image and type - info is set separate
        data = buildingsData;
        index = i;
        SetImageText(buildingsData.BuildingName);
        SetImage(buildingsData.BuildingImage);

        SetLevelText(BuildingDatas.Instance.owned[i]);
    }

    private void SetLevelText(int level)
    {
        int nextTierLevel = GetNextTierLevel(level);
        levelText.text = "Level " + level + "/" + nextTierLevel;
    }

    private int GetNextTierLevel(int level)
    {
        int fifties = (level / 50+1)*50;
        return level switch
        {
            0 => 1,
            < 10 => 10,
            < 25 => 25,
            _ => fifties
        };
    }

    internal void UpdateStats(bool canBuy, int buyAmt, BigDouble cost, BigDouble gain)
    {
        // Color of Buy Button
        SetAvailableColor(canBuy && buyAmt > 0);

        amt = buyAmt;

        // Amount to Buy
        upgradeAmoutText.text = "Buy x" + buyAmt;
        upgradeCostText.text = Stats.ReturnAsString(cost);
        upgradeGain.text = "+"+Stats.ReturnAsString(gain);  
    }

    public void UpdateLevelText() => SetLevelText(BuildingDatas.Instance.owned[index]);
}
