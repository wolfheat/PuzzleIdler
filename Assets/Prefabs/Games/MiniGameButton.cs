using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniGameButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        MultiplierMenu.Instance.ToggleGame(index);
    }

    int index = 0;

    [SerializeField] private TextMeshProUGUI GameNameText;
    [SerializeField] private TextMeshProUGUI multiplierText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetButtonInfo(string gameName, float multiplier, int indexIn)
    {
        index = indexIn;
        GameNameText.text = gameName;
        UpdateMultiplier(multiplier);
    }

    // Update is called once per frame
    public void UpdateMultiplier(float newMultiplayer) => multiplierText.text = newMultiplayer.ToString("F3", CultureInfo.InvariantCulture);
}
