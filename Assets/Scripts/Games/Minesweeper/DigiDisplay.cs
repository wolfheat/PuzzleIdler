using System;
using UnityEngine;
public class DigiDisplay : MonoBehaviour
{
    [SerializeField] Digits[] digits;


    public void ShowValue(int val)
    {
        if (val > 999)
            val = 999;

        bool minus = val < 0;
        val = Math.Abs(val);

        int hundreds = val / 100;
        val -= hundreds*100;
        int dec = val / 10;
        val -= dec*10;
        //Debug.Log(""+hundreds+","+dec+","+val);
        // Hundreds - dec - val
        SetDigits(minus?10:hundreds,dec,val);
    }

    private void SetDigits(int hundreds, int dec, int val)
    {
        digits[0].SetValue(hundreds);
        digits[1].SetValue(dec);
        digits[2].SetValue(val);

    }
}
