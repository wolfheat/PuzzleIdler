using System;
using System.Collections.Generic;
using UnityEngine;

public class PiecePromotion : MonoBehaviour
{
    [SerializeField] private GameObject panel; 
    [SerializeField] private List<SelectionChessPiece> options;

    public static PiecePromotion Instance { get; private set; }

    public Action<ChessMove> OnPlayerSelect;

    private ChessMove playersMove;
    int colorIndex = 0;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HidePanel() => panel.SetActive(false);

    public void InitiateWithColor(ChessMove PlayersMove, int color = 0)
    {
        playersMove = PlayersMove;
        SetColor(color);
    }

    private void SetColor(int color = 0)
    {
        panel.SetActive(true);

        colorIndex = color;
        for (int i = 0; i < 4; i++) {
            options[i].ChangeType(color*6+i);
        }
    }

    public void PlayerSelectingOption(int selectIndex)
    {
        Debug.Log("Player selected index "+selectIndex);

        // Add players chosen option
        playersMove.promote = 8 + selectIndex;

        // Send it back
        OnPlayerSelect?.Invoke(playersMove);

        // Hide the Panel
        panel.SetActive(false);
    }

}
