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
    public static string CPSAsString => ReturnAsString(GPSFinalCalculation());

    public static Action CoinUpdated;

    private static double GPSFinalCalculation()
    {
        return GPSBase * MultiplierA;
    }

    private static string ReturnAsString(BigDouble item)
    {
        if (item < 10)
            return item.ToString("F2");
        if (item < 100)
            return item.ToString("F1");
        if (item < 1000)
            return item.ToString("E3");
        if (item < 1000000)
            return item.ToString("E3");
        
        // scientifical here?         
        return item.ToString("E3");
    }

    public static void Tick()
    {
        // Tick the timer once
        AddCoins();
    }

    private static void AddCoins()
    {
        Debug.Log("Addcoins");
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
