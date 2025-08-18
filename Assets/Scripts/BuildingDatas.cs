using System;
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
        Debug.Log("BuildingDatas received load from file complete");
        // Handle incomplete load file
        // Handle set values from savefile
        int[] buildingsFromSave = SavingUtility.playerGameData.buildings;

        Debug.Log("buildings from save amt = "+buildingsFromSave.Length);


        for (int i = 0; i < buildingsFromSave.Length; i++) {
            owned[i] = buildingsFromSave[i];
            Debug.Log("loaded "+ owned[i]+" for building type: "+i);
            // Calculate how much the player benefit from the upgrades and apply that value
            AddBuildingIncome(i, owned[i]);
        }


        // Make sure the visual updates the level
        Buildings.Instance.UpdateLevelNeeded();

    }

    private void FillOwned()
    {
		owned = new int[buildingsDatas.Length];
		Debug.Log("Created a building owned array containing "+owned.Length+" Items.");
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
        BigDouble gain = GetGain(index, amt);

        if (Stats.CoinsHeld < cost)
            return false;

        Debug.Log("Player can afford this upgrade");

        // Update to new level
        AddLevels(index,amt);

        // Remove cost from player coins
        Stats.RemoveCoins(cost);
        Stats.AddCPS(gain);

        // Updates Buildings array to save        
        SavingUtility.playerGameData.buildings = owned; // Update Save file with this new info

        // Send save needed event
        PlayerGameData.SaveNeeded?.Invoke();

        return true;
    }
    internal void AddBuildingIncome(int index, int amt)
    {
        Debug.Log("Requesting to set index " + index + " - " + amt + " times");

        // Double check that player can afford before buying
        BigDouble fullGain = GetGain(index, amt);

        // Update to new level
        //AddLevels(index,amt);

        // Remove cost from player coins
        Stats.AddCPS(fullGain);
    }

    private void AddLevels(int index, int amt) => owned[index] += amt;

    public static double TargetROI = 0.1;
    public static double CostMultiplier = 1.07;
    public static double RewardMultiplier = 1.07;


    public BigDouble GetGain(int index, int amt)//(BigDouble baseCost, int owned, int amt)
    {
        BigDouble baseCost = buildingsDatas[index].baseCost;
        // BaseIncome = BaseCost × TargetROI
        // calculation for linear static income

        BigDouble gain = baseCost * TargetROI * amt * Stats.AllBuildingGainMultipliers();
        return gain;
    }

    public BigDouble GetCost(int index, int amt)
    {
        BigDouble baseCost = buildingsDatas[index].baseCost;
        int ownedAmt = owned[index];
        BigDouble gold = Stats.CoinsHeld;
        // *Bulk buy => TotalCost = BaseCost * Multiplier ^ n * (multiplier ^ k - 1) / (Multiplier - 1)
        if (amt == 1)
            return baseCost * Math.Pow(CostMultiplier, ownedAmt);
        return baseCost * Math.Pow(CostMultiplier, ownedAmt) * (Math.Pow(CostMultiplier, amt) - 1) / (CostMultiplier - 1);
    }

}
