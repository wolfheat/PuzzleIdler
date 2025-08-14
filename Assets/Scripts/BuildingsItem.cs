using System;
using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingsItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI imageText;
    [SerializeField] private Image image;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI upgradeGain;
    [SerializeField] private TextMeshProUGUI upgradeAmoutText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Color available;
    [SerializeField] private Color unAvailable;
    BuildingsData data;
    int index;
    int amt = 1;
    public BuildingsData Data => data;
    private void SetImageText(string newText) => imageText.text = newText;
    private void SetImage(Sprite sprite) => image.sprite = sprite;
    

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

        SetLevelText(buildingsData.Level);
    }

    private void SetAvailableColor(bool v) => upgradeButtonImage.color = v ? available : unAvailable;

    private void SetUpgradeBenefitValue()
    {
        Debug.Log("Calculate what benefit an upgrade click would do, depends on current upgrade mode");
        upgradeGain.text = "+1.23 UDc";
    }

    private void SetLevelText(int level)
    {
        int nextTierLevel = GetNextTierLevel(level);
        levelText.text = "Level " + level + "/" + nextTierLevel;
    }

    private int GetNextTierLevel(int level)
    {
        int fifties = (level / 50 +1)*50;
        return level switch
        {
            0 => 1,
            < 10 => 10,
            < 25 => 25,
            < 50 => fifties,
            _ => 10000
        };
    }

    internal void UpdateStats(bool canBuy, int buyAmt, BigDouble cost, BigDouble gain)
    {
        // Color of Buy Button
        SetAvailableColor(canBuy);

        amt = buyAmt;

        // Amount to Buy
        upgradeAmoutText.text = "Buy x" + buyAmt;
        upgradeCostText.text = Stats.ReturnAsString(cost);
        upgradeGain.text = Stats.ReturnAsString(gain);
    }

    internal void AddLevels(int amt)
    {
        data.Level += amt;
        SetLevelText(data.Level);
    }
}
