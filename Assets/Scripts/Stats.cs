using System;
using BreakInfinity;
using UnityEngine;

public static class Stats
{
    public static BigDouble CoinsHeld { get; private set; } = 0;
    public static double GemsHeld { get; private set; } = 0;


    public static BigDouble CPSBase { get; private set; } = 0.1d;
    public static BigDouble GPSBase { get; private set; } = 0.1d;

    // MULTIPLIERS
    public static float MultiplierA { get; private set; } = 1.1f;
    public static BigDouble IncomeMultiplerA { get; private set; } = 1.1f;
    public static BigDouble BuildingCostMultiplerA { get; private set; } = 1.1f;


    public static string CoinsHeldAsString => ReturnAsString(CoinsHeld);
    public static string GemsHeldAsString => ReturnAsString(GemsHeld);
    public static string CPSAsString => ReturnAsString(CPSFinalCalculation());

    public static Action CoinUpdated;

    public static NumberNotation ActiveNumberNotation = NumberNotation.Scientific;


    public static void SetActiveNumberNotation(NumberNotation newNotation)
    {
        Debug.Log("Changing stats notation to "+newNotation);
        ActiveNumberNotation = newNotation;
    }

    private static BigDouble CPSFinalCalculation()
    {
        //Debug.Log("Returning CPS base "+CPSBase+" mult="+MultiplierA);
        return CPSBase * MultiplierA;
    }

    public static string ReturnAsString(BigDouble item)
    {
        // Use the new numberformatter class to decide how to show the number
        return NumberFormatter.Format(item,ActiveNumberNotation);        
    }

    public static void Tick()
    {
        // Tick the timer once
        AddCoins();
    }

    private static void AddCoins()
    {
        //Debug.Log("Addcoins");
        CoinsHeld += CPSBase * MultiplierA;
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
        Debug.Log("Adding coins A");
        CoinsHeld += amt;
        Debug.Log("Adding coins B");
        //Debug.Log("Added "+ amt+ " coins > [" + CoinsHeld.ToString("F2")+ "]");
        CoinUpdated?.Invoke();
    }

    internal static bool CanAfford(int finalCoinCost)
    {
        return CoinsHeld >= finalCoinCost;
    }

    internal static void RemoveCoins(BigDouble cost)
    {
        CoinsHeld -= cost;
        Debug.Log("Removed coins, now have "+CoinsHeld);
    }

    public static BigDouble AllBuildingGainMultipliers()
    {
        // Calculate the new multiplier and store it - dont want to calculate this every time, only when the value changes ie getting new buildings or getting upgrades that affect it
        return IncomeMultiplerA;
    }
    
    public static BigDouble AllBuildingCostMultipliers()
    {
        // Get the entire list of multipliers?
        return BuildingCostMultiplerA;
    }

}
