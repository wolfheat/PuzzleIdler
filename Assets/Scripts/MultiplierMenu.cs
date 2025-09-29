using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MultiplierMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI[] multipierValues;
    [SerializeField] private TextMeshProUGUI totalValue;

    private bool active = false;
    private Vector3 activePosition = new Vector3(0,0,0);
    private Vector3 inActivePosition = new Vector3(-1110,0,0);

    private const float AnimationSpeed = 4500f;

    private void OnEnable() => AnimatePanelInto(active);

    

    private void Start()
    {
        Stats.StatsUpdated += StatsUpdated;
        SavingUtility.LoadingComplete += SaveFileLoaded;
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
        AnimatePanelInto(active);
        UpdateStats();
    }

    public void UpdateStats()
    {
        //Debug.Log("SAVESYSTEM - Updating Minigame stats");
        Stats.UpdateMiniGameTotalMultiplier();

        float[] multipliers = Stats.AllMiniGamesMultipliers();
        for (int i = 0; i < multipierValues.Length && i < multipliers.Length; i++) {
            multipierValues[i].text = "x " + multipliers[i].ToString("F3", CultureInfo.InvariantCulture);
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
}
