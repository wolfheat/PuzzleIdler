using System;
using BreakInfinity; // make sure BreakInfinity.cs is in your project

public enum NumberNotation
{
    ShortScale,       // Million, Billion, Trillion...
    ShortAbbreviation, // M, B, T...
    Scientific        // 1.23e45
}

public static class NumberFormatter 
{
    // Full short scale names
    private static readonly string[] shortScaleNames = {
        "", "Thousand", "Million", "Billion", "Trillion", "Quadrillion",
        "Quintillion", "Sextillion", "Septillion", "Octillion", "Nonillion",
        "Decillion", "Undecillion", "Duodecillion", "Tredecillion",
        "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septendecillion",
        "Octodecillion", "Novemdecillion", "Vigintillion"
    };

    // Abbreviations (match index with above names)
    private static readonly string[] shortAbbr = {
        "", "K", "M", "B", "T", "Qa",
        "Qi", "Sx", "Sp", "Oc", "No",
        "Dc", "UDc", "DDc", "TDc",
        "QaDc", "QuDc", "SxDc", "SpDc",
        "OcDc", "NvDc", "V"
    };

    public static Action<BigDouble> ReceivedValue; 
    public static Action<double,string> ReceivedDouble; 

    public static string Format(BigDouble value, NumberNotation notation = NumberNotation.ShortAbbreviation, int decimals = 2)
    {

        ReceivedValue.Invoke(value);

        double displayValue = value.Mantissa;

        if (value.Sign() == 0) return "0";

        if(value.Exponent < 3) {
            return $"{value.ToString($"F{decimals}")}";
        }

        // Get exponent in base 10
        int exponent = (int)Math.Floor(value.Log10());
        int group = exponent / 3;
        int truncateAway = group * 3;
        int mantissaStepMult = exponent - truncateAway;

        double mantissaMultiplier = Math.Pow(10,mantissaStepMult);
        double mantissaMultiplierToTruncate = Math.Pow(10,mantissaStepMult+2);

        // take the steps + decimals to truncate before showing?
        double truncatedMantissa = Math.Truncate(displayValue * mantissaMultiplierToTruncate);
        displayValue = truncatedMantissa / 100;


        ReceivedDouble.Invoke(exponent,"Exponent: ");
        ReceivedDouble.Invoke(group,"Group: ");
        ReceivedDouble.Invoke(truncateAway ,"Truncate away: ");
        ReceivedDouble.Invoke(mantissaMultiplier ,"Mantissa multiplier: ");

        //displayValue = value.Mantissa*mantissaMultiplier;
        ReceivedDouble.Invoke(displayValue ,"Displayvalue: ");

        double mantissa = value.Mantissa;
        /*
        BigDouble scale = BigDouble.Pow(10, group * 3);
        double scaled = mantissa / scale;

        double truncatedMantissa = Math.Truncate((double)scaled * Math.Pow(10, decimals)) / Math.Pow(10, decimals);

        string suffix = GetSuffixForGroup(group); // your suffix logic here
        */

        switch (notation) {
            case NumberNotation.ShortScale:
                if (group < shortScaleNames.Length) {
                    return $"{displayValue.ToString($"F{decimals}")} {shortScaleNames[group]}";
                }
                else {
                    // fallback to scientific
                    return displayValue.ToString($"E{decimals}");
                }

            case NumberNotation.ShortAbbreviation:
                if (group < shortAbbr.Length) {
                    return $"{displayValue.ToString($"F{decimals}")}{shortAbbr[group]}";
                }
                else {
                    return displayValue.ToString($"E{decimals}");
                }

            case NumberNotation.Scientific:
            default:
                return displayValue.ToString($"E{decimals}");
        }
    }
    public static BigDouble TruncateMantissa(BigDouble value, int decimals)
    {
        // Break into mantissa + exponent
        int exponent = (int)Math.Floor(value.Log10());
        BigDouble mantissa = value / BigDouble.Pow(10, exponent);

        // Truncate mantissa to decimals
        double factor = Math.Pow(10, decimals);
        mantissa = new BigDouble(Math.Truncate(mantissa.Mantissa * factor) / factor);

        // Rebuild number
        return mantissa * BigDouble.Pow(10, exponent);
    }
    private static double TruncateToDecimals(double value, int decimals)
    {
        double factor = Math.Pow(10, decimals);
        return Math.Truncate(value * factor) / factor;
    }

}
