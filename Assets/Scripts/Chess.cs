using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public static class ChessMoveEvaluator
{


    // Evaluate takes in the chessmove to perform, the previous chessMove nullable and the current chess setup
    // Returns a bool isValid, FullChessMove containing performed chessmove and any other piece affected (also nullable)
    public static FullChessMove Evaluate(ChessMove performedMove, ChessMove lastPerformedMove, ChessPiece[,] chessSetup, int playerColor = 0)
    {
        ChessMove additionalMove = null;
        FullChessMove result = new FullChessMove(performedMove,additionalMove,false);

        // Is this move valid on the current chess Setup with the last Performed move included (only for EP) - disregards previous castle check limitation moves

        // Invalid - Dropped on a piece of same color
        if(IsPieceDroppedOnTopOfOwnPiece(performedMove,chessSetup))
            return result;

        // Is piece going to an unreachable square
        GetValidFullMove(performedMove, chessSetup, playerColor, result);
        
        return result;         

    }

    private static bool GetValidFullMove(ChessMove performedMove, ChessPiece[,] chessSetup, int playerColor, FullChessMove result)
    {
        // Check the distination position in the setup - also check both pieces of same type
        ChessPiece movedPiece = chessSetup[performedMove.from.x, performedMove.from.y];
        ChessPiece targetPiece = chessSetup[performedMove.to.x, performedMove.to.y];

        // Sets the other piece for deletion by setting it to -1, -1 
        result.other = targetPiece != null ? new ChessMove(new Vector2Int(performedMove.to.x, performedMove.to.y), new Vector2Int(-1, -1)) : null;
        // Need to set this specific for en passent

        int pieceColor = movedPiece.Color;
        int squareStartColor = (1 + performedMove.from.x + performedMove.from.y) % 2;
        int squareEndColor = (1 + performedMove.to.x + performedMove.to.y) % 2;

        result.valid = movedPiece.Type switch {
            0 or 6 => IsValidRookMove(),
            1 or 7 => IsValidKnightMove(),
            2 or 8 => IsValidBishopMove(),
            3 or 9 => IsValidQueenMove(),
            4 or 10=> IsValidKingMove() || IsValidCastleMove(),
            _ => IsValidPawnMove()
        };

        return result.valid;

        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidRookMove()
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;
            Vector2Int moveDirection = new Vector2Int(colChange == 0 ? 0 : (colChange < 0 ? -1 : 1), rowChange == 0 ? 0 : (rowChange < 0 ? -1 : 1));
            int amtSteps = Math.Max(Math.Abs(rowChange), Math.Abs(colChange));

            // Any Rook move is valid that only changes row or column and doesnt jump over any piece, and caputers only opopnents piece
            if(rowChange != 0 && colChange != 0) // If both row and column change - invalid move
                return false; 
            if(IsJumping(moveDirection, amtSteps)) // If piece needs to jump to get there - Invalid move
                return false;
            
            return true;
        }

        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidKnightMove()
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;
            return Math.Abs(rowChange) + Math.Abs(colChange) == 3;
        }
        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidBishopMove()
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;
            Vector2Int moveDirection = new Vector2Int(Math.Sign(colChange), Math.Sign(rowChange));

            int amtSteps = Math.Abs(rowChange);
            if (Math.Abs(rowChange) != Math.Abs(colChange)) // If both change in row and column are not the same - invalid move
                return false;

            if (IsJumping(moveDirection, amtSteps)) // If piece needs to jump to get there - Invalid move
                return false;
            return true;
        }
        // Local Validat Methods - Can use the supplied move color and setup
        bool IsValidQueenMove() => IsValidBishopMove() || IsValidRookMove();

        bool IsValidCastleMove()
        {
            // Depends on rotation of board ie playerColor

            // To begin with just check for two steps from kings or queens square
            
            // Also need to check that rook is available and knight and bishop is not
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;

            // Moving up or down the board
            if(rowChange != 0) return false;

            // Not moving like a castle move
            if(colChange != 2)
                return false;

            // King and queen spot (depends on rotation of board)
            if(performedMove.from.x != 3 || performedMove.from.x != 4)
                return false;

            // Check all versions of castle
            // Can't have knight or bishop but need rook
            if (performedMove.from.x == 3 && performedMove.to.x == 1) { // player black short castle
                if (chessSetup[2, performedMove.from.y] != null || chessSetup[0, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(0, performedMove.from.y), new Vector2Int(2, performedMove.from.y));
                return true;
            }
            else if (performedMove.from.x == 3 && performedMove.to.x == 5) { // player black long castle
                if (chessSetup[4, performedMove.from.y] != null || chessSetup[6, performedMove.from.y] != null || chessSetup[7, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(7, performedMove.from.y), new Vector2Int(4, performedMove.from.y));
                return true;
            }
            else if (performedMove.from.x == 4 && performedMove.to.x == 2) { // player white long castle
                if (chessSetup[1, performedMove.from.y] != null || chessSetup[3, performedMove.from.y] != null || chessSetup[0, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(0, performedMove.from.y), new Vector2Int(3, performedMove.from.y));
                return true;
            }
            else if (performedMove.from.x == 4 && performedMove.to.x == 6) { // player white short castle
                if (chessSetup[5, performedMove.from.y] != null || chessSetup[7, performedMove.from.y] == null)
                    return false;
                // Add the rook move as other move
                result.other = new ChessMove(new Vector2Int(7, performedMove.from.y), new Vector2Int(5, performedMove.from.y));
                return true;
            }
            return false;
        }
        
        bool IsValidKingMove() // OBS - Does not check for stepping into check
        {
            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;            
            if (Math.Abs(rowChange) > 1 || Math.Abs(colChange) > 1) 
                return false;
            return true;
        }
        
        bool IsValidPawnMove() // 
        {
            int allowedMoveDirection = movedPiece.Color == 0 ? 1 : -1;
            // Inverse if player plays black
            if(playerColor == 1) {
                allowedMoveDirection *= -1;
            }

            int rowChange = performedMove.to.y - performedMove.from.y;
            int colChange = performedMove.to.y - performedMove.from.y;

            if(Math.Sign(rowChange) != Math.Sign(allowedMoveDirection)) // Moving in wrong direction
                return false;

            bool twoStepsAllowed = performedMove.from.y == 1 || performedMove.from.y == 6;


            if (Math.Abs(rowChange) != 1) {
                // If step ahead is free
                if(Math.Abs(rowChange) == 2 && twoStepsAllowed) {
                    if(chessSetup[performedMove.from.x, performedMove.from.y + allowedMoveDirection] != null)
                        return false; // Doesnt step one step forward - invalid
                    return true;
                }
                return false;
            }

            // Is one step

            if (Math.Abs(colChange) == 1) {
                // Take an oponent
                if (chessSetup[performedMove.to.x, performedMove.to.y] == null) return false; // Can only take another piece, except en passent
                return true;
            }
            if (Math.Abs(colChange) == 0) {
                if(chessSetup[performedMove.from.x, performedMove.from.y + allowedMoveDirection] != null) // Can't capture ahead
                    return false;
                return true; 
            }
            return false;
        }

        // Helper Methods
        // Jumps if ther is any piece where piece moves
        bool IsJumping(Vector2Int moveDirection, int steps)
            {
                Vector2Int checkedPos = performedMove.from;

                for (int i = 1; i < steps; i++) {
                    checkedPos = performedMove.from + moveDirection * i;
                    if (chessSetup[checkedPos.x,checkedPos.y] != null)
                        return true;
                }
                return false;
            }


    }

    private static bool IsPieceDroppedOnTopOfOwnPiece(ChessMove performedMove, ChessPiece[,] chessSetup)
    {
        // Check the distination position in the setup - also check both pieces of same type
        ChessPiece movedPiece = chessSetup[performedMove.from.x, performedMove.from.y];
        ChessPiece droppedOnPiece = chessSetup[performedMove.to.x, performedMove.to.y];
        return droppedOnPiece == null ? false : (movedPiece.Color == droppedOnPiece.Color);
    }

    public static bool Equal(ChessMove moveA,ChessMove moveB) => 
        moveA.from.x == moveB.from.x && 
        moveA.from.y == moveB.from.y && 
        moveA.to.x == moveB.to.x && 
        moveA.to.y == moveB.to.y && 
        moveA.promote == moveB.promote;
}

public struct FullChessMove
{
    public ChessMove performed;
    public ChessMove other;
    public bool valid;

    public FullChessMove(ChessMove performedIn, ChessMove otherIn, bool validIn)
    {
        performed = performedIn;
        other = otherIn;
        valid = validIn;
    }
}

public class ChessMove
{
    public Vector2Int from;
    public Vector2Int to;
    public int promote;

    public ChessMove(Vector2Int setFrom, Vector2Int SetTo, int promoteSetTo = 0)
    {
        from = setFrom;
        to = SetTo;
        promote = promoteSetTo;
    }

    public bool CheckIfCorrect(ChessMove move) => move.from.x == from.x && move.from.y == from.y && move.to.x == to.x && move.to.y == to.y && move.promote == promote;

    // public bool CheckIfCorrect(Vector2Int testFrom, Vector2Int TestTo) => (testFrom == from && TestTo == to);
    //public bool CheckIfCorrect(Vector2Int testFrom, Vector2Int TestTo, int PromoteTo) => (testFrom == from && TestTo == to && PromoteTo = promote);

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

    [SerializeField] private PiecePromotion piecePromotion;

    [SerializeField] private ChessWinNotice winNotice;


    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI problemRating;

    private const int SquareSize = 50;
    private float squareSizeScaled = 50;

    private ChessPiece[,] pieces = new ChessPiece[8,8];

    private bool dragging = false;
    private bool GameActive = false;

    private ChessPiece draggedPiece = null;
    private List<ChessMove> winCondition;
    private int playerColor = 0;

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

    private void OnEnable()
    {
        UpdateRating();
    }

    void Start()
    {
        if(isGenerator)
            return;

        PiecePromotion.Instance.OnPlayerSelect += Promote;
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

    public void LoadRandomProblem(int specificType = 0)
    {
        // Clear last problem
        ClearBoard();

        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

        // Remove any Promotion panel
        piecePromotion.HidePanel();

        
        ChessPuzzleData data = new ChessPuzzleData();
        
        if(specificType == 0)
            data = ChessProblemDatas.Instance.GetRandomProblem(Stats.ChessRating);
        else if(specificType == -1) {
            ChessProblemDatas.Instance.FindEnPassentProblem();
            //ChessProblemDatas.Instance.FindCastleProblem();
            return;
        }
        else
            data = ChessProblemDatas.Instance.GetSpecificProblem(specificType);

        
        List<Vector3Int> positions = new List<Vector3Int>();

        int[] setup = data.setup;
        for (int i = 0; i < 64; i++) {
            if (setup[i] == -1) continue;
            int row = i / 8;
            int col = i % 8;
            Vector3Int newPositionData = new Vector3Int(col, row, setup[i]);
            
            Debug.Log("* Adding a piece " + setup[i]+" on ["+col+","+row+"]");
            
            positions.Add(newPositionData);
        }

        // PLayer is inverse of the one making first move
        playerColor = setup[64] == 1 ? 0 : 1;

        // Set new list of winMoves
        winCondition = new List<ChessMove>();

        Debug.Log("");
        Debug.Log("Solution = "+ ArrayString(data.solution));

        for (int i = 0; i < data.solution.Length; i += 4) {
            // Set the solution
            Vector2Int fromWin  = new Vector2Int(data.solution[i + 0], data.solution[i + 1]);
            Vector2Int toWin    = new Vector2Int(data.solution[i + 2], data.solution[i + 3]);

            int promotion = 0;

            // If there is a promotion add it here
            if(i + 4 < data.solution.Length) {
                // There is more data that can be read
                int nextValue = data.solution[i + 4];

                // If this is a promotion add it and increment i
                if(nextValue > 7) {
                    // This is a promotion
                    // 8 = R, 9 = N, 10 = B, 11 = Q  - Is the color not needed? We know who is moving
                    promotion = nextValue;
                    Debug.Log("Adding a promotion with value "+nextValue);
                    i++;
                }
            }

            winCondition.Add(new ChessMove(fromWin, toWin, promotion));
        }


        // Create all Pieces on the game board
        CreatePieces(positions);

        GameActive = true;

        //Rating
        UpdateProblemRating(data.rating);

        StartCoroutine(AnimateComputerMove());

    }

    private string ArrayString(int[] solution)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in solution) {
            sb.Append(item.ToString()+",");
        }
        return sb.ToString();
    }

    private IEnumerator AnimateComputerMove()
    {
        yield return null; // Needed to make sure ghost hidden can be changed again even if player just disabled it
        Debug.Log("Winconditions = "+winCondition.Count);
        if(winCondition.Count == 0) {
            Debug.Log("No Conditions");
            yield break;
        }
        ChessMove oponentMove = winCondition[0];

        winCondition.RemoveAt(0);
        
        Debug.Log("Wincondition = "+oponentMove.from.x+","+oponentMove.from.y+" => "+oponentMove.to.x+","+oponentMove.to.y);



        // Do the enemys Move
        int col = oponentMove.from.x;
        int row = oponentMove.from.y;

        int colTo = oponentMove.to.x;
        int rowTo = oponentMove.to.y;

        // Get Moved Piece
        ChessPiece movedPiece = pieces[col, row];

        // Hide original item

        // Hide ghost
        movedPiece.Hide(true);

        // Show Ghost
        ghost.Hide(false);

        // Show ghost as moved piece
        ghost.ChangeType(movedPiece.Type);


        const float AnimationTime = 0.2f;
        float animationTimer = 0;

        Vector3 localFromLocation = movedPiece.transform.localPosition;
        Vector3 localToLocation = new Vector3(SquareSize / 2 + oponentMove.to.x * SquareSize, SquareSize / 2 + oponentMove.to.y * SquareSize, 0);

        // Animate Ghost
        while (animationTimer < AnimationTime) {
            float percent = animationTimer / AnimationTime;
            ghost.transform.localPosition = Vector3.Lerp(localFromLocation,localToLocation,percent);
            animationTimer += Time.deltaTime;
            yield return null;
        }

        // Hide Ghost again
        ghost.Hide(true);

        // Show Original Item if not Cathing a Piece
        movedPiece.Hide(false);

        // Promotion Handeling

        int targetRow = oponentMove.to.y;

            Debug.Log("Computer move from : [" + col + "," + row +"] => ["+ colTo + "," + rowTo +"]");
        Debug.Log("[" + col + "," + row +"] = " + movedPiece?.Type);

        //Debug.Log("MovedPiece: " + (pieces[col, row] != null)+ (pieces[row, col] != null)+ (pieces[col,8 - row] != null)+ (pieces[8 - col, row] != null));

        if (movedPiece != null) {
            Debug.Log("There is a piece here of type "+movedPiece.Type);
        }
        else {
            Debug.Log("There is no piece to move from this position - invalid solution");
        }

        // Get any replaced Piece
        ChessPiece replacedPiece = pieces[colTo, rowTo];
        Debug.Log("[" + colTo + "," +rowTo +"] = " + replacedPiece?.Type);

        if (replacedPiece != null) {
            Debug.Log("Changing target piece "+replacedPiece.Type+" at ["+ colTo + ","+ rowTo + "]");
            

            if ((movedPiece.Type == 5 || movedPiece.Type == 11) && targetRow == 0) { // white or black pawn - but since player always on its side only row 7 is a promotion
                // Fixed this for black pieces ??
                replacedPiece.SetType((playerColor == 0 ? 1 : 0) * 6 + oponentMove.promote - 8);
            }else
                replacedPiece.ChangeType(movedPiece.Type);

            Destroy(movedPiece.gameObject);
        }
        else {
            if(movedPiece == null)
                Debug.Log("NULL moved piece");
            if ((movedPiece.Type == 5 || movedPiece.Type == 11) && targetRow == 0) { // white or black pawn - but since player always on its side only row 7 is a promotion
                // Fixed this for black pieces ??
                movedPiece.SetPositionAndType(new Vector3Int(oponentMove.to.x, oponentMove.to.y, (playerColor == 0 ? 1 : 0) * 6 + oponentMove.promote - 8), localToLocation);
            }
            else
                movedPiece.SetPositionAndType(new Vector3Int(oponentMove.to.x, oponentMove.to.y, movedPiece.Type), localToLocation);
            
            // Move dragged to new position
            pieces[oponentMove.to.x, oponentMove.to.y] = movedPiece;
        }

        // Unset the moved pieces last position
        pieces[oponentMove.from.x, oponentMove.from.y] = null;



        // If castling - Currently only for W at bottom (fix so it doesnt matter later)
        if (movedPiece.ComputerCastle(oponentMove)) {
            bool leftCastle = oponentMove.to.x == 2 || oponentMove.to.x == 1;

            ChessPiece computerRook = pieces[leftCastle ? 0 : 7, 7];

            int rookNewCol = oponentMove.to.x + (leftCastle ? 1 : -1); 

            // Doesnt matter ? COmputer is always correct and these positions are changed above already
            ChessPiece computerKnight = pieces[leftCastle ? 1 : 6, 7];
            ChessPiece computerBishop = pieces[leftCastle ? 2 : 5, 7];

            if (computerRook == null) {
                Debug.Log("WARNING - castling without a rook present");
            }else {
                // Move the Rook to castle resulting position
                Vector3Int oldPosition = new Vector3Int(leftCastle ? 0 : 7, 7, computerRook.Type);
                Vector3Int newPosition = new Vector3Int(rookNewCol, 7, computerRook.Type);

                Vector3 rooksPlacement = new Vector3(SquareSize / 2 + newPosition.x * SquareSize, SquareSize / 2 + newPosition.y * SquareSize, 0);
                computerRook.SetPositionAndType(newPosition, rooksPlacement);

                Debug.Log("Moving castling rook to [" + newPosition.x + "," + newPosition.y + "]");

                // Remove the data
                pieces[oldPosition.x, oldPosition.y] = null;

                // Set new data for rook
                pieces[newPosition.x, newPosition.y] = computerRook;
            }
        }




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

        // RE-WRITING THIS

        // Determine what move player is trying to do
        // Create this as a Move

        // Check if it is a valid move - Validation needs current setup and the move to be made. If validating en passent also last move is needed (if we want to return players piece as if it is an illegal move when trying to do en passent)

        // Return the move to be made including any removed or castled piece - Maybe have a remove of a piece also be a type of move

        // CompleteChessMove

        // basic move
        // other affected piece move - Should work since there can ever only be one other affected piece thats either moved or taken (castle / captured)


        // Create The Players Move
        ChessMove playersMove = new ChessMove(new Vector2Int(sourceCol, sourceRow), new Vector2Int(targetCol, targetRow));


        if (!GameActive) return;

        if (!dragging) return;

        GetMouseLocalPosition(eventData);

        //Debug.Log("EventPosition UP = [" + eventData.position.x + " , " + eventData.position.y + "] - [" + squareHolder.transform.position.x + " , " + squareHolder.transform.position.y + "]");
        int targetCol = (int)localPosition.x / SquareSize;
        int targetRow = (int)localPosition.y / SquareSize;

        
        if (!InsideBoard(targetCol, targetRow)) {
            ReturnDraggedPiece();
            return;
        }

        // 
        dragging = false;

        int sourceCol = draggedPiece.Pos.x;
        int sourceRow = draggedPiece.Pos.y;

        // Check the target square
        ChessPiece replacedPiece = pieces[targetCol, targetRow];

        // Check if player returned the piece
        if(draggedPiece != replacedPiece) {

            Debug.Log("ReplacedItem = " + replacedPiece);
            Debug.Log("it index is  = " + pieces[targetCol, targetRow]?.Type);

            // Create The Players Move
            ChessMove playersMove = new ChessMove(new Vector2Int(sourceCol, sourceRow), new Vector2Int(targetCol, targetRow));

            // En passent - Can only happen to an empty square
            if (replacedPiece == null && draggedPiece.EnPassent(playersMove,pieces[targetCol,sourceRow])){
                Vector3Int oponentsPawnPosition = new Vector3Int(targetCol, targetRow - 1, draggedPiece.Type);
                ChessPiece opponentsPawn = pieces[targetCol, targetRow - 1];

                if (opponentsPawn == null) {
                    Debug.Log("WARNING - en passenting but no oponents pawn present");
                }
                else {

                    Debug.Log("Destroying en passent pawn at [" + targetCol + "," + (targetRow - 1) + "]");
                    // Destroy the pawn
                    Destroy(opponentsPawn.gameObject);

                    // Remove the data
                    pieces[targetCol, targetRow - 1] = null;
                }
            }

            // Castle
            if (draggedPiece.PlayerCastle(playersMove)) {
                bool leftCastle = playersMove.to.x == 2 || playersMove.to.x == 1;

                ChessPiece playerRook = pieces[leftCastle ? 0 : 7, 0];

                // Doesnt matter ? COmputer is always correct and these positions are changed above already
                ChessPiece playerKnight = pieces[leftCastle ? 1 : 6, 0];
                ChessPiece playerBishop = pieces[leftCastle ? 2 : 5, 0];

                int rookNewCol = playersMove.to.x + (leftCastle ? 1 : -1);

                if (playerRook == null) {
                    Debug.Log("WARNING - castling without a rook present");
                    ReturnDraggedPiece();
                    return;
                }

                // Have to make sure this is run before the pieces are actually moved
                if (playerKnight != null || playerBishop != null) {
                    Debug.Log("WARNING - castling with knight or Bishop in the way");
                    ReturnDraggedPiece();
                    return;
                }


                else {
                    // Move the Rook to castle resulting position
                    Vector3Int oldPosition = new Vector3Int(leftCastle ? 0 : 7, 0, playerRook.Type);
                    Vector3Int newPosition = new Vector3Int(rookNewCol, 0, playerRook.Type);

                    Vector3 rooksPlacement = new Vector3(SquareSize / 2 + newPosition.x * SquareSize, SquareSize / 2 + newPosition.y * SquareSize, 0);
                    playerRook.SetPositionAndType(newPosition, rooksPlacement);

                    Debug.Log("Moving castling rook to [" + newPosition.x + "," + newPosition.y + "]");

                    // Remove the data
                    pieces[oldPosition.x, oldPosition.y] = null;

                    // Set new data for rook
                    pieces[newPosition.x, newPosition.y] = playerRook;
                }
            }

            // Destroy the item if its there
            if (replacedPiece != null)
                Destroy(replacedPiece.gameObject);


            // Move dragged to new position
            Vector3Int piecePosition = new Vector3Int(targetCol, targetRow, draggedPiece.Type);
            draggedPiece.SetPositionAndType(piecePosition, new Vector3(SquareSize / 2 + piecePosition.x * SquareSize, SquareSize / 2 + piecePosition.y * SquareSize, 0));

            pieces[targetCol, targetRow] = draggedPiece;
            Debug.Log("it index becomes  = " + pieces[targetCol, targetRow]?.Type);

            // Unset the source position data
            pieces[sourceCol, sourceRow] = null;

            Debug.Log("Dragged the piece from [" + draggedPiece.Pos.x + ", " + draggedPiece.Pos.y + "] => [" + targetCol + "," + targetRow + "] " + (replacedPiece == null ? "" : ("Took " + replacedPiece.Type)));

            




            if ((draggedPiece.Type == 5 || draggedPiece.Type == 11) && targetRow == 7) { // white or black pawn - but since player always on its side only row 7 is a promotion

                // Show the dragged again
                draggedPiece.Hide(false);

                // Hide ghost
                ghost.Hide(true);

                // Player promotes a pawn
                OpenPromotionPanel(playersMove);

                return;
            }

            CheckMove(playersMove);
        }

        // Show the dragged again
        draggedPiece.Hide(false);

        // Hide ghost
        ghost.Hide(true);


    }

    private void ReturnDraggedPiece()
    {
        // Return it to its origin
        dragging = false;

        // Show the dragged again
        draggedPiece.Hide(false);

        // Hide ghost
        ghost.Hide(true);

        draggedPiece = null;
    }

    private void OpenPromotionPanel(ChessMove playersMove)
    {
        // Make player pick the promotion - then continue checking
        piecePromotion.gameObject.SetActive(true);

        // Do we have players color?
        piecePromotion.InitiateWithColor(playersMove, playerColor);
    }

    private void Promote(ChessMove playersMove)
    {
        Debug.Log("Promote player to "+playersMove.promote);

        // Change into this piece

        // Fixed this for black pieces ??
        draggedPiece.SetType(playerColor * 6 + playersMove.promote-8); 

        // Check for this as solution
        CheckMove(playersMove);
    }

    private void CheckMove(ChessMove playersMove)
    {
        Debug.Log("Checking Move "+playersMove.to.x+" "+playersMove.to.y+" promotion = "+playersMove.promote);

        // Check if Won        
        ChessMove winningMove = winCondition[0];
        winCondition.RemoveAt(0);

        bool correct = winningMove.CheckIfCorrect(playersMove);

        //bool correct = winningMove.CheckIfCorrect(new Vector2Int(sourceCol,sourceRow), new Vector2Int(targetCol,targetRow));

        if (isGenerator)
            return;


        // Win Notice

        if (!correct) {
            Win(false);

        }
        else if (winCondition.Count == 0) {
            Win(true);
        }
        else {
            // Correct but not last move, do next computer move
            StartCoroutine(AnimateComputerMove());
        }
    }

    private void Win(bool didWin)
    {
        Debug.Log(didWin ? "YOU WIN" : "LOST");
        GameActive = false;
        winNotice.gameObject.SetActive(true);
        winNotice.SetWin(didWin);

        // Award Rating and reward
        Stats.ChessRating += didWin ? 10 : ((Stats.ChessRating <= Stats.MinimumChessRating) ? 0 : - 20);
        UpdateRating();
    }


    // RATING
    private void UpdateRating()
    {
        playerRating.text = "Rating: " + Stats.ChessRating;
        //problemRating.text = "Problem: " + Stats.ChessRating;
    }

    private void UpdateProblemRating(int rating) => problemRating.text = rating.ToString();


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
