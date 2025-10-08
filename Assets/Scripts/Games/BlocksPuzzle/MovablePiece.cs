using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BasePiece : MonoBehaviour
{
    public RectTransform RectTransform { get; private set; }

    // Current Rotation
    public int Rotation { get; set; } = 0;
    public TetrisBlock[] TetrisBlocks { get; private set; }
    // Start Positions for Blocks
    public Vector2[] BlockPositions { get; set; }


    protected virtual void Start()
    {
        TetrisBlocks = transform.GetComponentsInChildren<TetrisBlock>();
        // Store thge pieces transform and block
        RectTransform = GetComponent<RectTransform>();
        // Store the initial position for 0 rotation for each piece
        BlockPositions = TetrisBlocks.Select(x => (Vector2)x.transform.localPosition).ToArray();
    }
}
public class MovablePiece : BasePiece, IPointerDownHandler, IPointerUpHandler
{
    // Type is Set in inspector
    public TetrisBlockType Type = TetrisBlockType.Empty;        

    private Vector2 home = Vector2.zero;

    // Occupy info
    public Vector2Int[] OccupySpots { get; private set; } = new Vector2Int[0];
    public int OccupyRotation { get; set; } = 0;



    override protected void Start()
    {
        base.Start();
        // Store the piece home position
        home = transform.localPosition;
    }

    public void ReturnHome()
    {
        transform.localPosition = home;
        OccupySpots = new Vector2Int[0];
    }
    internal void SetOccupySpots(Vector2Int[] indexPositions)
    {
        OccupySpots = indexPositions;
        OccupyRotation = Rotation;
        Debug.Log("Adding occypySpots " + OccupySpots?.Length);
    }


    internal Vector2 Rotate(int rotation, Vector2 holderOffset)
    {
        Rotation = (Rotation + 4 + rotation) % 4;

        // Change the internal piecepositions instead of rotating the object
        for (int i = 0; i < TetrisBlocks.Length; i++) {
            TetrisBlock box = TetrisBlocks[i];
            box.transform.localPosition = RotatePoint90(BlockPositions[i], Rotation);
        }
        //transform.rotation = Quaternion.Euler(0, 0, Rotation * 90);

        return RotatePoint90(holderOffset,Rotation);
    }
    
    internal void SetRotation(int rotation)
    {
        Rotation = rotation;

        // Change the internal piecepositions instead of rotating the object
        for (int i = 0; i < TetrisBlocks.Length; i++) {
            TetrisBlocks[i].transform.localPosition = RotatePoint90(BlockPositions[i], Rotation);
        }
    }
    
    // Helper function to rotate a 2D point around origin
    public static Vector2 RotatePoint90(Vector2 point, int rotation)
    {
        return (rotation % 4) switch
        {
            1 => new Vector2(point.y, -point.x),
            2 => new Vector2(-point.x, -point.y),
            3 => new Vector2(-point.y, point.x),
            _ => point
        };
    }

    // Helper method for picking up a rotated piece (calculates mouse position as if the piece was unrotated)
    internal Vector2 GetUnrotatedOffesetForPoint(Vector2 pieceOffestPosition) => RotatePoint90(pieceOffestPosition, 4 - Rotation);

    internal void ResetRotation()
    {
        Debug.Log("Active piece resetting rotation");
        SetRotation(OccupyRotation);
    }

    // Mouse Pointer Interractions
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Blocks Puzzle:Start Move Piece");    
        //Vector2 GridPosition = WolfheatProductions.Converter.GetMouseLocalPosition(eventData, this.GetComponentInParent<RectTransform>());
        //Debug.Log("Blocks Puzzle: Start Move Piece ["+GridPosition.x+","+GridPosition.y+"]");

        PiecesHandler.Instance.StartMovePiece(eventData, this);

        gameObject.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Blocks Puzzle:Drop Piece");    
    }

}
