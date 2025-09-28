using System;
using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AwayIncomeNotice : BaseNotice, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI coinText; 
    [SerializeField] private TextMeshProUGUI gemText; 
    [SerializeField] private TextMeshProUGUI time; 
    private void Awake()
    {
        noticeTime = 4f;
    }
    public void SetValues(BigDouble coins, BigDouble gems, long passedTime)
    {
        // also Format these to look correctly
        coinText.text = "+" + Stats.ReturnAsString(coins);
        gemText.text = "+" + Stats.ReturnAsString(gems);
        
        // Convert seconds to time
        time.text = "Away for " + CreatePassedTimeString(passedTime);


    }

    private string CreatePassedTimeString(long passedTime)
    {
        Debug.Log("Passed ticks: "+passedTime);

        TimeSpan passed = TimeSpan.FromSeconds(passedTime);
        int days = passed.Days;
        int hours = passed.Hours;
        int minutes = passed.Minutes;
        int seconds = passed.Seconds;
        if(days == 0) {
            if(hours == 0) {
                return minutes + " Min "+ seconds + " Sec.";
            }
            return days + (days==1?" Day ":" Days ")+ hours+" Hour.";
        }
        return hours + (hours == 1?" Hour ":" Hours ") + minutes+" Min.";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Removeing Away Notice");
        Destroy(gameObject);

    }
}
