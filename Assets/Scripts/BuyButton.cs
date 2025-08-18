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
    [SerializeField] private Color notOwnedColor;
    [SerializeField] private Image imageBackground;

    void Start()
    {
        // Show As not Owned as default
        Owned(true);
    }

    private void Owned(bool owned)
    {
        Checkmark.SetActive(owned);
        BuyTextObject.gameObject.SetActive(!owned);
        imageBackground.color = owned ? ownedColor : notOwnedColor;
    }

    public void SetCost(string costString)
    {
        Owned(false);
        BuyText.text = costString;
    }

    internal void SetData(UpgradeData data)
    {
        if (data.unlocked) {
            Owned(true);
            return;
        }
        SetCost(Stats.ReturnAsString(data.cost));
    }

    public void ClickedBuy()
    {
        Debug.Log("Player tries to buy this upgrade");
        Upgrades.Instance.RequestBuyUpgrade();
    }
}
