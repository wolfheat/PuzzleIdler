using System;
using UnityEngine;
using UnityEngine.UI;

public class SodukoBox : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] Sprite[] sprites;
    private int currentType = 0;
    private bool isFixed = false;
    public bool Fixed => isFixed;

    public int Number => currentType;

    private void Start()
    {
        UpdateSprite();
    }

    public void SetAsFixed(int number, bool fix=true)
    {
        Debug.Log("Setting as fixed = "+fix+" value = "+number, this);
        Debug.Log("Image = "+image);

        ChangeType(number);
        isFixed = fix;
    }
    
    public bool RequestChangeType(int type)
    {
        if (isFixed) return false;
        ChangeType(type);
        return true;
    }

    private void ChangeType(int type)
    {
        currentType = type;
        UpdateSprite();
    }

    public void UpdateSprite() => image.sprite = sprites[currentType];

    internal bool RequestNextNumber(bool rightClick = false)
    {
        if (isFixed) return false;
        currentType = (currentType + (rightClick ? 9 : 1)) % 10;
        UpdateSprite();
        return true;
    }
}
