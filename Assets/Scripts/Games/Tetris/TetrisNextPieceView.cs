using System;
using UnityEngine;

public class TetrisNextPieceView : MonoBehaviour
{
    TetrisPiece activePiece;

    [Header("Pieces: I, J, L, O, S, T, Z ")]
    [SerializeField] private GameObject[] pieces;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowPiece(-1);        
    }

    public void ShowPiece(int type)
    {
        for (int i = 0; i < pieces.Length; i++) {
            pieces[i].SetActive(type == i ? true : false);
        }
    }
}
