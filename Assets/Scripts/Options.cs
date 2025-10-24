using System;
using UnityEngine;

public class Options : MonoBehaviour
{
    [SerializeField] private GameObject[] panels; 
    [SerializeField] private GameObject settings; 
    [SerializeField] private GameObject stats;
        
    private int active = -1;

    public bool AnyStatsPanelOpen => stats.activeSelf || settings.activeSelf;

    public void SelectOption(int newSelected)
    {
        // if not same close all and open it
        Debug.Log("Select Panel "+newSelected);
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

    public void ToggleSettings()
    {
        // Cloase other panels
        stats.SetActive(false);
        settings.SetActive(!settings.activeSelf);        
    }
    public void ToggleStats()
    {
        // Cloase other panels
        settings.SetActive(false);
        stats.SetActive(!stats.activeSelf);
    }

    private void OpenPanel(int newPanel) => panels[newPanel].SetActive(true);
}
