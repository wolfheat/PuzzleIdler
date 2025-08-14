using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;
using static UnityEditor.Progress;

public enum UpgradeMode{X1,X10,X25,NEXT,MAX}

public class Buildings : MonoBehaviour
{
    [SerializeField] private BuildingsItem buildingsItemPrefab;
    [SerializeField] private GameObject buildingsParent;
    [SerializeField] private List<BuildingsItem> buildings = new();

    public UpgradeMode ActiveUpgradeMode { get; set; } = UpgradeMode.X1;
    
    public static Buildings Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void Start()
    {
        GenerateBuildingsList();
        //React to changed coin amount
        Stats.CoinUpdated += UpdateNeeded;

        UpdateAvailabilityAndStats();
    }

    private bool doUpdate = false;
    private void UpdateNeeded() => doUpdate = true;
    private void LateUpdate()
    {
        if (doUpdate) {
            UpdateAvailabilityAndStats();
        }
        doUpdate = false;
    }

    private void UpdateAvailabilityAndStats()
    {
        for (int i = 0; i < buildings.Count; i++) {
            BuildingsItem item = buildings[i];
            int buyAmt = GetBuyAmt(i);            
            BigDouble cost = GetCost(item.Data.baseCost, item.Data.Level, buyAmt);
            bool canBuy = Stats.CoinsHeld >= cost;
            BigDouble gain = GetGain(item.Data.baseCost, item.Data.Level, buyAmt);
            item.UpdateStats(canBuy,buyAmt,cost,gain);
        }
    }

    private int GetBuyAmt(int i)
    {
        return ActiveUpgradeMode switch
        {
            UpgradeMode.X1 => 1,
            UpgradeMode.X10 => 10,
            UpgradeMode.X25 => 25,
            UpgradeMode.NEXT => CalculateRemainingToMilestone(buildings[i].Data.Level),
            UpgradeMode.MAX => GetMaxAffordableAmount(buildings[i].Data.baseCost, buildings[i].Data.Level),
            _ => 0
        };
    }

    private int CalculateRemainingToMilestone(int heldAmount)
    {
        int fifties = (heldAmount / 50 + 1) * 50;
        return heldAmount switch
        {
            0   => 1,
            <10 => 10-heldAmount,
            <25 => 25-heldAmount,
            _   => 50 - (heldAmount%50)
        };
    }

    public static double TargetROI = 0.1;
    public static double CostMultiplier = 1.07;
    public static double RewardMultiplier = 1.07;


    public static BigDouble GetGain(BigDouble baseCost,int owned, int amt)
    {
        // BaseIncome = BaseCost × TargetROI
        // calculation for linear static income

        BigDouble gain = baseCost * TargetROI * amt *  Stats.AllBuildingGainMultipliers();
        return gain;
    }
    
    public BigDouble GetCost(BigDouble baseCost,int owned,int amt)
    {
        BigDouble gold = Stats.CoinsHeld;
        // *Bulk buy => TotalCost = BaseCost * Multiplier ^ n * (multiplier ^ k - 1) / (Multiplier - 1)
        if(amt == 1)
            return baseCost * Math.Pow(CostMultiplier, owned);
        return baseCost * Math.Pow(CostMultiplier, owned) * (Math.Pow(CostMultiplier, amt)-1)/(CostMultiplier -1);
    }
    
    public static int GetMaxAffordableAmount(BigDouble baseCost,int owned)
    {
        BigDouble gold = Stats.CoinsHeld;

        if (CostMultiplier == 1) {
            // No scaling, just floor(gold / baseCost)
            var amt = (gold / baseCost);
            return (int)amt.Mantissa*(int)Math.Log10(amt.Exponent);
        }
        double k = BigDouble.Log((gold * (CostMultiplier - 1) / (baseCost * BigDouble.Pow(CostMultiplier, owned))) + 1, CostMultiplier);
        return Math.Max(0, (int)Math.Floor(k));
    }


    private void GenerateBuildingsList()
    {
        Debug.Log("Generate the buildings list");

        //Clear old items
        foreach(Transform child in buildingsParent.transform) {
            Destroy(child.gameObject);
        }

        BuildingsData[] buildingDatas = BuildingDatas.Instance.Buildings;

        // By generating the buildings items a reference to this class can be sent in and receive any click info from that item
        for (int i = 0; i < buildingDatas.Length; i++) {
            Debug.Log("Creating a building "+(i+1));
            BuildingsItem item = Instantiate(buildingsItemPrefab,buildingsParent.transform);
            item.SetAllInfo(buildingDatas[i],i);
            buildings.Add(item);
        }
    }



    public void BuyX(int x)
    {
        Debug.Log("Player pressed buy X = "+x);

        ActiveUpgradeMode = x switch{
            1  => UpgradeMode.X1,
            10 => UpgradeMode.X10,
            25 => UpgradeMode.X25,
            _   => UpgradeMode.X1
        };
    }
    
    public void BuyNEXT()
    {
        Debug.Log("Player pressed buy NEXT");
        ActiveUpgradeMode = UpgradeMode.NEXT;
    }
    
    public void BuyMAX()
    {
        Debug.Log("Player pressed buy MAX");
        ActiveUpgradeMode = UpgradeMode.MAX;
    }
    
    public void BuyALL()
    {
        Debug.Log("Player pressed buy ALL");
        // Dont set a new active upgrade mode, just buy them all
    }

    internal void RequestUpgrade(int index, int amt)
    {
        Debug.Log("Requesting to upgrade index "+index+" - "+amt+ " times");

        // Double check that player can afford before buying
        BigDouble cost = GetCost(buildings[index].Data.baseCost, buildings[index].Data.Level, amt);
        BigDouble gain = GetGain(buildings[index].Data.baseCost, buildings[index].Data.Level, amt);


        if (Stats.CoinsHeld < cost)
            return;

        Debug.Log("Player can afford this upgrade");

        // Update to new level
        buildings[index].AddLevels(amt);

        // Remove cost from player coins
        Stats.RemoveCoins(cost);
        Stats.AddCPS(gain);

    }
}
