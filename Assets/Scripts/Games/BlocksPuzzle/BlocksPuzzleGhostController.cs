using System;
using System.Collections.Generic;
using UnityEngine;

public class BlocksPuzzleGhostController : MonoBehaviour
{
    private GameObject[][] tetrisPieces;

    [SerializeField] private MovablePiece ghost;

    [SerializeField] private GameObject ghostHolder;

    private int activeType = 0;
    public int Rotation { get; set; } = 0;

    public static BlocksPuzzleGhostController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    //public void MoveToPosition(Vector2 pos) => transform.localPosition = pos;

    public void UpdateOffset(Vector2 offset)
    {
        // Offset the pickup Point for the ghost
        ghostHolder.transform.localPosition = -offset;
    }

    public void ActivatePiece(MovablePiece piece, Vector2 offset)
    {
        // activate the visuals
        ghost.gameObject.SetActive(true);

        // Set the ghost to mimic the picked piece
        ghost.MimicTypeAndRotation(piece);

        // set the type
        activeType = (int)piece.Type - 2;

        UpdateOffset(offset);
    }

    internal void Hide()
    {
        ghost.gameObject.SetActive(false);
        activeType = -1;
    }

    internal void MimicRotation(MovablePiece piece)
    {
        // Set the ghost to mimic the picked piece
        ghost.MimicTypeAndRotation(piece);
    }
}
