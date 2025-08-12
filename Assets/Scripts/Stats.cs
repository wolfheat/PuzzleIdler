using System;
using BreakInfinity;
using UnityEngine;

public static class Stats
{
    public static BigDouble CoinsHeld { get; private set; } = 0;
    public static double GemsHeld { get; private set; } = 0;


    public static double CPSBase { get; private set; } = 0.1d;
    public static double GPSBase { get; private set; } = 0.1d;
    public static float MultiplierA { get; private set; } = 1.1f;


    public static string CoinsHeldAsString => ReturnAsString(CoinsHeld);
    public static string GemsHeldAsString => ReturnAsString(GemsHeld);
    public static string CPSAsString => ReturnAsString(CPSFinalCalculation());

    public static Action CoinUpdated;

    private static NumberNotation activeNumberNotation = NumberNotation.Scientific;


    public static void ActiveNumberNotation(NumberNotation newNotation)
    {
        Debug.Log("Changing stats notation to "+newNotation);
        activeNumberNotation = newNotation;
    }

    private static double CPSFinalCalculation()
    {
        //Debug.Log("Returning CPS base "+CPSBase+" mult="+MultiplierA);
        return CPSBase * MultiplierA;
    }

    private static string ReturnAsString(BigDouble item)
    {
        // Use the new numberformatter class to decide how to show the number
        return NumberFormatter.Format(item,activeNumberNotation);        
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
}
