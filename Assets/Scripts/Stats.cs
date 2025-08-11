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

    private static double CPSFinalCalculation()
    {
        Debug.Log("Returning CPS base "+CPSBase+" mult="+MultiplierA);
        return CPSBase * MultiplierA;
    }

    private static string ReturnAsString(BigDouble item)
    {
        Debug.Log("Trying to format item:" + item + " consisting of " + item.Mantissa+"e"+item.Exponent);
        // Use the new numberformatter class to decide how to show the number

        

        return NumberFormatter.Format(item);        
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
        CoinUpdated.Invoke();
    }

    internal static void AddCoins(int amt)
    {
        CoinsHeld += amt;
        Debug.Log("Added "+ amt+ " coins > [" + CoinsHeld.ToString("F2")+ "]");
        CoinUpdated.Invoke();
    }
}
