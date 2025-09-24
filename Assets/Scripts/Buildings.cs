using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;

public enum UpgradeMode{X1,X10,X25,NEXT,MAX}



public class BuildingsSaveData : MonoBehaviour
{
        
}

public class Buildings : MonoBehaviour
{
    [SerializeField] private GameObject panel;
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
        // React to changed coin amount
        Stats.CoinUpdated += UpdateLevelNeeded;
        // Stats.CoinUpdated += () => { doUpdate = true; };

        // Start by also doing one of the updates
        UpdateAvailabilityAndStats();
    }

    private bool doUpdate = false;
    private bool doUpdateLevel = false;
    private void UpdateNeeded() => doUpdate = true;
    public void UpdateLevelNeeded() => doUpdateLevel = true;

    private void LateUpdate()
    {
        if (doUpdate || doUpdateLevel) {
            UpdateAvailabilityAndStats(doUpdateLevel);
        }
        doUpdate = false;
        doUpdateLevel = false;
    }

    private void UpdateAvailabilityAndStats(bool includeLevel = false)
    {
        for (int i = 0; i < buildings.Count; i++) {
            BuildingsItem item = buildings[i];
            int buyAmt = Math.Max(1,BuildingDatas.Instance.GetBuyAmt(i,ActiveUpgradeMode));// Minimum to buy will allways be 1

            BigDouble cost = BuildingDatas.Instance.GetCost(i, buyAmt);
            bool canBuy = Stats.CoinsHeld >= cost;
            BigDouble gain = BuildingDatas.Instance.GetGain(i, buyAmt);
            item.UpdateStats(canBuy,buyAmt,cost,gain);
            if (includeLevel)
                item.UpdateLevelText();
        }
    }


    private void GenerateBuildingsList()
    {
        Debug.Log("Generate the buildings list");

        //Clear old items
        foreach(Transform child in buildingsParent.transform) {
            Destroy(child.gameObject);
        }

        BuildingsData[] buildingDatas = BuildingDatas.Instance.BuildingsArray;

        // By generating the buildings items a reference to this class can be sent in and receive any click info from that item
        for (int i = 0; i < buildingDatas.Length; i++) {
            Debug.Log("Creating a building "+(i+1));
            // Reset Level
            BuildingsItem item = Instantiate(buildingsItemPrefab,buildingsParent.transform);
            item.SetAllInfo(buildingDatas[i],i);
            buildings.Add(item);
        }
    }



    public void BuyX(int x)
    {
        Debug.Log("Player pressed buy X = "+x);

        UpgradeMode newUpgradeMode = x switch{
            1  => UpgradeMode.X1,
            10 => UpgradeMode.X10,
            25 => UpgradeMode.X25,
            _   => UpgradeMode.X1
        };
        if(ActiveUpgradeMode != newUpgradeMode) {
            ActiveUpgradeMode = newUpgradeMode;
            Stats.CoinUpdated?.Invoke();
        }

    }
    
    public void BuyNEXT()
    {
        Debug.Log("Player pressed buy NEXT");
        if (ActiveUpgradeMode != UpgradeMode.NEXT) {
            ActiveUpgradeMode = UpgradeMode.NEXT;
            Stats.CoinUpdated?.Invoke();
        }
    }
    
    public void BuyMAX()
    {
        Debug.Log("Player pressed buy MAX");
        if(ActiveUpgradeMode != UpgradeMode.MAX) {
            ActiveUpgradeMode = UpgradeMode.MAX;
            Stats.CoinUpdated?.Invoke();
        }
    }
    
    public void BuyALL()
    {
        Debug.Log("Player pressed buy ALL");
        // Dont set a new active upgrade mode, just buy them all
    }

    public void UpdateAllLevels()
    {

    }

    internal void RequestUpgrade(int index, int amt)
    {
        // Send this to be handeled by the BuildingDatas
        bool levelsChanged = BuildingDatas.Instance.BuyBuilding(index,amt);

        // Need to update all if coins are changed - not just this one
        if (levelsChanged) {
            doUpdateLevel = true;
        }
    }
}
