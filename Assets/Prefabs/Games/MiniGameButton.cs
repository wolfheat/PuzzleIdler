using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniGameButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) => OnPointerClick();
    public void OnPointerClick()
    {
        MultiplierMenu.Instance.ToggleGame(index);
    }

    int index = 0;

    [SerializeField] private TextMeshProUGUI GameNameText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private Image image;

    [Header("Button Colors")]
    [SerializeField] private Color existColor;
    [SerializeField] private Color nonExistColor;

    [Header("Text Colors")]
    [SerializeField] private Color existTextColor;
    [SerializeField] private Color nonExistTextColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetButtonInfo(string gameName, float multiplier, int indexIn, bool gameExists)
    {
        SetColor(gameExists);
        index = indexIn;
        GameNameText.text = gameName;
        UpdateMultiplier(multiplier);
    }

    private void SetColor(bool gameExists)
    {
        image.color = gameExists ? existColor : nonExistColor;

        // Also change text to dimmed when unavailable? /maybe just hide?
        GameNameText.color = gameExists ? existTextColor : nonExistTextColor;
        multiplierText.color = gameExists ? existTextColor : nonExistTextColor;
    }

    // Update is called once per frame
    public void UpdateMultiplier(float newMultiplayer) => multiplierText.text = newMultiplayer.ToString("F3", CultureInfo.InvariantCulture);

}
