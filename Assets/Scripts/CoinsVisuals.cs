using TMPro;
using Unity.VisualScripting;
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
        Canvas.willRenderCanvases += DoPendingUpdates;
    }

    private void OnDisable()
    {
        Stats.CoinUpdated -= UpdateTexts;
        Stats.CPSUpdated -= UpdateCPSNeeded;
        Canvas.willRenderCanvases -= DoPendingUpdates;
    }



    private bool doUpdate = false;
    private bool doCPSUpdate = false;
    private void UpdateNeeded() => doUpdate = true;
    private void UpdateCPSNeeded() => doCPSUpdate = true;
    private void DoPendingUpdates()
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
        Debug.Log("** A Updating Texts Stats.CPSAsString = "+ Stats.CPSAsString);
        cps.text = Stats.CPSAsString;
        Debug.Log("** B Updating Texts Stats.CPSAsString = "+ Stats.CPSAsString);
        gems.text = Stats.GemsHeldAsString;
        Debug.Log("** C Updating Texts Stats.CPSAsString = "+ Stats.CPSAsString);
        //Debug.Log("Updating Texts-Complete");
    }
}
