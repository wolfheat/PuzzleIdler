using System;
using BreakInfinity;
using UnityEngine;

public class PrintBigDoubles : MonoBehaviour
{


    void Start()
    {
        IncrementalNumberFormatter.ReceivedValue += PrintValue;    
        IncrementalNumberFormatter.ReceivedDouble += PrintDouble;    
    }

    private void PrintValue(BigDouble number)
    {
        Debug.Log("Recevied number "+number+" consisting of "+number.Mantissa+"e"+number.Exponent);
    }
    private void PrintDouble(double number,string prefix)
    {
        Debug.Log(prefix+number);
    }
}
