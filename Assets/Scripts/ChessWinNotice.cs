using TMPro;
using UnityEngine;

public class ChessWinNotice : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textMeshProText;

    public void SetWin(bool win) => textMeshProText.text = win ? "YOU WIN" : "INCORRECT";
}
