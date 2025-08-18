using System;
using UnityEngine;

public class SavingUtilityHelper : MonoBehaviour
{
    void Start()
    {
        Stats.CoinUpdated += PlayerCoinsChanged;
        SavingUtility.LoadingComplete += LoadingComplete;
    }

    private void LoadingComplete()
    {
        Debug.Log("SAVINGUTILITY HELPER - LOADING COMPLETE");
        // Set the stats values
        Stats.AddCoins(SavingUtility.playerGameData.coins);
        Stats.AddGems(SavingUtility.playerGameData.gems);
    }
        
    private void PlayerCoinsChanged()
    {
        if (!SavingUtility.HasLoaded) {
            Debug.Log("Can not save currency, not loaded");
            return;
        }
        SavingUtility.playerGameData.coins = Stats.CoinsHeld;
        Debug.Log("Updating held currency to save file " + SavingUtility.playerGameData.coins);
        SavingUtility.playerGameData.gems = Stats.GemsHeld;
    }
}
