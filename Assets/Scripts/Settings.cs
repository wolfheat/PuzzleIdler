using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WolfheatProductions.SoundMaster;

public class Settings : MonoBehaviour
{

    [SerializeField] private TMP_Dropdown dropDown; 
    [SerializeField] private Toggle musicToggle; 
    [SerializeField] private GameObject confirmQuitPanel; 
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
        GreyScaleController.Instance.SetGameGreyScale(true);
    }

    private void OnDisable()
    {
        //Unset the greyscale
        GreyScaleController.Instance.SetGameGreyScale(false);
    }

    private void UpdateDropDownInitial()
    {
        dropDown.SetValueWithoutNotify((int)Stats.ActiveNumberNotation);
    }
    
    private void UpdateMusicInitial()
    {
        musicToggle.SetIsOnWithoutNotify(Stats.UseMusic);
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
    
    public void ToggleMusic()
    {
        if(musicToggle.isOn) {
            //SavingUtility.gameSettingsData.ActiveNumberNotation = (NumberNotation)dropDown.value;
            //GameSettingsData.SaveNeeded?.Invoke();
            SoundMaster.Instance.ActivateMusic();
        }
        else {
            SoundMaster.Instance.MuteMusic();
        }
    }

    public void RequestSaveToFile()
    {   
        Debug.Log("Settings - Requesting save to file");
        SavingUtility.playerGameData.TriggerSave();
        //SavingUtility.Instance.SavePlayerDataToFile();
    }

    public void OpenConfirmQuitPanel() => confirmQuitPanel.SetActive(true);
    public void ExitGameAndSave() => SavingUtility.Instance.PlayerInitiatedQuit();
}
