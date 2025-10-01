using System;
using System.Globalization;
using BreakInfinity;
// 
// make sure BreakInfinity.cs is in your project

public enum NumberNotation
{
    Scientific,   // 1.23e14
    Mathematical, // M, B, T...
    Engineering,  // 123e12
    Alphabetical  // a,b,c...
    // "Mathematical", "Scientifical", "Engineering", "Alphabetical" };
}

public static class IncrementalNumberFormatter 
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
    "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No",
    "Dc", "UnDc", "DoDc", "TrDc", "QaDc", "QiDc", "SxDc", "SpDc", "OcDc", "NoDc",
    "Vg", "UnVg", "DoVg", "TrVg", "QaVg", "QiVg", "SxVg", "SpVg", "OcVg", "NoVg",
    "Tg", "UnTg", "DoTg", "TrTg", "QaTg", "QiTg", "SxTg", "SpTg", "OcTg", "NoTg",
    "Ce", "UnCe", "DoCe", "TrCe", "QaCe", "QiCe", "SxCe", "SpCe", "OcCe", "NoCe",
    "De", "UnDe", "DoDe", "TrDe", "QaDe", "QiDe", "SxDe", "SpDe", "OcDe", "NoDe",
    "Hc", "UnHc", "DoHc", "TrHc", "QaHc", "QiHc", "SxHc", "SpHc", "OcHc", "NoHc",
    "Ic", "UnIc", "DoIc", "TrIc", "QaIc", "QiIc", "SxIc", "SpIc", "OcIc", "NoIc",
    "Lc", "UnLc", "DoLc", "TrLc", "QaLc", "QiLc", "SxLc", "SpLc", "OcLc", "NoLc",
    "Mc", "UnMc", "DoMc", "TrMc", "QaMc", "QiMc", "SxMc", "SpMc", "OcMc", "NoMc",
    "Na", "UnNa", "DoNa", "TrNa", "QaNa", "QiNa", "SxNa", "SpNa", "OcNa", "NoNa",
    "Pc", "UnPc", "DoPc", "TrPc", "QaPc", "QiPc", "SxPc", "SpPc", "OcPc", "NoPc",
    "Fc", "UnFc", "DoFc", "TrFc", "QaFc", "QiFc", "SxFc", "SpFc", "OcFc", "NoFc",
    "Rg", "UnRg", "DoRg", "TrRg", "QaRg", "QiRg", "SxRg", "SpRg", "OcRg", "NoRg",
    "Og", "UnOg", "DoOg", "TrOg", "QaOg", "QiOg", "SxOg", "SpOg", "OcOg", "NoOg"
};

    /* OLD VERSION
    // Abbreviations (match index with above names)
    private static readonly string[] shortAbbr = {
        "", "K", "M", "B", "T", "Qa",
        "Qi", "Sx", "Sp", "Oc", "No",
        "Dc", "UDc", "DDc", "TDc", "QaDc", 
        "QuDc", "SxDc", "SpDc", "OcDc", "NvDc", 
        "Vg", "UVg", "DVg", "TVg", "QaVg", 
        "QuVg", "SxVg", "SpVg", "OcVg", "NvVg",
        "Tg", "UTg", "DTg", "TTg", "QaTg",
        "QuTg", "SxTg", "SpTg", "OcTg", "NvTg",
        "Ce", "UCe", "DCe", "TCe", "QaCe",
        "QuCe", "SxCe", "SpCe", "OcCe", "NvCe"

    };*/

    private const int AlphabeticScientificEndsAt = 11;

    public static Action<BigDouble> ReceivedValue; 
    public static Action<double,string> ReceivedDouble; 

    public static string Format(BigDouble value, NumberNotation notation = NumberNotation.Scientific, int decimals = 2)
    {
        // Formats the number into the selected notation
        double displayValue = value.Mantissa;

        if (value.Sign() == 0) return "0";

        if(value.Exponent < 3) {
            return $"{value.ToString($"F{decimals}", CultureInfo.InvariantCulture)}";
        }

        // Get exponent in base 10 - Do I need this at all?
        //long exponent = (long)Math.Floor(value.Log10());
        long exponent = value.Exponent;

        // Calculates which group the number belongs to as well as the digits to remove before truncation
        long group = exponent / 3;

        // scientific or engineer - Transform the mantissa into a number to be truncated
        double mantissaMultiplierToTruncate = Math.Pow(10, exponent - group * 3 + 2);

        // Truncates and divides the number to the final to be shown
        displayValue = Math.Truncate(displayValue * mantissaMultiplierToTruncate) / 100;

        switch (notation) {
            case NumberNotation.Scientific:
                if (group < shortAbbr.Length) {
                    return $"{displayValue.ToString($"F{decimals}", CultureInfo.InvariantCulture)}{shortAbbr[group]}";
                }
                // Fallback if out of strings to show                    
                return displayValue.ToString($"E{decimals}e{exponent}", CultureInfo.InvariantCulture);
            case NumberNotation.Engineering: {
                    exponent = group * 3;
                    return displayValue.ToString("0.00") + "e" + (exponent < 100 ? exponent.ToString("00") : exponent.ToString("000"));
                }
            case NumberNotation.Alphabetical: {
                    // convert group to alpha
                    // 10³⁶ = aa (10^36)
                    if (group < AlphabeticScientificEndsAt) {
                        return $"{displayValue.ToString($"F{decimals}", CultureInfo.InvariantCulture)}{shortAbbr[group]}";
                    }
                    else {
                        group -= AlphabeticScientificEndsAt;
                        // convert number to aa system
                        string alphaName = ""+(char)('a' + (group / 26)) + (char)('a' + (group % 26));

                        return $"{displayValue.ToString($"F{decimals}", CultureInfo.InvariantCulture)}{alphaName}";
                    }
                }
            case NumberNotation.Mathematical:
            default: {
                    double mathematicalDisplayValue = Math.Truncate(value.Mantissa * 100) / 100;
                    return mathematicalDisplayValue.ToString("0.00", CultureInfo.InvariantCulture) + "e" + (exponent<100 ? exponent.ToString("00"): exponent.ToString("000"));
                    //return displayValue.ToString($"E{decimals}e{exponent}");
                }            
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
