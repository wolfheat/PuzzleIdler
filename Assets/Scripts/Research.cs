using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;

public class Research : MonoBehaviour
{
    public static Research Instance { get; private set; }

    [SerializeField] private List<ResearchItem> researches = new();
    [SerializeField] private ResearchItem researchItemPrefab;
    [SerializeField] private ResearchTierItem tierHeaderPrefab;
    [SerializeField] private GameObject researchParent;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        GenerateResearchList();
        // React to changed coin amount
        Stats.CoinUpdated += UpdateNeeded;
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
        // Here all owned amounts are set for each research, just update teh values

        for (int i = 0; i < researches.Count; i++) {
            ResearchItem researchItem = researches[i];
            
            // Only care about any cost and affordability if this isnt maxed out

            (int amtOwned, bool allOwned) = ResearchDatas.Instance.GetAmtOwned(researchItem.Data);

            if (allOwned) {
                researchItem.SetAsOwned();
                continue;
            }

            BigDouble cost = ResearchDatas.Instance.GetCost(researchItem.Data);
            bool canBuy = Stats.CoinsHeld >= cost;

            researchItem.UpdateStats(canBuy, cost);
        }
    }

    private void GenerateResearchList()
    {
        Debug.Log("Generate the buildings list");

        //Clear old items
        foreach (Transform child in researchParent.transform) {
            Destroy(child.gameObject);
        }

        List<ResearchData[]> researchDatas = ResearchDatas.Instance.ResearchList;

        for (int j = 0; j < researchDatas.Count; j++) {
            ResearchData[] researchTier = researchDatas[j];
            // Create a header
            ResearchTierItem tierItem = Instantiate(tierHeaderPrefab, researchParent.transform);
            tierItem.SetTierInfo(j);

            // By generating the buildings items a reference to this class can be sent in and receive any click info from that item
            for (int i = 0; i < researchTier.Length; i++) {
                Debug.Log("Creating a building " + (i + 1));
                // Reset Level
                ResearchItem item = Instantiate(researchItemPrefab, researchParent.transform);
                item.SetAllInfo(researchTier[i], i);
                researches.Add(item);
            }
        }
    }


    internal void RequestBuyUpgrade(ResearchData data)
    {
        Debug.Log("Research - Player tries to buy research: " + data.ResearchName);

        // Send this task to the UpgradeDatas?
        ResearchDatas.Instance.BuyResearch(data);
    }

    internal void SetUpAllResearchItems()
    {
        throw new NotImplementedException();
    }
}
