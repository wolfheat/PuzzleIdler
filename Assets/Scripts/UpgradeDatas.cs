using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
using UnityEngine;
using WolfheatProductions.SoundMaster;

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

        SavingUtility.LoadingComplete += OnSaveFileLoaded;
    }

    private void OnSaveFileLoaded()
    {
        // Load Upgrades From File

        //dictionary = SavingUtility.playerGameData.upgrades;

        Debug.Log("");
        Debug.Log("---  LOADING " + dictionary.Count+ " Upgrades  ---");

        foreach (var upgrade in SavingUtility.playerGameData.upgrades) {

            if (!dictionary.ContainsKey(upgrade.Key)) continue;

            dictionary[upgrade.Key] = upgrade.Value;
            Debug.Log(" Loaded upgrade: " + upgrade);
            // Make sure all these upgrades counts = are activated
        }

        // Update the stats
        UpdateStatsValues();

        // Activates bought items on Upgrades screen
        Upgrades.Instance.UpdateOwned(dictionary.Where(x => x.Value == true).Select(x => x.Key).ToList());

        // Make sure the buildings panel updates
        Buildings.Instance.UpdateLevelNeeded();
    }

    private void UpdateStatsValues()
    {
        Debug.Log("**- STATS values update");
        UpdateCoinMultiplierValue();
        UpdateGemMultiplierValue();
    }

    private void FillDictionary()
    {
        foreach (var data in upgradeDatas) {
            dictionary[data.UpgradeName] = false;
            Debug.Log("*-* Filled Upgrade: "+data.UpgradeName);
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
        if (dictionary[data.UpgradeName] == true) {
			Debug.Log("Can not unlock this upgrade, already Owned");
			return;
		}

        // make owned
        dictionary[data.UpgradeName] = true;

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
        foreach (UpgradeData data in coinIncomeDatas) {
            if(coinIncomeDatas.Contains(data) && dictionary.ContainsKey(data.UpgradeName) && dictionary[data.UpgradeName] == true)
                coinIncomeMultiplier *= data.UpgradeValue;            
        }
        Stats.SetCPSUpgradeMultiplier(coinIncomeMultiplier);
    }

    public void UpdateGemMultiplierValue()
    {
        BigDouble gemIncomeMultiplier = 1;
        foreach (UpgradeData data in gemIncomeDatas) {
            if(coinIncomeDatas.Contains(data) && dictionary.ContainsKey(data.UpgradeName) && dictionary[data.UpgradeName] == true)
                gemIncomeMultiplier *= data.UpgradeValue;
        }
        Stats.SetGemMultiplier(gemIncomeMultiplier);
    }

    internal void BuyUpgrade(UpgradeData selectedData)
    {
        // Double check that player can afford before buying
        BigDouble cost = selectedData.cost;

        if (Stats.GemsHeld < cost) {
            SoundMaster.Instance.PlaySound(SoundName.BuyFailSound);
            Debug.Log("Upgrades - Player Can not afford, play fail sound" + selectedData.UpgradeName);

            return;
        }

        Debug.Log("Player can afford this upgrade");


        if (!dictionary.ContainsKey(selectedData.UpgradeName)) {
            Debug.Log("The name " + selectedData.UpgradeName + " does not exist in the upgradesOwned dictionary.");
            Debug.Log("Dictionary size:  " + dictionary.Count);

            foreach (var item in dictionary.Keys) {
                Debug.Log("Dictionary item: "+item);
            }

            return;
        }
        if (dictionary[selectedData.UpgradeName] == true) {
            Debug.Log("Upgrade " + selectedData.UpgradeName + " already owned.");
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
        else if (selectedData.type == UpgradeType.GemBoosters) {
            UpdateGemMultiplierValue();
        }

        // Updates Upgrades dictionary to save
        // Dont save all dictionary... check how it is done in research

        SavingUtility.playerGameData.upgrades = dictionary;

        // Send save needed event
        SavingUtility.playerGameData.TriggerSave();
    }

    internal (List<float> incomeList, List<string> researchNames) GetAllResearchCPSList()
    {
        List<float> list = new();
        List<string> names = dictionary.Keys.ToList();

        foreach (var data in upgradeDatas) {
            string name = data.UpgradeName;
            if (!dictionary.ContainsKey(name) || dictionary[data.UpgradeName]==false)
                continue; 
            list.Add(data.UpgradeValue);
            names.Add(name);
        }
        return (list, names);
    }

    internal bool Owns(UpgradeData data) => dictionary.ContainsKey(data.UpgradeName) && dictionary[data.UpgradeName];
}
