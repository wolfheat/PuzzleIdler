using System;
using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseItem : MonoBehaviour
{
    [Header("BASE")]
    // Image illustrating the upgrade
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI imageText;

    // Button the player clicks for upgrade
    [SerializeField] protected Image upgradeButtonImage;

    [SerializeField] private Color available;
    [SerializeField] private Color unAvailable;
    protected void SetImage(Sprite sprite) => image.sprite = sprite;
    protected void SetImageText(string newText) => imageText.text = newText;
    protected void SetAvailableColor(bool v) => upgradeButtonImage.color = v ? available : unAvailable;

}

public class ResearchItem : BaseItem
{
    [Header("RESEARCH")]
    [SerializeField] private TextMeshProUGUI researchName;
    [SerializeField] private TextMeshProUGUI researchDescription;

    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Slider upgradeSlider;
    [SerializeField] private TextMeshProUGUI sliderText;
    
    ResearchData data;

    public ResearchData Data => data;

    [SerializeField] private Image upgradeButtonCompletedImage;
    public void SetAsOwned()
    {
        upgradeButtonImage.gameObject.SetActive(false);
        upgradeButtonCompletedImage.gameObject.SetActive(true);
    }

    public void ClickedUpgrade()
    {
        Debug.Log("Clicked to Upgrade this researchItem "+data?.ResearchName);
        Research.Instance.RequestBuyUpgrade(data);
    }

    // Send in a SO here?
    public void SetAllInfo(ResearchData resourceData, int owned)
    {
        // Set Data Image and type - info is set separate
        data = resourceData;    
        
        // Set the percent complete text here
        SetCompleteText(owned);

        // Image Text is the name here
        SetImageText(resourceData.ResearchName);

        SetImage(resourceData.ResearchImage);
    }

    private void SetCompleteText(int owned)
    {
        // Figure out the complete text here
        float percent = owned / data.steps /100;

        // If maxed dont show anything
        sliderText.text = owned == 100 ? "" : percent.ToString(); 
    }

    // Only called when not maxed
    internal void UpdateStatsAsOwned(bool canBuy, BigDouble cost)
    {
        // Color of Buy Button
        SetAvailableColor(canBuy);

        // Cost of next Upgrade
        upgradeCostText.text = Stats.ReturnAsString(cost);
    }

    // Only called when not maxed
    internal void UpdateStats(bool canBuy, BigDouble cost)
    {
        // Color of Buy Button
        SetAvailableColor(canBuy);

        // Cost of next Upgrade
        upgradeCostText.text = Stats.ReturnAsString(cost);
    }
}
