using System;
using BreakInfinity;
using UnityEngine;

public static class Stats
{
    // Coinds and Gems - Held
    public static BigDouble CoinsHeld { get; private set; } = 0;
    public static BigDouble GemsHeld { get; private set; } = 0;


    // Base Income
    public static BigDouble CPSBase { get; private set; } = 0.1d;
    public static BigDouble GPSBase { get; private set; } = 0.1d;

    // Multipliers
    public static BigDouble CoinIncomeMultiplier { get; private set; } = 1f;
    public static BigDouble GemIncomeMultiplier { get; private set; } = 1f;
    public static BigDouble BuildingCostMultiplerA { get; private set; } = 1.1f;

    // Multipliers - Set methods
    public static void SetCoinMultiplier(BigDouble newValue) => CoinIncomeMultiplier = newValue;
    public static void SetGemMultiplier(BigDouble newValue) => GemIncomeMultiplier = newValue;

    // Strings - Get value strings
    public static string CoinsHeldAsString => ReturnAsString(CoinsHeld);
    public static string GemsHeldAsString => ReturnAsString(GemsHeld);
    public static string CPSAsString => ReturnAsString(CPSFinalCalculation());

    // Action - Events
    public static Action CoinUpdated;

    // Stats Settigns - Notation etc
    public static NumberNotation ActiveNumberNotation = NumberNotation.Scientific;


    public static void SetActiveNumberNotation(NumberNotation newNotation) => ActiveNumberNotation = newNotation;
    private static BigDouble CPSFinalCalculation() => CPSBase * CoinIncomeMultiplier;


    public static string ReturnAsString(BigDouble item) => NumberFormatter.Format(item, ActiveNumberNotation); // Use the new numberformatter class to decide how to show the number

    public static void Tick() => AddCoins(); // Tick the timer once

    private static void AddCoins()
    {
        //Debug.Log("Addcoins");
        CoinsHeld += CPSBase * CoinIncomeMultiplier;
        //Debug.Log("Invoke added coins");
        CoinUpdated?.Invoke();
    }

    internal static void AddCPS(BigDouble amt)
    {
        CPSBase += amt;
        //Debug.Log("Added "+ amt+ " coins > [" + CoinsHeld.ToString("F2")+ "]");
        CoinUpdated?.Invoke();
    }
    internal static void AddCoins(long amt)
    {
        Debug.Log("Adding coins A");
        CoinsHeld += amt;
        Debug.Log("Adding coins B");
        //Debug.Log("Added "+ amt+ " coins > [" + CoinsHeld.ToString("F2")+ "]");
        CoinUpdated?.Invoke();
    }
    internal static void AddCoins(BigDouble amt)
    {
        Debug.Log("");
        Debug.Log("Adding coins A have:" + CoinsHeld+" adding "+amt);
        CoinsHeld += amt;
        Debug.Log("Adding coins B now have: "+CoinsHeld);
        //Debug.Log("Added "+ amt+ " coins > [" + CoinsHeld.ToString("F2")+ "]");
        CoinUpdated?.Invoke();
    }
    
    internal static void AddGems(BigDouble amt)
    {
        GemsHeld += amt;
        CoinUpdated?.Invoke();
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

    public static BigDouble AllBuildingGainMultipliers()
    {
        // Calculate the new multiplier and store it - dont want to calculate this every time, only when the value changes ie getting new buildings or getting upgrades that affect it
        return CoinIncomeMultiplier;
    }
    
    public static BigDouble AllBuildingCostMultipliers()
    {
        // Get the entire list of multipliers?
        return BuildingCostMultiplerA;
    }

}
