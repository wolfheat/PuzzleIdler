using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public struct ChessWinCondition
{
    public Vector2Int from;
    public Vector2Int to;

    public ChessWinCondition(Vector2Int setFrom, Vector2Int SetTo)
    {
        from = setFrom;
        to = SetTo;
    }
    public bool CheckIfWon(Vector2Int testFrom, Vector2Int TestTo) => (testFrom == from && TestTo == to);
}

public class Chess : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] private ChessPiece piecePrefab;
    [SerializeField] private ChessPiece ghost;

    [SerializeField] private ChessSquare squarePrefab;
    [SerializeField] private Transform pieceHolder;
    [SerializeField] private Transform squareHolder;

    private const int SquareSize = 50;
    private float squareSizeScaled = 50;

    private ChessPiece[,] pieces = new ChessPiece[8,8];

    private bool dragging = false;

    private ChessPiece draggedPiece = null;
    private ChessWinCondition winCondition;

    public static Chess Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Derive the square size at current scale
        squareSizeScaled = squareHolder.GetComponent<RectTransform>().sizeDelta.x;
        Debug.Log("**-- SquareSize = "+squareSizeScaled);

    }

    void Start()
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        Vector3Int rookPosition = new Vector3Int(0, 0, 0);
        Vector3Int pawnPosition = new Vector3Int(0, 1, 6);

        positions.Add(rookPosition);
        positions.Add(pawnPosition);

        Vector2Int fromWin = new Vector2Int(0,0);
        Vector2Int toWin = new Vector2Int(7,0);

        winCondition = new ChessWinCondition(fromWin,toWin);

        // Create Holders for each position?
        CreateSquares();

        // Create all Pieces on the game board
        CreatePieces(positions);
    }

    private void CreateSquares()
    {
        Debug.Log("**-- CHESS -Generating Squares.");

        for (int j = 0; j < 8; j++) {
            for (int i = 0; i < 8; i++) {
                ChessSquare square = Instantiate(squarePrefab, squareHolder);
                Vector2Int pos = new Vector2Int(i,j);
                square.SetToType((i+j)%2,pos);
                square.transform.localPosition = new Vector3(SquareSize/2 + i * SquareSize, SquareSize / 2 + j * SquareSize,0);
            }
        }
    }

    private void CreatePieces(List<Vector3Int> positions)
    {
        Debug.Log("**-- CHESS - Generating Pieces.");
        foreach (Vector3Int piecePosition in positions) {
            ChessPiece piece = Instantiate(piecePrefab, pieceHolder);
            piece.SetPositionAndType(piecePosition, new Vector3(SquareSize / 2 + piecePosition.x * SquareSize, SquareSize / 2 + piecePosition.y * SquareSize, 0));
            pieces[piecePosition.x, piecePosition.y] = piece;
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // Only if dragging an item do this
        if (!dragging) return;

        GetMouseLocalPosition(eventData);       

        ghost.transform.localPosition = localPosition;

    }

    private void GetMouseLocalPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            squareHolder.GetComponent<RectTransform>(),  // The RectTransform of your board
            eventData.position,
            eventData.pressEventCamera,
            out localPosition
        );
    }

    Vector2 localPosition;

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!dragging) return;

        GetMouseLocalPosition(eventData);

        //Debug.Log("EventPosition UP = [" + eventData.position.x + " , " + eventData.position.y + "] - [" + squareHolder.transform.position.x + " , " + squareHolder.transform.position.y + "]");
        int col = (int)localPosition.x / SquareSize;
        int row = (int)localPosition.y / SquareSize;
        //Debug.Log("Releasing on [" + col + "," + row + "]");

        if (!InsideBoard(col, row)) {
            // Return it to its origin
            dragging = false;

            // Show the dragged again
            draggedPiece.Hide(false);

            // Hide ghost
            ghost.Hide(true);

            draggedPiece = null;

            return;
        }

        // 
        dragging = false;

        ChessPiece replacedPiece = pieces[col, row];


        if (replacedPiece != null)
            Destroy(replacedPiece.gameObject);

        // Move dragged to new position
        pieces[col,row] = draggedPiece;

        // Unset the moved pieces last position
        pieces[draggedPiece.Pos.x, draggedPiece.Pos.y] = null;


        Debug.Log("Dragged the piece from [" + draggedPiece.Pos.x + ", " + draggedPiece.Pos.y + "] => [" + col + "," + row + "] " + (replacedPiece == null ? "" : ("Took "+replacedPiece.Type)));
        Debug.Log("Checking win condition "+winCondition.from.x+","+ winCondition.from.y+" => " + winCondition.to.x + "," + winCondition.to.y);
        bool won = winCondition.CheckIfWon(draggedPiece.Pos, new Vector2Int(col,row));

        Vector3Int piecePosition = new Vector3Int(col, row, draggedPiece.Type);
        draggedPiece.SetPositionAndType(piecePosition, new Vector3(SquareSize / 2 + piecePosition.x * SquareSize, SquareSize / 2 + piecePosition.y * SquareSize, 0));

        // Check if Won        

        // Show the dragged again
        draggedPiece.Hide(false);

        // Hide ghost
        ghost.Hide(true);

        if (won) {
            Debug.Log("YOU WIN");
        }
        else {
            Debug.Log("YOU LOSE");
        }

    }

    private bool InsideBoard(int col, int row) => col >= 0 && row >= 0 && col < 8 && row < 8;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            squareHolder.GetComponent<RectTransform>(),  // The RectTransform of your board
            eventData.position,
            eventData.pressEventCamera,
            out localPosition
        );


        //Debug.Log("EventPosition DOWN = ["+eventData.position.x+" , "+eventData.position.y+"] - [" + squareHolder.transform.position.x + " , " + squareHolder.transform.position.y + "] = ["+localPosition.x+","+localPosition.y+"]");

        int col = (int)localPosition.x / SquareSize;
        int row = (int)localPosition.y / SquareSize;
        //Debug.Log("Clicking on ["+col+","+row+"]");

        // Get the piece
        if (pieces[col,row] == null) return;

        Debug.Log("Starting to Drag a piece");
        draggedPiece = pieces[col, row];

        // Deactivate the dragged item ad show the ghost
        draggedPiece.Hide(true);

        ghost.Hide(false);

        ghost.ChangeType(draggedPiece.Type);

        // Get the mouse position and activate the ghost there
        GetMouseLocalPosition(eventData);
        ghost.transform.localPosition = localPosition;

        dragging = true;
    }

    // Have a grid for the piece positions


}
