using System;
using UnityEngine;
using UnityEngine.UI;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] private ChessPieceData chessPieceData;
    [SerializeField] private Image image;
    public int Type { get; private set; }
    private bool dragging = false;

    public Vector2Int Pos { get; private set; }
    public Vector3 HomePosition { get; private set; }

    internal void SetPositionAndType(Vector3Int piecePosition, Vector3 homePos)
    {
        // Set its index
        Pos = new Vector2Int(piecePosition.x,piecePosition.y);

        // Place it correctly
        HomePosition = homePos;
        transform.localPosition = homePos;

        // Set type
        Type = piecePosition.z;

        // Set its visual
        image.sprite = chessPieceData.Sprites[Type];
    }

    internal void Hide(bool v) => image.enabled = !v;

    internal void ChangeType(int type)
    {
        Type = type;

        // Set its visual
        image.sprite = chessPieceData.Sprites[Type];
    }
}
