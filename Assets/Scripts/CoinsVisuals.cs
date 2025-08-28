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
        Stats.CoinUpdated += UpdateNeeded;
        Stats.CPSUpdated += UpdateCPSNeeded;
    }
    private bool doUpdate = false;
    private bool doCPSUpdate = false;
    private void UpdateNeeded() => doUpdate = true;
    private void UpdateCPSNeeded() => doCPSUpdate = true;
    private void LateUpdate()
    {
        if (doUpdate) {
            UpdateTexts();
        }
        if (doCPSUpdate) {
            UpdateCPSTexts();
        }
        doUpdate = false;
        doCPSUpdate = false;
    }

    private void OnDisable()
    {
        Stats.CoinUpdated -= UpdateTexts;        
    }


    public void UpdateTexts()
    {
        //Debug.Log("Updating Texts");
        coins.text = Stats.CoinsHeldAsString;

        // Remove CPS AND GPS FROM HERE??
        //cps.text = Stats.CPSAsString;
        //gems.text = Stats.GemsHeldAsString;
        //Debug.Log("Updating Texts-Complete");
    }
    public void UpdateCPSTexts()
    {
        Debug.Log("** Updating Texts Stats.CPSAsString = "+ Stats.CPSAsString);
        cps.text = Stats.CPSAsString;
        gems.text = Stats.GemsHeldAsString;
        //Debug.Log("Updating Texts-Complete");
    }
}
