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
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderText;
    
    ResearchData data;
    int amtStepsOwned = 0;
    public ResearchData Data => data;

    [SerializeField] private Image upgradeButtonCompletedImage;

    public void SetAsNotOwned()
    {
        upgradeButtonImage.gameObject.SetActive(true);
        upgradeButtonCompletedImage.gameObject.SetActive(false);
    }
        
    public void SetAsOwned()
    {
        upgradeButtonImage.gameObject.SetActive(false);
        upgradeButtonCompletedImage.gameObject.SetActive(true);

        // Set the amount text percent
        SetPercentCompleteText(data.steps);
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
        SetPercentCompleteText(owned);

        // Image Text is the name here
        SetImageText(resourceData.ResearchName);

        SetImage(resourceData.ResearchImage);
    }

    private void SetPercentCompleteText(int owned)
    {
        // Figure out the complete text here
        float fraction = (float)owned / data.steps;
        float percent = fraction * 100;
        float benefit = fraction * data.RewardValueInPercent;


        string formattedBenefit;
        if (Mathf.Approximately(benefit % 1f, 0f)) // integer
        {
            formattedBenefit = benefit.ToString("F0");
        }
        else if (benefit >= 1f) {
            formattedBenefit = benefit.ToString("F2"); // up to 2 decimals
        }
        else {
            // Use significant digits for small numbers
            formattedBenefit = benefit.ToString("G2");
        }


        amtStepsOwned = owned;



        //Debug.Log("*** percent is "+percent+" benefit = "+benefit.ToString("F2"));  

        // If maxed dont show anything
        sliderText.text = (owned == 0) ? "" : formattedBenefit+ (data.isPercent?"%":"");

        slider.value = fraction;
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
    internal void UpdateStats(bool canBuy, BigDouble cost, int amtOwned)
    {
        SetAsNotOwned();

        // Color of Buy Button
        SetAvailableColor(canBuy);

        // Cost of next Upgrade
        upgradeCostText.text = Stats.ReturnAsString(cost);

        if (amtStepsOwned == amtOwned) return;

        // Set the amount text percent
        SetPercentCompleteText(amtOwned);
    }
}
