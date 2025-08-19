using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Settings : MonoBehaviour
{

    [SerializeField] private TMP_Dropdown dropDown; 
    private readonly List<string> dropDownOptionsNames = new List<string>{ "Scientifical 99.99M", "Mathematical 9.99e07", "Engineering 99.99e6", "Alphabetical" };


    private void Start()
    {
        if (dropDown != null) {
            dropDown.ClearOptions();
            dropDown.AddOptions(dropDownOptionsNames); 
        }
        UpdateDropDownInitial();
    }

    private void OnEnable()
    {
        // Activate correct Numberformation in the list
        UpdateDropDownInitial();
    }

    private void UpdateDropDownInitial()
    {
        dropDown.SetValueWithoutNotify((int)Stats.ActiveNumberNotation);
    }

    public void CloseSettings() => gameObject.SetActive(false);

    public void ChangeNumberNotation()
    {
        if(dropDown != null) {
            Debug.Log("Notations changed to "+dropDown.value);
            Stats.SetActiveNumberNotation((NumberNotation)dropDown.value);
            SavingUtility.gameSettingsData.ActiveNumberNotation = (NumberNotation)dropDown.value;
            GameSettingsData.SaveNeeded?.Invoke();
        }
    }

    public void RequestSaveToFile()
    {   
        Debug.Log("Settings - Requesting save to file");
        SavingUtility.playerGameData.TriggerSave();
        //SavingUtility.Instance.SavePlayerDataToFile();
    }
}
