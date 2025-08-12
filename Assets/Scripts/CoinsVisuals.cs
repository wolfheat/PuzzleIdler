using System;
using TMPro;
using UnityEngine;

public class CoinsVisuals : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coins;
    [SerializeField] private TextMeshProUGUI cps;

    [SerializeField] private TextMeshProUGUI gems;

    public static CoinsVisuals Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("Coinsvisual Awake");
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Add listener for added coin event
        Stats.CoinUpdated += UpdateTexts;
    }

    private void OnDisable()
    {
        Stats.CoinUpdated -= UpdateTexts;        
    }

    public void UpdateTexts()
    {
        //Debug.Log("Updating Texts");
        coins.text = Stats.CoinsHeldAsString;
        cps.text = Stats.CPSAsString;
        gems.text = Stats.GemsHeldAsString;
        //Debug.Log("Updating Texts-Complete");
    }
}
