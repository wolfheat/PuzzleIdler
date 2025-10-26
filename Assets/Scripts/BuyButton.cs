using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI BuyText;
    [SerializeField] private GameObject BuyTextObject;
    [SerializeField] private GameObject buyButtonImageObject;

    [SerializeField] private Color ownedColor;
    [SerializeField] private Color affordColor;
    [SerializeField] private Color notAffordColor;
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
        //Checkmark.SetActive(owned);
        // Owned items hides the buy button entirely and shows a separate payed cost text instead?
        buyButtonImageObject.SetActive(!owned);

        BuyTextObject.gameObject.SetActive(!owned);
        
        // Color of text
        BuyText.color = owned ? ownedTextColor : defaultTextColor;

        // Color of buy button if affording
        // If can buy make green
        
        imageBackground.color = owned ? ownedColor : notAffordColor;
    }

    public void SetAfford(bool afford)
    {
        Debug.Log(afford ? "Afford Color":"Not afford color");
        imageBackground.color = afford ? affordColor : notAffordColor;
    }

    internal void SetData(UpgradeData data, bool owned)
    {
        Debug.Log("Setting Button Data Owned: "+ owned+" afford: "+ (Stats.GemsHeld > data.cost));
        
        // Update Cost 
        BuyText.text = Stats.ReturnAsString(data.cost);
        
        // Change to correct Button
        Owned(owned);

        // Change color from afford or not
        SetAfford(!owned && (Stats.GemsHeld > data.cost));
    }

    public void ClickedBuy()
    {
        Debug.Log("Player tries to buy this upgrade");
        Upgrades.Instance.RequestBuyUpgrade();
    }
}
