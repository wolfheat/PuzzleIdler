using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
using UnityEngine;

public class BuildingDatas : MonoBehaviour
{
	[SerializeField] private BuildingsData[] buildingsDatas;

	public static BuildingDatas Instance { get; private set; }

	public BuildingsData[] BuildingsArray => buildingsDatas;

    // Keeps track of what level the player has upgraded each building to
    public int[] owned;

	private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;

		FillOwned();
	}

    private void Start()
    {
        SavingUtility.LoadingComplete += OnSaveFileLoaded;
    }

    private void OnSaveFileLoaded()
    {
        // Load Buildings From File
        int[] buildingsFromSave = SavingUtility.playerGameData.buildings;

        Debug.Log("");
        Debug.Log("---  LOADING " + buildingsFromSave.Length + " Buildings  ---");

        for (int i = 0; i < buildingsFromSave.Length; i++) {
            owned[i] = buildingsFromSave[i];
            Debug.Log(" Loaded building "+ (i+1) + " => " + owned[i]);            
        }

        // Make sure the visual updates the level
        Buildings.Instance.UpdateLevelNeeded();

        // Make sure the CPS is updated correctly
        OwnedAmountChange(false);
    }

    private void FillOwned()
    {
		owned = new int[buildingsDatas.Length];
		Debug.Log("** Created a building owned array containing "+owned.Length+" Items.");
    }

    private int CalculateRemainingToMilestone(int heldAmount)
    {
        int fifties = (heldAmount / 50 + 1) * 50;
        return heldAmount switch
        {
            0 => 1,
            < 10 => 10 - heldAmount,
            < 25 => 25 - heldAmount,
            _ => 50 - (heldAmount % 50)
        };
    }

    public int GetBuyAmt(int i, UpgradeMode activeMode)
    {
        return activeMode switch
        {
            UpgradeMode.X1 => 1,
            UpgradeMode.X10 => 10,
            UpgradeMode.X25 => 25,
            UpgradeMode.NEXT => CalculateRemainingToMilestone(owned[i]),
            UpgradeMode.MAX => GetMaxAffordableAmount(buildingsDatas[i].baseCost, owned[i]),
            _ => 0
        };
    }

    public static int GetMaxAffordableAmount(BigDouble baseCost, int owned)
    {
        BigDouble gold = Stats.CoinsHeld;

        if (CostMultiplier == 1) {
            // No scaling, just floor(gold / baseCost)
            var amt = (gold / baseCost);
            return (int)amt.Mantissa * (int)Math.Log10(amt.Exponent);
        }
        double k = BigDouble.Log((gold * (CostMultiplier - 1) / (baseCost * BigDouble.Pow(CostMultiplier, owned))) + 1, CostMultiplier);
        return Math.Max(0, (int)Math.Floor(k));
    }

    internal bool BuyBuilding(int index, int amt)
    {
        Debug.Log("Requesting to upgrade index " + index + " - " + amt + " times");

        BuildingsData activeBuilding = buildingsDatas[index];

        // Double check that player can afford before buying
        BigDouble cost = GetCost(index, amt);
                
        if (Stats.CoinsHeld < cost)
            return false;

        Debug.Log("Player can afford this upgrade");

        // Update to new level
        AddLevels(index,amt);

        // Remove cost from player coins
        Stats.RemoveCoins(cost);

        OwnedAmountChange();
        
        return true;
    }


    private void OwnedAmountChange(bool alsoSave = true)
    {
        // Every time amount of buildings change, recalculate how it affects the multipliers in stats?
        Stats.SetBuildingBaseIncome(GetAllBuildingsBaseIncome());

        if (!alsoSave)
            return;

        // Updates Buildings array to save        
        SavingUtility.playerGameData.buildings = owned; // Update Save file with this new info

        // Send save needed event
        SavingUtility.playerGameData.TriggerSave();
    }

    private void AddLevels(int index, int amt) => owned[index] += amt;

    public static double TargetROI = 0.1;
    public static double CostMultiplier = 1.07;
    public static double RewardMultiplier = 1.07;


        
    public BigDouble GetBaseGain(int index, int amt) => buildingsDatas[index].baseCost * TargetROI * amt; // BaseIncome = BaseCost × TargetROI        
    public BigDouble GetGain(int index, int amt) => GetBaseGain(index, amt) * Stats.GetCPSTotalMultiplier();
    public BigDouble GetCost(int index, int amt)
    {
        for (int i = 0; i < buildingsDatas.Length; i++) {

        }


        BigDouble baseCost = buildingsDatas[index].baseCost;
        int ownedAmt = owned[index];
        BigDouble gold = Stats.CoinsHeld;
        // *Bulk buy => TotalCost = BaseCost * Multiplier ^ n * (multiplier ^ k - 1) / (Multiplier - 1)
        if (amt == 1)
            return baseCost * Math.Pow(CostMultiplier, ownedAmt);
        return baseCost * Math.Pow(CostMultiplier, ownedAmt) * (Math.Pow(CostMultiplier, amt) - 1) / (CostMultiplier - 1);
    }

    internal void ResetAll()
    {
        owned = new int[owned.Length];
        Stats.CoinUpdated?.Invoke();

        OwnedAmountChange();
    }

    internal BigDouble GetAllBuildingsBaseIncome()
    {
        BigDouble allBuildingsIncomeSum = 0;
        for (int i = 0; i < owned.Length; i++) {
            int index = owned[i];
            allBuildingsIncomeSum += GetBaseGain(i, owned[i]);
        }
        return allBuildingsIncomeSum;
    }

    public List<BigDouble> GetAllBuildingsBaseIncomeList()
    {
        Debug.Log("** Getting all base incomes when owning " + owned[0]);
        List<BigDouble> incomeList = new List<BigDouble>();
        for (int i = 0; i < owned.Length; i++) {
            incomeList.Add(GetBaseGain(i, owned[i]));
        }
        return incomeList;
    }

    internal List<string> GetAllBuildingsNameList()
    {
        return buildingsDatas.Select(x => x.BuildingName).ToList();
    }
}