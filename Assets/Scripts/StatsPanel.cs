using System;
using System.Collections;
using System.Collections.Generic;
using BreakInfinity;
using TMPro;
using Unity.VisualScripting;
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

    [Header("Final")]
    [SerializeField] private TextMeshProUGUI finalTotMultiplierTextField;
    [SerializeField] private TextMeshProUGUI finalTotIncomeTextField;


    public static StatsPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SavingUtility.LoadingComplete += OnStatsLoaded;



        Stats.CPSUpdated += UpdateInfo;
}

    private void OnStatsLoaded()
    {
        Debug.Log("** LOADING COMPLETE - StatsPanel");
        StartCoroutine(DelayedUpdate());
    }

    void OnEnable() => StartCoroutine(DelayedUpdate());

    private IEnumerator DelayedUpdate()
    {
        yield return null;
        yield return null;
        yield return null;

        UpdateInfo();
    }

    private void UpdateInfo()
    {
        // Update the list
        BigDouble baseIncome = UpdateBuildingListValues();

        // Update the Multipliers
        BigDouble multiplierTot = UpdateGamesListValues();
        multiplierTot *= UpdateResearchListValues();
        multiplierTot *= UpdateUpgradesListValues();

        // total
        finalTotMultiplierTextField.text = Stats.ReturnAsString(multiplierTot);
        finalTotIncomeTextField.text = Stats.ReturnAsString(baseIncome * multiplierTot);
    }

    public float UpdateGamesListValues()
    {
        // Get list from data
        float[] miniGames = Stats.AllMiniGamesMultipliers();
        string[] names = Enum.GetNames(typeof(MiniGame));


        foreach (Transform child in gamesTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        float product = 1;

        for (int i = 0; i < miniGames.Length; i++) {
            float income = miniGames[i];

            BuildingsListItem item = Instantiate(listItemPrefab, gamesTextHolder.transform);

            item.SetName(names[i]);
            item.SetValue(Stats.ReturnAsString(income,3));
            item.SetImage((int)IconType.Multiply);
            product *= income;
        }
        gamesSumTextfield.text = Stats.ReturnAsString(product);
        return product;
    }
    public BigDouble UpdateUpgradesListValues()
    {
        // Get list from data
        (List<float> incomeList, List<string> researchNames) = UpgradeDatas.Instance.GetAllResearchCPSList();

        Debug.Log("** STATSPANEL - Upgrades - Updating Upgrade List "+incomeList.Count+" items");

        foreach (Transform child in upgradesTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        BigDouble product = 1;

        for (int i = 0; i < incomeList.Count; i++) {
            float income = incomeList[i];
            BuildingsListItem item = Instantiate(listItemPrefab, upgradesTextHolder.transform);
            item.SetName(researchNames[i]);
            item.SetValue(Stats.ReturnAsString(income));
            item.SetImage((int)IconType.Multiply);
            product *= income;
        }
        upgradesSumTextfield.text = Stats.ReturnAsString(product);
        return product;
    }
    public BigDouble UpdateResearchListValues()
    {
        // Get list from data
        (List<float> incomeList, List<string> researchNames) = ResearchDatas.Instance.GetAllResearchCPSList();

        Debug.Log("** STATSPANEL - Research - ** Updating List " + incomeList.Count+" items");

        foreach (Transform child in researchTextHolder.transform.GetComponentInChildren<Transform>(false))
            Destroy(child.gameObject);

        BigDouble product = 1;

        for (int i = 0; i < incomeList.Count; i++) {
            float income = incomeList[i];
            BuildingsListItem item = Instantiate(listItemPrefab, researchTextHolder.transform);
            item.SetName(researchNames[i]);
            item.SetValue(Stats.ReturnAsString(income));
            item.SetImage((int)IconType.Multiply);
            product *= income;
        }
        researchSumTextfield.text = Stats.ReturnAsString(product);

        return product;
    }
    public BigDouble UpdateBuildingListValues()
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
            item.SetImage((int)IconType.CPS);
            sum += income;
        }
        buildingSumTextfield.text = Stats.ReturnAsString(sum);
        return sum;
    }
}
