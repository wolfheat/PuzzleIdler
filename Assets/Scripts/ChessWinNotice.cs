using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChessWinNotice : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textMeshProText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;

    public void SetWin(bool win)
    {
        textMeshProText.text = win ? "YOU WIN" : "INCORRECT";
        backgroundImage.color = win ? winColor : loseColor;

    }
}
