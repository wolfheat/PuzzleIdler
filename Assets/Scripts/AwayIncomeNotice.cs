using BreakInfinity;
using TMPro;
using UnityEngine;

public class AwayIncomeNotice : BaseNotice
{
    [SerializeField] private TextMeshProUGUI coinText; 
    [SerializeField] private TextMeshProUGUI gemText; 
    private void Awake()
    {
        noticeTime = 4f;
    }
    public void SetValues(BigDouble coins, BigDouble gems)
    {
        coinText.text = coins.ToString();
        gemText.text = gems.ToString();
    }
}
