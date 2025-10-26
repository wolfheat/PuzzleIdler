using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;
using UnityEngine.UI;

public class Upgrades : MonoBehaviour
{
	[SerializeField] private InfoPanel infoPanel; 
	[SerializeField] private Transform upgradeHolder; 
	[SerializeField] private GameObject panel; 
	[SerializeField] private GameObject selectionBorder; 
	public static Upgrades Instance { get; private set; }

	private UpgradeData selectedData;
	private UpgradeButton selectedButton = null;
		


    private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

    private void OnEnable()
    {
		// Listen for gems held change
		Stats.HeldGemsUpdated += UpdateAffordActive;
    }
	
    private void OnDisable()
    {
		Stats.HeldGemsUpdated -= UpdateAffordActive;
    }

    private void UpdateAffordActive() => UpdateInfoPanel(selectedButton);

    internal void UpdateOwned(List<string> strings)
    {
		Debug.Log(" -** Updating Owned Upgrades so they show in correct colors");
		UpgradeButton[] buttons = upgradeHolder.GetComponentsInChildren<UpgradeButton>();
		Debug.Log(" -** Buttons amount "+buttons.Length);

        foreach (UpgradeButton button in buttons) {
			foreach (string name in strings) {
				Debug.Log("Name: "+name+" compare to "+button.Data.UpgradeName);
				if(button.Data.UpgradeName == name) {
					Debug.Log("Found " +button.Data.UpgradeName);
					button.SetAsOwned(true);
					continue;
				}
			}
		}
    }

    internal void UpdateInfoPanel(UpgradeButton button)
    {
		if (button == null) return;

		bool owned = UpgradeDatas.Instance.Owns(button.Data);
		infoPanel?.UpdateInfo(button.Data, owned);
		selectedData = button.Data;
        selectedButton = button;

        // also set the button
        selectionBorder.transform.localPosition = button.transform.localPosition + new Vector3(button.transform.parent.GetComponent<HorizontalLayoutGroup>().padding.left,0,0);
    }

    internal void RequestBuyUpgrade()
    {
        Debug.Log("Upgrades - Player tries to buy active upgrade: "+selectedData.UpgradeName);

        // Send this task to the UpgradeDatas?

        UpgradeDatas.Instance.BuyUpgrade(selectedData);


    }


}
