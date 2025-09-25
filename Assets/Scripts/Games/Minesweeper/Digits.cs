using UnityEngine;
using UnityEngine.UI;

public class Digits : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite[] digitsNumbers;
    internal void SetValue(int val)
    {
        image.sprite = digitsNumbers[val];
    }
}
