using UnityEngine;

public class Digits : MonoBehaviour
{
    [SerializeField] SpriteRenderer rend;
    [SerializeField] Sprite[] digitsNumbers;
    internal void SetValue(int val)
    {
        rend.sprite = digitsNumbers[val];
    }
}
