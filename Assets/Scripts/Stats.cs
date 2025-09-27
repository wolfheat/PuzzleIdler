using System;
using BreakInfinity;
using UnityEngine;



public enum MiniGameNames{ChessProblem,MineSweeper,DuckCantSwim,Bomberman,Tetris,Snake,BubbleTanks};

public static class Stats
{
    public const int MinimumChessRating = 1000;
    private const int ChessGameLossRatingChange = -50;
    private const int ChessGameWinRatingChange = 10;

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
    public static BigDouble BuildingCostMultiplerA { get; private set; } = 1.1f;
    public static BigDouble BuildingsBaseIncome { get; private set; } = 1f;
    public static float[] MiniGamesMultipliers { get; private set; } = { 1, 1.1f, 1.3f, 1.6f, 1.01f, 1.02f, 1.05f };

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
    public static int SudokuRating { get; internal set; } = 1000;
    public static int ChessRating { get; internal set; } = 1000;
    public static int MineSweeperRating { get; internal set; } = 1000;
    public static float MiniGamesMultipliersTotal => MiniGamesTotal();

    public static Action StatsUpdated;

    private static float MiniGamesTotal()
    {
        float ans = 1;
        foreach (var multi in MiniGamesMultipliers)
            ans *= multi;            
        return ans;
    }


    // Action - Events
    public static Action CoinUpdated;

    public static Action CPSUpdated;

    // Stats Settigns - Notation etc
    public static NumberNotation ActiveNumberNotation = NumberNotation.Scientific;


    public static void SetActiveNumberNotation(NumberNotation newNotation) => ActiveNumberNotation = newNotation;
    private static BigDouble CPSPerTick() => BuildingsBaseIncome * GetCPSTotalMultiplier();


    public static string ReturnAsString(BigDouble item) => IncrementalNumberFormatter.Format(item, ActiveNumberNotation); // Use the new numberformatter class to decide how to show the number
    public static string ReturnAsString(float item) => IncrementalNumberFormatter.Format(item, ActiveNumberNotation); // Use the new numberformatter class to decide how to show the number

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
        CoinUpdated?.Invoke();
        return added;
    }
    public static void AddGems(BigDouble amt)
    {
        Debug.Log("   Adding gems "+amt);
        GemsHeld += amt;
        CoinUpdated?.Invoke();
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
    }
    
    internal static void RemoveCoins(BigDouble cost)
    {
        CoinsHeld -= cost;
        Debug.Log("Removed coins, now have "+CoinsHeld);
    }

    internal static void ChangeChessRating(bool didWin)
    {
        ChessRating = Math.Max(ChessRating + (didWin ? ChessGameWinRatingChange : ChessGameLossRatingChange),1000);
        
        MiniGamesMultipliers[(int)MiniGameNames.ChessProblem] = ChessRating/1000f;

        Debug.Log("SAVESYSTEM - Changed player Rating to "+ChessRating);

        SavingUtility.playerGameData.PlayerChessRating = ChessRating;

        StatsUpdated?.Invoke();

    }
    
    internal static void RatingDropEachMinute(int amt = 1)
    {
        Debug.Log("RatingDrop Happening - each minute");

        ChessRating = Math.Max(ChessRating - amt,1000);
        MiniGamesMultipliers[(int)MiniGameNames.ChessProblem] = ChessRating/1000f;

        MineSweeperRating = Math.Max(MineSweeperRating - amt, 1000);
        MiniGamesMultipliers[(int)MiniGameNames.MineSweeper] = MineSweeperRating/1000f;

        UpdateSaveRatings();
        StatsUpdated?.Invoke();
        CPSUpdated?.Invoke();

        // Also Re-save this new info
        SavingUtility.playerGameData.TriggerSave();

    }

    private static void UpdateSaveRatings()
    {
        SavingUtility.playerGameData.PlayerChessRating = ChessRating;
        SavingUtility.playerGameData.PlayerMinesweeperRating = MineSweeperRating;
    }

    internal static void SetMiniGameStats(int playerChessRating, int playerMinesweeperRating)
    {
        // Sets data from save ? Do not save it?
        ChessRating = Math.Max(playerChessRating,1000);
        MineSweeperRating = Math.Max(playerMinesweeperRating,1000);


        MiniGamesMultipliers[(int)MiniGameNames.ChessProblem] = ChessRating / 1000f;
        MiniGamesMultipliers[(int)MiniGameNames.MineSweeper] = MineSweeperRating / 1000f;


        Debug.Log("Rating loaded into stats as " + ChessRating);

    }

    internal static int IncreaseMinesweeperRating(int boardDifficulty)
    {
        // Have stats increase less when higher=
        // 1000-1399 - Normal
        // 1400-1599 - 0.8
        // 1600-1799 - 0.5
        // 1800-1999 - 0.3
        // 2000+     - 0.1


        float multiplier = MinesweeperDifficultyRatingAwardedMultiplier(boardDifficulty);

        int ratingGain = (int)(boardDifficulty * multiplier);

        Debug.Log("MS: Rating gain "+ratingGain);

        // If there will be a loss possible this needs to exist to limit it to 1000
        MineSweeperRating = Math.Max(MineSweeperRating + ratingGain, 1000);

        MiniGamesMultipliers[(int)MiniGameNames.MineSweeper] = MineSweeperRating/ 1000f;

        Debug.Log("SAVESYSTEM - Changed player Rating to " + ChessRating);

        SavingUtility.playerGameData.PlayerMinesweeperRating = MineSweeperRating;

        UpdateSaveRatings();
        StatsUpdated?.Invoke();

        // Make sure the CPS is recaluclated
        Stats.CPSUpdated?.Invoke();


        return ratingGain;
    }


    private static float MinesweeperDifficultyRatingAwardedMultiplier(int boardDifficulty)
    {        
        float multiplier = MineSweeperRating switch
        {
            < 1400 => 1,
            < 1600 => 0.8f,
            < 1800 => 0.5f,
            < 2000 => 0.3f,
            _ => 0.1f
        };
        return multiplier;
    }
}
