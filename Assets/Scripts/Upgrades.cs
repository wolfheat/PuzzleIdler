using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
	[SerializeField] private InfoPanel infoPanel; 
	[SerializeField] private Transform upgradeHolder; 
	[SerializeField] private GameObject panel; 
	public static Upgrades Instance { get; private set; }

	private UpgradeData selectedData;
		


    private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	public void SetSelected(UpgradeData data)
	{
		selectedData = data;

		// Show this info on the info screen

	}

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






    internal void UpdateInfoPanel(UpgradeData data)
    {
		bool owned = UpgradeDatas.Instance.Owns(data);
		infoPanel?.UpdateInfo(data, owned);
		selectedData = data;

		// also set the button

    }

    internal void RequestBuyUpgrade()
    {
        Debug.Log("Upgrades - Player tries to buy active upgrade: "+selectedData.UpgradeName);

        // Send this task to the UpgradeDatas?

        UpgradeDatas.Instance.BuyUpgrade(selectedData);


    }


}
