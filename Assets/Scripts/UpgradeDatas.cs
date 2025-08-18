using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
using UnityEngine;

public class UpgradeDatas : MonoBehaviour
{
	[SerializeField] private UpgradeData[] upgradeDatas;
	[SerializeField] private List<UpgradeData> coinIncomeDatas;
	[SerializeField] private List<UpgradeData> gemIncomeDatas;

	[SerializeField] private Sprite missingSprite;
    public Sprite MissingSprite => missingSprite;
    public static UpgradeDatas Instance { get; private set; }

    private Dictionary<string , bool> dictionary = new Dictionary<string , bool>();

    private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;

        FillDictionary();
    }

    private void Start()
    {
        CreateIncomeLists();

        UpdateCoinMultiplierValue();
        UpdateGemMultiplierValue();

        // Write these numbers to Stats?
    }

    private void FillDictionary()
    {
        foreach (var data in upgradeDatas) {
            dictionary[data.UpgradeName] = false;
        }
        Debug.Log("Created an upgrade Dictionary containing " + dictionary.Count + " Items.");
    }

    public void UnlockUpgrade(UpgradeData data)
    {
        // Every time a multiplier is unlocked, recalculate the new multiplier sum
        if (!upgradeDatas.Contains(data)) {
			Debug.Log("Can not ulock this upgrade, can not find it in the upgrade list");
			return;
		}
		data.unlocked = true;

		if (data.type == UpgradeType.IncomeBoosters)
			UpdateCoinMultiplierValue();
        else if(data.type == UpgradeType.GemBoosters)
            UpdateGemMultiplierValue();
    }

    private void CreateIncomeLists()
    {
        foreach (UpgradeData upgradeData in upgradeDatas) {
            if (upgradeData.type == UpgradeType.IncomeBoosters) {
                coinIncomeDatas.Add(upgradeData);
            }
            else if (upgradeData.type == UpgradeType.GemBoosters) {
                gemIncomeDatas.Add(upgradeData);
            }
        }
    }

    public void UpdateCoinMultiplierValue()
    {
        BigDouble coinIncomeMultiplier = 1;
        foreach (UpgradeData upgradeData in coinIncomeDatas) {
            if(upgradeData.unlocked)
                coinIncomeMultiplier *= upgradeData.value;            
        }
        Stats.SetCoinMultiplier(coinIncomeMultiplier);
    }

    public void UpdateGemMultiplierValue()
    {
        BigDouble gemIncomeMultiplier = 1;
        foreach (UpgradeData upgradeData in gemIncomeDatas) {
            if (upgradeData.unlocked)
                gemIncomeMultiplier *= upgradeData.value;
        }
        Stats.SetGemMultiplier(gemIncomeMultiplier);
    }

    internal void BuyUpgrade(UpgradeData selectedData)
    {
        // Double check that player can afford before buying
        BigDouble cost = selectedData.cost;

        if (Stats.GemsHeld < cost)
            return;

        Debug.Log("Player can afford this upgrade");


        if (!dictionary.ContainsKey(selectedData.UpgradeName)) {
            Debug.Log("The name " + selectedData.UpgradeName + " does not exist in the upgradesOwned dictionary.");
            return;
        }
        if (dictionary[selectedData.UpgradeName] == true) {
            Debug.Log("Upgrade " + selectedData.UpgradeName + " allready owned.");
            return;
        }

        // Set As Owned
        dictionary[selectedData.UpgradeName] = true;

        // Remove cost from player coins
        Stats.RemoveGems(cost);

        // Activate the Benefit
        if (selectedData.type == UpgradeType.IncomeBoosters) {
            UpdateCoinMultiplierValue();
        }
        else if (selectedData.type == UpgradeType.IncomeBoosters) {
            UpdateGemMultiplierValue();
        }

        SavingUtility.playerGameData.upgrades = dictionary;

        // Send save needed event
        PlayerGameData.SaveNeeded?.Invoke();
    }
}
