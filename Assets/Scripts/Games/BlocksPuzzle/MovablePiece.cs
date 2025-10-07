using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovablePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TetrisBlockType Type = TetrisBlockType.Empty;

    public Vector2 home = Vector2.zero;

    public RectTransform RectTransform;
    public TetrisBlock[] GetAllTetrisBlocks => transform.GetComponentsInChildren<TetrisBlock>();

    private TetrisBlock[] tetrisBlocks;

    // Occupy info
    public Vector2Int[] OccupySpots { get; private set; } = new Vector2Int[0];
    public int OccupyRotation { get; set; } = 0;

    // Start Positions for Blocks
    public Vector2[] BlockPositions { get; set; }

    // Current Rotation
    public int Rotation { get; set; } = 0;


    private void Start()
    {
        RectTransform = GetComponent<RectTransform>();

        tetrisBlocks = transform.GetComponentsInChildren<TetrisBlock>();

        // Store the initial position for 0 rotation for each piece
        BlockPositions = tetrisBlocks.Select(x => (Vector2)x.transform.localPosition).ToArray();

        SetHome();
    }

    public void SetHome()
    {
        home = transform.localPosition;
    }
    public void ReturnHome()
    {
        transform.localPosition = home;
        OccupySpots = new Vector2Int[0];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Blocks Puzzle:Start Move Piece");    
        //Vector2 GridPosition = WolfheatProductions.Converter.GetMouseLocalPosition(eventData, this.GetComponentInParent<RectTransform>());
        //Debug.Log("Blocks Puzzle: Start Move Piece ["+GridPosition.x+","+GridPosition.y+"]");

        PiecesHandler.Instance.StartMovePiece(eventData, this);

        gameObject.SetActive(false);
    }

    internal void SetOccupySpots(Vector2Int[] indexPositions)
    {
        OccupySpots = indexPositions;
        OccupyRotation = Rotation;
        Debug.Log("Adding occypySpots " + OccupySpots?.Length);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Blocks Puzzle:Drop Piece");    
    }

    internal Vector2 Rotate(int rotation, Vector2 holderOffset)
    {
        Rotation = (Rotation + 4 + rotation) % 4;

        // Change the internal piecepositions instead of rotating the object
        for (int i = 0; i < tetrisBlocks.Length; i++) {
            TetrisBlock box = tetrisBlocks[i];
            box.transform.localPosition = RotatePoint90(BlockPositions[i], Rotation);
        }
        //transform.rotation = Quaternion.Euler(0, 0, Rotation * 90);

        return RotatePoint90(holderOffset,Rotation);
    }
    
    internal void SetRotation(int rotation)
    {
        Rotation = rotation;

        // Change the internal piecepositions instead of rotating the object
        for (int i = 0; i < tetrisBlocks.Length; i++) {
            TetrisBlock box = tetrisBlocks[i];
            box.transform.localPosition = RotatePoint90(BlockPositions[i], Rotation);
        }
    }
    
    internal void MimicTypeAndRotation(MovablePiece piece)
    {
        // Dont need this for the ghost???
        Rotation = piece.Rotation;

        // Get the other pieces blocks
        TetrisBlock[] otherBlocks = piece.tetrisBlocks;

        // copy each blocks position
        for (int i = 0; i < tetrisBlocks.Length; i++) {
            tetrisBlocks[i].transform.localPosition = otherBlocks[i].transform.localPosition;
            tetrisBlocks[i].SetType((int)piece.Type);
        }
    }


    // Helper function to rotate a 2D point around origin
    public static Vector2 RotatePoint90(Vector2 point, int rotation)
    {
        switch (rotation % 4) {
            case 1: // 90°
                return new Vector2(point.y, -point.x);
            case 2: // 180°
                return new Vector2(-point.x, -point.y);
            case 3: // 270°
                return new Vector2(-point.y, point.x);
            default: // 0°
                return point;
        }
    }

    internal Vector2 GetUnrotatedOffesetForPoint(Vector2 pieceOffestPosition)
    {
        return RotatePoint90(pieceOffestPosition, 4 - Rotation);
    }

    internal void ResetRotation()
    {
        Debug.Log("Active piece resetting rotation");
        SetRotation(OccupyRotation);
    }
}
