using BreakInfinity;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
	[SerializeField] private InfoPanel infoPanel; 
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

    internal void UpdateInfoPanel(UpgradeData data)
    {
		infoPanel?.UpdateInfo(data);
		selectedData = data;
    }

    internal void RequestBuyUpgrade()
    {
        Debug.Log("Upgrades - Player tries to buy active upgrade: "+selectedData.UpgradeName);

        // Send this task to the UpgradeDatas?

        UpgradeDatas.Instance.BuyUpgrade(selectedData);


    }


}
