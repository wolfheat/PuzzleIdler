using System;
using System.Collections;
using System.Linq;
using TMPro;
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

    public void TogglePanel()
    {
        active = !active;
        // Animate it to become this value
        AnimatePanelInto(active);
        UpdateStats();
    }

    public void UpdateStats()
    {
        Debug.Log("Updating stars");
        float[] multipliers = Stats.MiniGamesMultipliers;
        for (int i = 0; i < multipierValues.Length && i < multipliers.Length; i++) {
            multipierValues[i].text = "x " + multipliers[i].ToString("F2");
        }
        totalValue.text = "x " + multipliers.Sum().ToString("F2");
    }

    private void AnimatePanelInto(bool active)
    {
        Debug.Log("Animating panel to become active: "+active);
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
