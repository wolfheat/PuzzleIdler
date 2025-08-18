using System;
using UnityEngine;

public class Options : MonoBehaviour
{
    [SerializeField] private GameObject[] panels; 
    [SerializeField] private GameObject settings; 

    private int active = -1;
    public void SelectOption(int newSelected)
    {
        // if not same close all and open it

        if(newSelected == active)
            return;

        CloseAll();
        OpenPanel(newSelected);
        active = newSelected;
    }

    private void CloseAll()
    {
        foreach(GameObject panel in panels) 
            panel.SetActive(false);
    }

    public void ToggleSettings() => settings.SetActive(!settings.activeSelf);

    private void OpenPanel(int newPanel) => panels[newPanel].SetActive(true);
}
