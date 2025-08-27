using System;
using BreakInfinity;
using UnityEngine;

public class NoticeController : MonoBehaviour
{
    [SerializeField] private BaseNotice saveNotice; 
    [SerializeField] private AwayIncomeNotice awayIncomeNoticePrefab;

    public static NoticeController Instance { get; private set; }

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
        PlayerGameData.SaveNeeded += ShowSaveNotice;
        //SavingUtility.LoadingComplete += ShowAwayIncomeNotice; // Call this by another method need to calculate the away income
    }

    public void ShowAwayIncomeNotice(BigDouble coins, BigDouble gems, long ticks)
    {
        AwayIncomeNotice notice = Instantiate(awayIncomeNoticePrefab,transform);

        // Send in the away values here
        notice.SetValues(coins,gems,ticks);
    }

    private void ShowSaveNotice()
    {
        Instantiate(saveNotice, transform);
    }
    
    public void ShowDebugText(string text)
    {
        Debug.Log("LOGGED: "+text);
    }

}
