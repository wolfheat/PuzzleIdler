using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyButton : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI BuyText;
    [SerializeField] private GameObject Checkmark;
    [SerializeField] private GameObject BuyTextObject;

    [SerializeField] private Color ownedColor;
    [SerializeField] private Color affordColor;
    [SerializeField] private Color notOwnedColor;
    [SerializeField] private Color ownedTextColor;
    [SerializeField] private Color defaultTextColor;
    [SerializeField] private Image imageBackground;

    void Start()
    {
        // Show As not Owned as default
        Owned(false);
    }

    private void Owned(bool owned)
    {
        Checkmark.SetActive(owned);
        BuyTextObject.gameObject.SetActive(!owned);
        BuyText.color = owned ? ownedTextColor : defaultTextColor;
        imageBackground.color = owned ? ownedColor : notOwnedColor;
    }

    public void SetCost(string costString)
    {
        Owned(false);
        BuyText.text = costString;
    }
    
    public void SetAfford()
    {
        imageBackground.color = affordColor;
    }

    internal void SetData(UpgradeData data, bool owned)
    {
        Debug.Log("Setting Button Data Owned: "+ owned+" afford: "+ (Stats.GemsHeld > data.cost));
        BuyText.text = Stats.ReturnAsString(data.cost);
        
        Owned(owned);

        // If can buy make green
        if(!owned && (Stats.GemsHeld > data.cost))
            SetAfford();
    }

    public void ClickedBuy()
    {
        Debug.Log("Player tries to buy this upgrade");
        Upgrades.Instance.RequestBuyUpgrade();
    }
}
