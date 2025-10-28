using System;
using System.Linq;
using BreakInfinity;
using UnityEngine;
using WolfheatProductions.SoundMaster;

public enum MiniGame{Chess,Minesweeper,Sudoku,Tetris, Snake, BlockPuzzle, Merge};

public static class Stats
{
    // Specific game limitations
    private const int MiniGameMin = 1000;
    private const int MiniGameMax = 2999;

    // Chess
    public const int ChessGameLossRatingChange = -50;
    public const int ChessGameWinRatingChange = 10;
    // Minesweeper

    // Sudoku
    public const int SudokuGameLossRatingChange = 0;
    public const int SudokuGameWinRatingChange = 150;

    public const int BlocksPuzzleBaseWinRatingChange = 100;

    
    // Coinds and Gems - Held
    public static BigDouble CoinsHeld { get; private set; } = 0;
    public static BigDouble GemsHeld { get; private set; } = 0;


    // Base Income
    public static BigDouble CPSBase { get; private set; } = 0.1d; // MAYBE NOT NEEDED SINCE WE HAVE - BuildingsBaseIncome which is the only thing that can contribute to the baseCPS ?

    public static BigDouble GPSBase { get; private set; } = 0.1d;

    // Multipliers
    public static BigDouble CPSResearchMultiplier { get; private set; } = 1f;
    public static BigDouble CPSUpgradesMultiplier { get; private set; } = 1f;

    public static BigDouble GPSMultiplier { get; private set; } = 0.0f;
    //public static BigDouble BuildingCostMultiplerA { get; private set; } = 1.1f;
    public static BigDouble BuildingsBaseIncome { get; private set; } = 1f;

    // Mini Games
    public static int[] MiniGameRatings = new int[] { 1000, 1000, 1000, 1000, 1000, 1000, 1000};
    public static int[] MiniGameGems = new int[] { 0, 0, 0, 0, 0, 0, 0};

    public static float MiniGamesMultipliersTotal { get; set; } = 1;

    public static float[] AllMiniGamesMultipliers() => MiniGameRatings.Select(x => x*0.001f).ToArray();

    // Multipliers - Set methods
    public static void SetCPSResearchMultiplier(BigDouble newValue) { CPSResearchMultiplier = newValue; CPSUpdated?.Invoke(); }
    public static void SetCPSUpgradeMultiplier(BigDouble newValue) { CPSUpgradesMultiplier = newValue; CPSUpdated?.Invoke(); }
    public static void SetBuildingBaseIncome(BigDouble newValue) { BuildingsBaseIncome = newValue; CPSUpdated?.Invoke(); }
    public static BigDouble GetCPSTotalMultiplier() => CPSResearchMultiplier * CPSUpgradesMultiplier * MiniGamesMultipliersTotal;

    public static void SetGemMultiplier(BigDouble newValue) => GPSMultiplier = newValue;


    // Strings - Get value strings
    public static string CoinsHeldAsString => ReturnAsString(CoinsHeld);
    public static string GemsHeldAsString => ReturnAsString(GemsHeld);
    public static string CPSAsString => ReturnAsString(CPSPerTick());

    // Games Stats
    //public static int SudokuRating { get; internal set; } = 1000;
    //public static int ChessRating { get; internal set; } = 1000;
    //public static int MineSweeperRating { get; internal set; } = 1000;

    public static Action StatsUpdated;

    public static void UpdateMiniGameTotalMultiplier()
    {
        MiniGamesMultipliersTotal = 1;
        float[] multipliers = MiniGameRatings.Select(x => x * 0.001f).ToArray();
        foreach (var multi in multipliers)
            MiniGamesMultipliersTotal *= multi;
    }


    // Action - Events
    public static Action CoinUpdated;
    public static Action HeldGemsUpdated;

    public static Action CPSUpdated;

    // Stats Settigns - Notation etc
    public static NumberNotation ActiveNumberNotation = NumberNotation.Scientific;
    public static bool UseMusic = false;


    public static void SetActiveNumberNotation(NumberNotation newNotation) => ActiveNumberNotation = newNotation;
    private static BigDouble CPSPerTick() => BuildingsBaseIncome * GetCPSTotalMultiplier();


    public static string ReturnAsString(BigDouble item, int decimals = 2) => IncrementalNumberFormatter.Format(item, ActiveNumberNotation, decimals); // Use the new numberformatter class to decide how to show the number
    public static string ReturnAsString(float item, int decimals = 2) => IncrementalNumberFormatter.Format(item, ActiveNumberNotation, decimals); // Use the new numberformatter class to decide how to show the number

    public static void Tick() => AddCoinByTicks(); // Tick the timer once

    private static BigDouble AddCoinByTicks(long ticks = 1)
    {
        BigDouble added = CPSPerTick() * ticks;
        //Debug.Log("Addcoins "+added);
        CoinsHeld += added;
        //Debug.Log("Invoke added coins");
        CoinUpdated?.Invoke();
        return added;
    }

    public static void SetCoinsAndGems(BigDouble coins, BigDouble gems)
    {
        CoinsHeld += coins;
        GemsHeld += gems;

        // Make sure not to call this before all values are written from the save file, since it overwrites all values on change
        //CoinUpdated?.Invoke();
    }

    public static void AddCoins(BigDouble amt)
    {
        CoinsHeld += amt;
        Debug.Log("   Adding coins " + amt);
        CoinUpdated?.Invoke();
    }

    private static BigDouble AddGemsByTicks(long ticks = 1)
    {
        // Maybe not have gems GPS added when away??

        BigDouble added = GPSBase * GPSMultiplier * ticks;
        //Debug.Log("Addcoins");
        GemsHeld += added;
        //Debug.Log("Invoke added coins");
        HeldGemsUpdated?.Invoke();
        return added;
    }
    public static void AddGems(BigDouble amt)
    {
        Debug.Log("   Adding gems "+amt);
        GemsHeld += amt;
        HeldGemsUpdated?.Invoke();
    }

    internal static void AddGPS(BigDouble amt)
    {
        GPSBase += amt;
        CoinUpdated?.Invoke();
    }

    public static void AddAwayCurrency(long ticks)
    {
        // Adds the income from the save time to the current time
        Debug.Log("Added CPS and GPS for away");
        BigDouble coinsAdded = AddCoinByTicks(ticks);

        BigDouble gemsAdded = AddGemsByTicks(ticks);

        Debug.Log("Coins "+coinsAdded+" for "+ticks+"s");

        // Send a notice to player on these addons
        NoticeController.Instance.ShowAwayIncomeNotice(coinsAdded,gemsAdded,ticks);

        // Also Re-save this new info
        SavingUtility.playerGameData.TriggerSave();

    }

    internal static bool CanAfford(int finalCoinCost)
    {
        return CoinsHeld >= finalCoinCost;
    }

    internal static void RemoveGems(BigDouble cost)
    {
        GemsHeld -= cost;
        Debug.Log("Removed gems, now have "+GemsHeld);
        HeldGemsUpdated?.Invoke();
    }
    
    internal static void RemoveCoins(BigDouble cost)
    {
        CoinsHeld -= cost;
        Debug.Log("Removed coins, now have "+CoinsHeld);

        // Play CoinSound
        SoundMaster.Instance.PlaySound(SoundName.BuySound);

    }

    //MINI GAMES - HANDLING
    private static void UpdateMinigameSaveStats()
    {
        SavingUtility.playerGameData.MiniGameRatings = MiniGameRatings;
        SavingUtility.playerGameData.MiniGameGems = MiniGameGems;

        StatsUpdated?.Invoke();

        // Also Re-save this new info - Moved here from each game panel better to have it here
        SavingUtility.playerGameData.TriggerSave();
    }

    // Changes
    public static void ChangeMiniGameRating(MiniGame game, int value)
    {
        MiniGameRatings[(int)game] = Mathf.Clamp(MiniGameRatings[(int)game] + value, MiniGameMin, MiniGameMax);
        Debug.Log("MiniGame "+game+" get new rating: " + MiniGameRatings[(int)game]);
        UpdateMinigameSaveStats();
    }

    public static void SetMiniGameValue(MiniGame game, int value)
    {
        MiniGameRatings[(int)game] = Mathf.Clamp(value, MiniGameMin, MiniGameMax);
        UpdateMinigameSaveStats();
    }


    // Specific games

    // Chess
    /*
    internal static void ChangeChessRating(bool didWin)
    {
        ChangeMiniGameRating(MiniGame.ChessProblem, (didWin ? ChessGameWinRatingChange : ChessGameLossRatingChange));
    }
    */

    // MineSweeper
    /*
    internal static int IncreaseMinesweeperRating(int boardDifficulty)
    {
        // Have stats increase less when higher=
        // 1000-1399 - Normal
        // 1400-1599 - 0.8
        // 1600-1799 - 0.5
        // 1800-1999 - 0.3
        // 2000+     - 0.1


        float multiplier = MinesweeperRatingGain(boardDifficulty);

        int ratingGain = (int)(boardDifficulty * multiplier);

        Debug.Log("MS: Rating gain " + ratingGain);

        // If there will be a loss possible this needs to exist to limit it to 1000
        MineSweeperRating = Math.Max(MineSweeperRating + ratingGain, 1000);

        MiniGameRatings[(int)MiniGame.MineSweeper] = MineSweeperRating / 1000f;

        Debug.Log("SAVESYSTEM - Changed player Rating to " + ChessRating);


        UpdateSaveRatings();
        StatsUpdated?.Invoke();

        // Make sure the CPS is recaluclated
        Stats.CPSUpdated?.Invoke();


        return ratingGain;
    }*/


    public static int MinesweeperRatingGain(int boardDifficulty)
    {
        float multiplier = MiniGameRatings[(int)MiniGame.Minesweeper] switch
        {
            < 1400 => 1,
            < 1600 => 0.8f,
            < 1800 => 0.5f,
            < 2000 => 0.3f,
            _ => 0.1f
        };
        return (int)(multiplier * boardDifficulty);
    }
    
    public static int BlockPuzzleRatingGain()
    {
        
        float multiplier = MiniGameRatings[(int)MiniGame.BlockPuzzle] switch
        {
            < 1400 => 1,
            < 1600 => 0.8f,
            < 1800 => 0.5f,
            < 2000 => 0.3f,
            _ => 0.1f
        };
        return (int)(multiplier * BlocksPuzzleBaseWinRatingChange);
    }


    internal static void RatingDropEachMinute(int amt = 1)
    {
        Debug.Log("RatingDrop Happening - each minute");

        // Decrease every mini Game
        for (int i = 0; i < MiniGameRatings.Length; i++) {            
            MiniGameRatings[i] = Math.Clamp(MiniGameRatings[i] - amt, MiniGameMin, MiniGameMax);
        }

        // Updage ratings
        UpdateSaveRatings(); 


        StatsUpdated?.Invoke();
        CPSUpdated?.Invoke();

        // Also Re-save this new info
        SavingUtility.playerGameData.TriggerSave();

    }

    private static void UpdateSaveRatings() => SavingUtility.playerGameData.MiniGameRatings = MiniGameRatings;
    
    private static void UpdateSaveGems() => SavingUtility.playerGameData.MiniGameGems = MiniGameGems;

    internal static void SetMiniGameStats(int[] ratings)
    {
        if (ratings == null)
            return; // Keep the default values

        // Overwrite all games in the game - allows for new games to be added and save file still working. (if array placement is kept - need id save for it to work with any)
        for (int i = 0; i < ratings.Length; i++) {
            MiniGameRatings[i] = ratings[i];
        }
    }
    
    internal static void SetMiniGameGems(int[] gems)
    {

        Debug.Log("Setting Mini games Gems ");  
        if (gems == null)
            return; // Keep the default values

        Debug.Log("Setting Mini games Gems [0] =" + gems[0]);

        // Overwrite all games in the game - allows for new games to be added and save file still working. (if array placement is kept - need id save for it to work with any)
        for (int i = 0; i < gems.Length; i++) {
            MiniGameGems[i] = gems[i];
        }
    }

    internal static int MiniGameRating(MiniGame game) => MiniGameRatings[(int)game];

    internal static void GemGain(MiniGame gameType)
    {
        Debug.Log("Gaining a gem "+gameType);
        MiniGameGems[(int)gameType]++;
        StatsUpdated?.Invoke();
    }
}
