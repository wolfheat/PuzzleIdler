using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
using UnityEngine;
public class ResearchDatas : MonoBehaviour
{
	[SerializeField] private List<ResearchData[]> researchDatas;
	[SerializeField] private ResearchData[] Tier1;
	[SerializeField] private ResearchData[] Tier2;
	[SerializeField] private ResearchData[] Tier3;

	public static ResearchDatas Instance { get; private set; }

    public List<ResearchData[]> ResearchList => researchDatas;

    private Dictionary<string, int> dictionaryAmount = new Dictionary<string, int>();
    //private Dictionary<string, ResearchData> dictionaryData = new Dictionary<string, ResearchData>();

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
        SavingUtility.LoadingComplete += OnSaveFileLoaded;
    }

    private void FillDictionary()
    {
        // Place all into the database?

        researchDatas = new List<ResearchData[]>();
        researchDatas.Add(Tier1);
        researchDatas.Add(Tier2);
        researchDatas.Add(Tier3);




        foreach (var tier in researchDatas) {
            foreach (var researchData in tier) {
                dictionaryAmount[researchData.ResearchName] = 0;
                //dictionaryData[researchData.ResearchName] = researchData;
            }
        }
        Debug.Log("Created an research Dictionary containing " + dictionaryAmount.Count + " Items. ");
        foreach (var amt in dictionaryAmount.Keys) {
            Debug.Log("Key: " + amt);
        }
        
    }

    private void OnSaveFileLoaded()
    {
        Debug.Log("ResearchDatas received load from file complete");

        // Handle incomplete load file
        // Handle set values from savefile

        // Should only non null be saved = yes

        //Dictionary<string, int> fromSave = SavingUtility.playerGameData.researches;

        //dictionaryAmount = SavingUtility.playerGameData.researches;

        foreach (var ownedUpgrade in SavingUtility.playerGameData.researches) {

            if (!dictionaryAmount.ContainsKey(ownedUpgrade.Key)) {
                Debug.Log("The upgrade "+ownedUpgrade.Key+" does not exist in the research dictionary list. It is ignored.");
                continue;
            }
            Debug.Log("Loaded resource with name: " + ownedUpgrade.Key);

            // Make sure all these upgrades counts = are activated
            dictionaryAmount[ownedUpgrade.Key] = ownedUpgrade.Value;
        }
        // Handle any update here?

        // Make sure the visual updates the level
        Buildings.Instance.UpdateLevelNeeded();

    }

    internal bool BuyResearch(ResearchData data)
    {
        string researchName = data.ResearchName;

        Debug.Log("Requesting to upgrade research with name: " + researchName);

        // Find it in dictionary?
        if (!dictionaryAmount.ContainsKey(researchName)) {
            Debug.Log("Can not buy resource it does not exist in the dictionary");
            return false;
        }

        //ResearchData data = dictionaryData[researchName];
        int ownedAmt = dictionaryAmount[researchName];


        // Double check that player can afford before buying
        BigDouble cost = GetCost(data,ownedAmt);

        if (Stats.CoinsHeld < cost)
            return false;

        Debug.Log("Player can afford this upgrade");

        // Add the benefit

        Debug.Log("BENEFIT ADD HERE");

        // Remove cost from player coins
        Stats.RemoveCoins(cost);

        // Add one to owned
        dictionaryAmount[researchName]++;

        // Updates Research array to save        
        SavingUtility.playerGameData.researches = (Dictionary<string, int>) dictionaryAmount.Where(x => x.Value != 0);

        // Send save needed event
        SavingUtility.playerGameData.TriggerSave();

        return true;
    }

    public BigDouble GetCost(ResearchData data, int ownAmt)
    {
        BigDouble baseCost = new BigDouble(data.baseCost.Mantissa,data.baseCost.Exponent);
        BigDouble finalCost = baseCost * BigDouble.Pow(data.costIncrementMultiplier, ownAmt);
        return finalCost;
    }

    internal BigDouble GetCost(ResearchData data)
    {
        if (dictionaryAmount.ContainsKey(data.ResearchName))
            return GetCost(data, dictionaryAmount[data.ResearchName]);

        Debug.Log("No research upgrade found with the name of "+ data.ResearchName+" in the dictionary amount. DictionaryAmt contains "+dictionaryAmount.Keys.Count);
        return new BigDouble();
    }

    internal (int,bool) GetAmtOwned(ResearchData data)
    {
        if (dictionaryAmount.ContainsKey(data.ResearchName)) {
            int amtOwned = dictionaryAmount[data.ResearchName];
            return (amtOwned, amtOwned == data.steps);
        }
        return (0,false);
    }
}
