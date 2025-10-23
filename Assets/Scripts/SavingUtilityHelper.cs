using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class SavingUtilityHelper : MonoBehaviour
{
    private const int MaxAwayTime = 172800; // 2 days = 48h

    void Start()
    {
        Stats.CoinUpdated += PlayerCoinsChanged;
        SavingUtility.LoadingComplete += LoadingComplete;
    }


    private void LoadingComplete()
    {
        // Set the correct Numberformation
        Stats.ActiveNumberNotation = SavingUtility.gameSettingsData.ActiveNumberNotation;

        Debug.Log("  SAVINGUTILITY HELPER - LOADING COMPLETE");
        
        // Set the stats values - to the old values
        Stats.SetCoinsAndGems(SavingUtility.playerGameData.coins, SavingUtility.playerGameData.gems);

        // Mini Games
        Stats.SetMiniGameStats(SavingUtility.playerGameData.MiniGameRatings);

        // Gems
        Stats.SetMiniGameGems(SavingUtility.playerGameData.MiniGameGems);



        // Time
        long seconds = (long)(DateTime.Now - SavingUtility.playerGameData.SaveTime).TotalSeconds;
        Debug.Log("Save Date = " + SavingUtility.playerGameData.SaveTime);
        Debug.Log("Current Date = " + DateTime.Now);
        Debug.Log("Seconds passed = "+seconds);

        // Calculate the away income and give the player that
        int secondsPassedAway = (int)Math.Clamp(seconds,0, MaxAwayTime);

        // Delay this so stats has time to get updated
        StartCoroutine(DelayedAddAwayCurrency(secondsPassedAway));
    }

    private IEnumerator DelayedAddAwayCurrency(int secondsPassedAway)
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("  SAVINGUTILITY HELPER - ADD AWAY CURRENCY");
        Stats.AddAwayCurrency(secondsPassedAway);
    }

    private void PlayerCoinsChanged()
    {
        if (!SavingUtility.HasLoaded) {
            Debug.Log("Can not save currency, not loaded");
            return;
        }
        SavingUtility.playerGameData.coins = Stats.CoinsHeld;
        //Debug.Log("Updating held currency to save file " + SavingUtility.playerGameData.coins);
        SavingUtility.playerGameData.gems = Stats.GemsHeld;
    }
}
