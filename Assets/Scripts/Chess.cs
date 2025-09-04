using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI.Table;

public struct ChessWinCondition
{
    public Vector2Int from;
    public Vector2Int to;

    public ChessWinCondition(Vector2Int setFrom, Vector2Int SetTo)
    {
        from = setFrom;
        to = SetTo;
    }
    public bool CheckIfCorrect(Vector2Int testFrom, Vector2Int TestTo) => (testFrom == from && TestTo == to);

    internal int[] asArray()
    {
        return new int[4] {from.x,from.y,to.x,to.y };
    }

    internal void AddSolutionSpot(Vector2Int newPos)
    {
        Vector2Int mem = to;
        to = newPos;
        from = mem;
    }
}

public class Chess : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] private bool isGenerator = false;

    [SerializeField] private ChessPiece piecePrefab;
    [SerializeField] private ChessPiece ghost;

    [SerializeField] private ChessSquare squarePrefab;
    [SerializeField] private Transform pieceHolder;
    [SerializeField] private Transform squareHolder;

    [SerializeField] private GameObject fromSelector;
    [SerializeField] private GameObject toSelector;

    private const int SquareSize = 50;
    private float squareSizeScaled = 50;

    private ChessPiece[,] pieces = new ChessPiece[8,8];

    private bool dragging = false;
    private bool GameActive = false;

    private ChessPiece draggedPiece = null;
    private List<ChessWinCondition> winCondition;

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
        /*
        List<Vector3Int> positions = new List<Vector3Int>();

        Vector3Int rookPosition = new Vector3Int(0, 0, 0);
        Vector3Int pawnPosition = new Vector3Int(0, 1, 6);

        positions.Add(rookPosition);
        positions.Add(pawnPosition);

        Vector2Int fromWin = new Vector2Int(0,0);
        Vector2Int toWin = new Vector2Int(7,0);

        winCondition = new ChessWinCondition(fromWin,toWin);

        // Create Holders for each position?
        //CreateSquares();

        // Create all Pieces on the game board
        CreatePieces(positions);
        */
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

    public void LoadRandomProblem()
    {
        // Clear last problem
        ClearBoard();

        ChessPuzzleData data = ChessProblemDatas.Instance.GetRandomProblem(Stats.ChessRating);
        // Set the Placement of all pieces
        Debug.Log("Loading chess problem: "+data.name);


        List<Vector3Int> positions = new List<Vector3Int>();

        int[] setup = data.setup;
        for (int i = 0; i < 64; i++) {
            if (setup[i] == -1) continue;
            int row = i / 8;
            int col = i % 8;
            Vector3Int newPositionData = new Vector3Int(col, row, setup[i]);
            //Debug.Log("Add a " + setup[i]+" on "+col+","+row);
            positions.Add(newPositionData);
        }

        // Set new list of winMoves
        winCondition = new List<ChessWinCondition>();

        for (int i = 0; i < data.solution.Length; i += 4) {
            // Set the solution
            Vector2Int fromWin  = new Vector2Int(data.solution[i + 0], data.solution[i + 1]);
            Vector2Int toWin    = new Vector2Int(data.solution[i + 2], data.solution[i + 3]);
            winCondition.Add(new ChessWinCondition(fromWin, toWin));
        }


        // Create all Pieces on the game board
        CreatePieces(positions);

        GameActive = true;

        StartCoroutine(AnimateComputerMove());

    }

    private IEnumerator AnimateComputerMove()
    {
        Debug.Log("Winconditions = "+winCondition.Count);
        if(winCondition.Count == 0) {
            Debug.Log("No Conditions");
            yield break;
        }
        ChessWinCondition oponentMove = winCondition[0];

        winCondition.RemoveAt(0);
        
        Debug.Log("Wincondition = "+oponentMove.from.x+","+oponentMove.from.y+" => "+oponentMove.to.x+","+oponentMove.to.y);

        yield return new WaitForSeconds(0.1f);

        // Do the enemys Move

        // Get Moved Piece
        ChessPiece movedPiece = pieces[oponentMove.from.x, oponentMove.from.y];
        if (movedPiece != null) {
            Debug.Log("There is no piece to move from this position - invalid solution");
        }

        // Get any replaced Piece
        ChessPiece replacedPiece = pieces[oponentMove.to.x, oponentMove.to.y];
        if (replacedPiece != null) {
            replacedPiece.ChangeType(movedPiece.Type);
            Destroy(movedPiece.gameObject);
        }
        else {
            if(movedPiece == null)
                Debug.Log("NULL moved piece");
            movedPiece.SetPositionAndType(new Vector3Int(oponentMove.to.x, oponentMove.to.y, movedPiece.Type), new Vector3(SquareSize / 2 + oponentMove.to.x * SquareSize, SquareSize / 2 + oponentMove.to.y * SquareSize, 0));
        }

        // Update Data Array

        // Move dragged to new position
        pieces[oponentMove.to.x, oponentMove.to.y] = movedPiece;

        // Unset the moved pieces last position
        pieces[oponentMove.from.x, oponentMove.from.y] = null;

    }




    public void ClearBoard()
    {
        // Remove all pieces

        for (int j = 0; j < pieces.GetLength(1); j++) {
            for (int i = 0; i < pieces.GetLength(0); i++) {

                if (pieces[i, j] == null) continue;
                Destroy(pieces[i, j].gameObject);
                pieces[i, j] = null;    
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
        if (!GameActive) return;

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
        if (!GameActive) return;

        if (!dragging) return;

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

        Vector3Int piecePosition = new Vector3Int(col, row, draggedPiece.Type);
        draggedPiece.SetPositionAndType(piecePosition, new Vector3(SquareSize / 2 + piecePosition.x * SquareSize, SquareSize / 2 + piecePosition.y * SquareSize, 0));



        // Show the dragged again
        draggedPiece.Hide(false);

        // Hide ghost
        ghost.Hide(true);


        // Check if Won        
        ChessWinCondition winningMove = winCondition[0];
        winCondition.RemoveAt(0);


        Debug.Log("Checking win condition "+ winningMove.from.x+","+ winningMove.from.y+" => " + winningMove.to.x + "," + winningMove.to.y);
        bool correct = winningMove.CheckIfCorrect(draggedPiece.Pos, new Vector2Int(col,row));
        
        if(isGenerator)
            return;


        if (!correct) {
            Debug.Log("YOU LOSE");
            GameActive = false;
        }
        else if(winCondition.Count == 0) {
            Debug.Log("YOU WIN");
            GameActive = false;
        }
        else {
            // Correct but not last move, do next computer move
            StartCoroutine(AnimateComputerMove());
        }

    }

    private bool InsideBoard(int col, int row) => col >= 0 && row >= 0 && col < 8 && row < 8;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GameActive && !isGenerator) return;

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


        if (isGenerator) {
            //OnPointerDownGenerator(eventData, col, row);
            return;
        }

        // Get the piece
        if (pieces[col, row] == null) return;


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
    /*
    private void OnPointerDownGenerator(PointerEventData eventData, int col, int row)
    {
        // Check if there is a placed item here that is allready this type, if so remove it


        localPosition = new Vector2(SquareSize/2 + col * SquareSize, SquareSize / 2 + row * SquareSize);

        int newType = PieceSelector.Instance.ActiveType;
        Debug.Log("Place " + newType + " on this square " + col + "," + row);

        ChessPiece placed = pieces[col, row];

        if (Inputs.Instance.PlayerControls.Player.Ctrl.IsPressed()) {
            winCondition.AddSolutionSpot(new Vector2Int(col,row));
            MarkSolutionSpots();
            return;
        }


        if (placed != null) {

            // If shift is held just copy type
            if (Inputs.Instance.PlayerControls.Player.Shift.IsPressed()) {
                PieceSelector.Instance.ChangeSelected(placed.Type);
                return;
            }
            if(placed.Type == newType) {
                // Remove it
                Destroy(placed.gameObject);
            }
            else {
                // Replace it
                placed.ChangeType(newType);
            }
        }
        else {
            // Create new At
            ChessPiece newPiece = Instantiate(piecePrefab, pieceHolder.transform);
            pieces[col, row] = newPiece;
            newPiece.ChangeType(newType);
            newPiece.transform.localPosition = localPosition;
        }
    }

    private void MarkSolutionSpots()
    {
        Vector2Int from = winCondition.from;
        Vector2Int to = winCondition.to;

        if(from.x != -1){
            fromSelector.gameObject.SetActive(true);
            Vector2 newPosition = new Vector2(from.x * SquareSize, from.y * SquareSize);
            fromSelector.transform.localPosition = newPosition;
        }
        else {
            fromSelector.gameObject.SetActive(false);
        }
        if(to.x != -1) {
            toSelector.gameObject.SetActive(true);
            Vector2 pos = new Vector2(to.x * SquareSize, to.y * SquareSize);
            toSelector.transform.localPosition = pos;
        }
        else {
            toSelector.gameObject.SetActive(false);
        }
    }
    */

    internal int[] GetBoardData()
    {
        int[] boardData = new int[64];

        int i = 0;
        for(var pieceRow = 0; pieceRow < pieces.GetLength(1); pieceRow++) {
            for(var pieceCol = 0; pieceCol < pieces.GetLength(0); pieceCol++) {
                boardData[i] = pieces[pieceCol, pieceRow] == null ? -1 : pieces[pieceCol, pieceRow].Type;
                Debug.Log("Spot ["+pieceCol+","+pieceRow+"] = " + boardData[i]);
                i++;
            }
        }
        return boardData;
    }
    /*
    internal int[] GetSolution()
    {
        int[] solution = new int[4];
        solution = winCondition.asArray();
        return solution;
    }
    */

    // Have a grid for the piece positions


}
