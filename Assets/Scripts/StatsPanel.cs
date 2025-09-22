using System;
using System.Collections;
using System.Collections.Generic;
using BreakInfinity;
using TMPro;
using UnityEngine;

public class StatsPanel : MonoBehaviour
{

    [SerializeField] private GameObject gamesTextHolder;
    [SerializeField] private GameObject buildingTextHolder;
    [SerializeField] private GameObject researchTextHolder;
    [SerializeField] private GameObject upgradesTextHolder;
    [SerializeField] private BuildingsListItem listItemPrefab;

    [SerializeField] private TextMeshProUGUI gamesSumTextfield;
    [SerializeField] private TextMeshProUGUI buildingSumTextfield;
    [SerializeField] private TextMeshProUGUI researchSumTextfield;
    [SerializeField] private TextMeshProUGUI upgradesSumTextfield;


    public static StatsPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SavingUtility.LoadingComplete += OnStatsLoaded;

    }

    private void OnStatsLoaded()
    {
        Debug.Log("** LOADING COMPLETE - StatsPanel");
        DelayedUpdate();
    }

    void OnEnable() => StartCoroutine(DelayedUpdate());

    private IEnumerator DelayedUpdate()
    {
        yield return null;
        yield return null;
        yield return null;
        // Update the list
        UpdateGamesListValues();
        UpdateBuildingListValues();
        UpdateResearchListValues();
        UpdateUpgradesListValues();
    }

    public void UpdateGamesListValues()
    {
        // Get list from data
        float[] miniGames = Stats.MiniGamesMultipliers;
        string[] names = Enum.GetNames(typeof(MiniGameNames));


        foreach (Transform child in gamesTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        float product = 1;

        for (int i = 0; i < miniGames.Length; i++) {
            float income = miniGames[i];

            BuildingsListItem item = Instantiate(listItemPrefab, gamesTextHolder.transform);

            item.SetName(names[i]);
            item.SetValue(Stats.ReturnAsString(income));
            product *= income;
        }
        gamesSumTextfield.text = Stats.ReturnAsString(product);
    }
    public void UpdateUpgradesListValues()
    {
        // Get list from data
        (List<BigDouble> incomeList, List<string> researchNames) = UpgradeDatas.Instance.GetAllResearchCPSList();

        Debug.Log("** STATSPANEL - Upgrades - Updating Upgrade List "+incomeList.Count+" items");

        foreach (Transform child in upgradesTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        BigDouble product = 1;

        for (int i = 0; i < incomeList.Count; i++) {
            BigDouble income = incomeList[i];
            BuildingsListItem item = Instantiate(listItemPrefab, upgradesTextHolder.transform);
            item.SetName(researchNames[i]);
            item.SetValue(Stats.ReturnAsString(income));
            product *= income;
        }
        upgradesSumTextfield.text = Stats.ReturnAsString(product);
    }
    public void UpdateResearchListValues()
    {
        // Get list from data
        (List<BigDouble> incomeList, List<string> researchNames) = ResearchDatas.Instance.GetAllResearchCPSList();

        Debug.Log("** STATSPANEL - Research - ** Updating List " + incomeList.Count+" items");

        foreach (Transform child in researchTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        BigDouble product = 1;

        for (int i = 0; i < incomeList.Count; i++) {
            BigDouble income = incomeList[i];
            BuildingsListItem item = Instantiate(listItemPrefab, researchTextHolder.transform);
            item.SetName(researchNames[i]);
            item.SetValue(Stats.ReturnAsString(income));
            product *= income;
        }
        researchSumTextfield.text = Stats.ReturnAsString(product);
    }
    public void UpdateBuildingListValues()
    {
        // Get list from data
        List<BigDouble> incomeList = BuildingDatas.Instance.GetAllBuildingsBaseIncomeList();
        List<string> buildingNames = BuildingDatas.Instance.GetAllBuildingsNameList();

        Debug.Log("** STATSPANEL - Buildings - ** Updating List " + incomeList.Count+" items");

        foreach (Transform child in buildingTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        BigDouble sum = 0;

        for (int i = 0; i < incomeList.Count; i++) {
            BigDouble income = incomeList[i];
            BuildingsListItem item = Instantiate(listItemPrefab, buildingTextHolder.transform);
            item.SetName(buildingNames[i]);
            item.SetValue(Stats.ReturnAsString(income));
            sum += income;
        }
        buildingSumTextfield.text = Stats.ReturnAsString(sum);
    }
}
