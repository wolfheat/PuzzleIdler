using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class MultiplierMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private List<MiniGameButton> miniGameButtons = new();
    [SerializeField] private List<GameObject> miniGames;
    [SerializeField] private TextMeshProUGUI totalValue;

    // Prefabs
    [SerializeField] private MiniGameButton miniGameButtonPrefab;
    [SerializeField] private GameObject equalPrefab;
    [SerializeField] private GameObject timesPrefab;

    [SerializeField] private GameObject miniButtonHolder;

    private bool active = false;
    private int activeGameIndex = -1;

    private Vector3 activePosition = new Vector3(0,0,0);
    private Vector3 inActivePosition = new Vector3(-1110,0,0);

    private const float AnimationSpeed = 4500f;
     
    private void OnEnable() => AnimatePanelInto(active);


    public static MultiplierMenu Instance { get; private set; }

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
        Stats.StatsUpdated += StatsUpdated;
        SavingUtility.LoadingComplete += SaveFileLoaded;

        SetAllButtonInfo();
    }

    private void SetAllButtonInfo()
    {
        Debug.Log("Creating buttons Start ");
        DestroyAllButtons();

        float[] multipliers = Stats.AllMiniGamesMultipliers();
        string[] gameNames = Enum.GetNames(typeof(MiniGame));
        Debug.Log("Creating buttons sizes =  "+multipliers.Length+","+gameNames.Length);

        Instantiate(equalPrefab, miniButtonHolder.transform);

        for (int i = 0; i < multipliers.Length && i < gameNames.Length; i++) {
            Debug.Log("Creating button "+i);
            MiniGameButton button = Instantiate(miniGameButtonPrefab, miniButtonHolder.transform);
            miniGameButtons.Add(button);
            button.SetButtonInfo(gameNames[i],multipliers[i],i);
            if(i < multipliers.Length -1 && i < gameNames.Length -1)
                Instantiate(timesPrefab, miniButtonHolder.transform);
        }
    }

    private void DestroyAllButtons()
    {
        foreach(Transform child in miniButtonHolder.transform) {
            if (child == miniButtonHolder)
                continue;
            Destroy(child.gameObject);
        }
    }

    private void SaveFileLoaded()
    {
        // Set all values from loaded save file
        StartCoroutine(UpdateStatsDelayed());
    }

    private IEnumerator UpdateStatsDelayed()
    {
        yield return new WaitForSeconds(0.3f);
        UpdateStats();
    }


    private void StatsUpdated()
    {
        UpdateStats();
    }

    public void TogglePanel()
    {
        active = !active;
        // Animate it to become this value
        Debug.Log("Toggle Minigame Panel: "+active);

        AnimatePanelInto(active);
        UpdateStats();
    }

    public void UpdateStats()
    {
        //Debug.Log("SAVESYSTEM - Updating Minigame stats");
        Stats.UpdateMiniGameTotalMultiplier();

        float[] multipliers = Stats.AllMiniGamesMultipliers();
        for (int i = 0; i < miniGameButtons.Count && i < multipliers.Length; i++) {
            miniGameButtons[i].UpdateMultiplier(multipliers[i]); 
            //Debug.Log(i+" "+ multipliers[i]);
        }
        totalValue.text = "x " + Stats.MiniGamesMultipliersTotal.ToString("F3", CultureInfo.InvariantCulture);
    }

    private void AnimatePanelInto(bool active)
    {
        //Debug.Log("Animating panel to become active: "+active);
        //panel.transform.localPosition = active ? activePosition : inActivePosition;
        StopAllCoroutines();
        StartCoroutine(AnimatePanel());
    }

    private IEnumerator AnimatePanel()
    {
        panel.SetActive(true);
        yield return null; // Makes sure current transform position is updated correctly
        Vector3 startPosition = panel.transform.localPosition;
        Vector3 targetPosition = active ? activePosition : inActivePosition;
        float distance = Mathf.Abs(targetPosition.x - startPosition.x);
        float timeToAnimate = distance / AnimationSpeed;
        float timer = timeToAnimate;
        Debug.Log("Animation time: "+timeToAnimate+" moving from "+startPosition.x+" to "+targetPosition.x);
        while (timer > 0) {
            timer -= Time.deltaTime;
            float percent = timer / timeToAnimate;
            // Place menu
            panel.transform.localPosition = Vector3.Lerp(startPosition,targetPosition,1-percent);
            yield return null;
        }
        Debug.Log("Animation of panel complete active:"+active);
        panel.SetActive(active);
    }

    internal void ToggleGame(int index)
    {
        Debug.Log("Activating Menu "+index);

        if (index >= miniGames.Count)
            return;

        if(activeGameIndex >= 0)
            miniGames[activeGameIndex].SetActive(false);

        if(activeGameIndex == index) {
            activeGameIndex = -1;
            return;
        }

        // New Game
        miniGames[index].SetActive(true);
        activeGameIndex = index;
    }
}
