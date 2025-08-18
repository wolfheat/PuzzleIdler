using System;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI info;
    [SerializeField] private BuyButton buyButton;
    internal void UpdateInfo(UpgradeData data)
    {
        header.text = data.UpgradeName;
        info.text = data.Description;

        buyButton.SetData(data);

    }
}
