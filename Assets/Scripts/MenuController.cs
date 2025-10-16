using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Options optionsLeft; 

    void Start()
    {
        Inputs.NumberPressed += OnNumberPressed;       
    }

    private void OnNumberPressed(int menuIndex)
    {
        // Chceck if any games are loaded
        if (menuIndex < 1 || MultiplierMenu.Instance.GameIsLoaded) return;

        // No Game is loaded so switch to the panel
        switch (menuIndex) {
            case < 8:
                if(optionsLeft.AnyStatsPanelOpen)
                    return;
                optionsLeft.SelectOption(menuIndex-1); 
                break;
            case 8:
                optionsLeft.ToggleSettings(); 
                break;
            case 9:
                optionsLeft.ToggleStats(); 
                break;
        }

    }

}
