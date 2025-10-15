using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public bool enPassentable = false;

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
/*
public class ChessGameEngine
{
    private ChessPiece[,] pieces = new ChessPiece[8, 8];
    private ChessMove lastPerformedMove = null;
    private int playerColor = 0;
    public FullChessMove TryMakeMove(ChessMove move)
    {
        // Get result from trying to make this move
        FullChessMove fullChessMove = ChessMoveEvaluator.Evaluate(move,lastPerformedMove,pieces,playerColor);

        if (!fullChessMove.valid) return fullChessMove;

        // Apply move
        ApplyMoveToBoard(fullChessMove);
        return fullChessMove;
    }

    private void ApplyMoveToBoard(FullChessMove fullChessMove)
    {
        lastPerformedMove = fullChessMove.performed;

        // update board state only (no Unity GameObjects here)
        pieces[fullChessMove.performed.to.x, fullChessMove.performed.to.y] = pieces[fullChessMove.performed.from.x, fullChessMove.performed.from.y];
        pieces[fullChessMove.performed.from.x, fullChessMove.performed.from.y] = null;

        // Other
        if (fullChessMove.other != null) {
            // Remove this piece
            if (fullChessMove.other.to.x != -1) {
                pieces[fullChessMove.other.to.x, fullChessMove.other.to.y]  = pieces[fullChessMove.other.from.x, fullChessMove.other.from.y];
            }
            pieces[fullChessMove.other.from.x, fullChessMove.other.from.y]  = null;
        }
    }
}*/

public class Chess : MiniGameBase, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] private bool isGenerator = false;

    [SerializeField] private ChessPiece piecePrefab;
    [SerializeField] private ChessPiece ghost;

    [SerializeField] private ChessSquare squarePrefab;
    [SerializeField] private Transform pieceHolder;
    [SerializeField] private Transform squareHolder;

    [SerializeField] private GameObject fromSelector;
    [SerializeField] private GameObject toSelector;
    [SerializeField] private RectTransform chessBoardRect;

    [SerializeField] private PiecePromotion piecePromotion;
    [SerializeField] private PlayerColorView playerColorView;

    [SerializeField] private MiniGameChessWinNotice winNotice;


    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI problemRating;

    private static int SquareSize = 50;
    private float squareSizeScaled = 50;

    private ChessPiece[,] pieces = new ChessPiece[8,8];
    private int[,] setup = new int[8,8];

    private bool dragging = false;
    private bool GameActive = false;

    private ChessPiece draggedPiece = null;
    private ChessMove lastPerformedMove = null;

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

        FIndAllTextfields();

        SquareSize = (int)chessBoardRect.rect.width/8;


        // Also scale by squareSize
        ghost?.SetScale(SquareSize);

        // Derive the square size at current scale
        squareSizeScaled = squareHolder.GetComponent<RectTransform>().sizeDelta.x;
        Debug.Log("**-- SquareSize = "+squareSizeScaled);
    }

    private void FIndAllTextfields()
    {

        TMP_Text[] fields = Resources.FindObjectsOfTypeAll<TMP_Text>();
        //TMP_Text[] fields = FindObjectsOfType<TMP_Text>();
        foreach (TMP_Text field in fields) {
            if (field.font == null) {
                Debug.LogWarning($"TMP_Text '{field.name}' has no Font assigned!");
            }
        }

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

        Stats.StatsUpdated += OnStatsUpdated;
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

    private void OnStatsUpdated()
    {
        UpdateRating();
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

        // Reset the lastPerformed
        lastPerformedMove = null;


        ChessPuzzleData data = new ChessPuzzleData();
        
        if(specificType == 0)
            data = ChessProblemDatas.Instance.GetRandomProblem(Stats.MiniGameRating(GameType));
        else if(specificType == -1) {
            // Find long castle

            //ChessProblemDatas.Instance.FindEnPassentProblem();
            ChessProblemDatas.Instance.FindCastleProblem();
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
            
            //Debug.Log("* Adding a piece " + setup[i]+" on ["+col+","+row+"]");
            
            positions.Add(newPositionData);
        }

        // PLayer is inverse of the one making first move
        playerColor = setup[64] == 1 ? 0 : 1;

        // Updates the players Colors
        playerColorView.SetColor(playerColor);

        // Set new list of winMoves
        winCondition = new List<ChessMove>();

        //Debug.Log("");
        //Debug.Log("Solution = "+ ArrayString(data.solution));

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
                    //Debug.Log("Adding a promotion with value "+nextValue);
                    i++;
                }
            }

            winCondition.Add(new ChessMove(fromWin, toWin, promotion));
        }


        // Create all Pieces on the game board
        CreatePieces(positions);

        GameActive = true;

        // Problem Rating Text
        UpdateProblemRating(data.rating);

        // Perform the first computer move
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
        //Debug.Log("  COMPUTER MOVES:  moves left = " + winCondition.Count);
        if (winCondition.Count == 0) {
            Debug.Log("No Conditions");
            yield break;
        }
        ChessMove oponentMove = winCondition[0];

        winCondition.RemoveAt(0);

        //Debug.Log("Wincondition = " + oponentMove.from.x + "," + oponentMove.from.y + " => " + oponentMove.to.x + "," + oponentMove.to.y);


        // Change this to use same as player ?


        // RE-WRITING THIS

        //Hinder player from doing moves during computer move
        GameActive = false;

        // Separate enemys Move
        (int col, int row, int targetCol, int targetRow) = MoveIntoParts(oponentMove);

        // Check the target square
        ChessPiece replacedPiece = pieces[targetCol, targetRow];

        // Check if the Move is valid - Also get any changes as a FullChessMove
        // Check if it is a valid move - Validation needs current setup and the move to be made. If validating en passent also last move is needed (if we want to return players piece as if it is an illegal move when trying to do en passent)
        FullChessMove fullChessMove = ChessMoveEvaluator.Evaluate(oponentMove, lastPerformedMove, pieces, playerColor);
        // Return the move to be made including any removed or castled piece 

        if (!fullChessMove.valid) {
            // Return the piece
            Debug.Log("Enemy Move is not valid - Should never happen.");
        }

        // Here we got a valid move - Main move and other - Use it depending on player moving = direct, computer = animated
        // On Mouse Up is always player so direct

        // Store this move as last performed for next check - this only counts for the computer move, after computer move this should be reset to another value
        lastPerformedMove = fullChessMove.performed;


        // Write computer animated code to handle the same input as player code

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
            ghost.transform.localPosition = Vector3.Lerp(localFromLocation, localToLocation, percent);
            animationTimer += Time.deltaTime;
            yield return null;
        }

        // Animation is complete here

        // Hide Ghost again
        ghost.Hide(true);

        // Show Original Item if not Cathing a Piece
        movedPiece.Hide(false);

        // Handle promotion change of type                                                                                 
        if ((movedPiece.Type == 5 || movedPiece.Type == 11) && targetRow == 0) // white or black pawn - but since player always on its side only row 7 is a promotion
            movedPiece.SetPositionAndType(new Vector3Int(oponentMove.to.x, oponentMove.to.y, (playerColor == 0 ? 1 : 0) * 6 + oponentMove.promote - 8), localToLocation);
        else
            // Also need to set the moved piece position and type?
            movedPiece.SetPositionAndType(new Vector3Int(oponentMove.to.x, oponentMove.to.y, movedPiece.Type), localToLocation);



        // Other
        if (fullChessMove.other != null) {
            Debug.Log("Action for the other piece");
            // Remove this piece
            if (fullChessMove.other.to.x == -1) {
                Debug.Log("Removing the other piece on [" + fullChessMove.other.from.x + "," + fullChessMove.other.from.y + "] null = " + (pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] == null));
                if (pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] != null) {
                    Destroy(pieces[fullChessMove.other.from.x, fullChessMove.other.from.y].gameObject);
                    pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] = null;
                }
            }
            else {
                Debug.Log("Moving the other place");
                // Do the move
                ChessPiece otherPiece = pieces[fullChessMove.other.from.x, fullChessMove.other.from.y];

                // Move dragged to new position
                Vector3Int otherPosition = new Vector3Int(fullChessMove.other.to.x, fullChessMove.other.to.y, otherPiece.Type);
                otherPiece.SetPositionAndType(otherPosition, new Vector3(SquareSize / 2 + otherPosition.x * SquareSize, SquareSize / 2 + otherPosition.y * SquareSize, 0));
                Debug.Log("Nullifying position [" + fullChessMove.other.from.x + "," + fullChessMove.other.from.y + "]");

                pieces[fullChessMove.other.to.x, fullChessMove.other.to.y] = otherPiece;
                pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] = null;
            }
        }


        // Move dragged to new position
        pieces[oponentMove.to.x, oponentMove.to.y] = movedPiece;

        // Forget last value
        pieces[oponentMove.from.x, oponentMove.from.y] = null;


        GameActive = true;

    }

    private static (int col, int row, int targetCol, int targetRow) MoveIntoParts(ChessMove move) => (move.from.x,move.from.y,move.to.x,move.to.y);

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
        //Debug.Log("**-- CHESS - Generating Pieces.");
        foreach (Vector3Int piecePosition in positions) {
            ChessPiece piece = Instantiate(piecePrefab, pieceHolder);
            piece.SetPositionAndType(piecePosition, new Vector3(SquareSize / 2 + piecePosition.x * SquareSize, SquareSize / 2 + piecePosition.y * SquareSize, 0));
            pieces[piecePosition.x, piecePosition.y] = piece;
            // Also scale by squareSize
            piece.SetScale(SquareSize);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!GameActive) return;

        // Only if dragging an item do this
        if (!dragging) return;

        UpdateMouseLocalPosition(eventData);       

        ghost.transform.localPosition = localPosition;

    }

    private void UpdateMouseLocalPosition(PointerEventData eventData)
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
        // Computer Moving rook as a king castle will castle with the king at rook position - prob error in animation since it has separate logic - might be fixed when new is implemented
        // Pieces captured are not removed correctly        
        Debug.Log("  PLAYER DROPPING A PIECE.");

        // RE-WRITING THIS
        if (!GameActive) return;
        if (!dragging) return;

        // Set the locol position
        UpdateMouseLocalPosition(eventData);

        // Translate the localposition into the index position in the grid
        (int targetCol, int targetRow) = ToIndexPosition(localPosition);
        
        if (!InsideBoard(targetCol, targetRow)) {
            // If releasing outisde the board, return everything
            ShowDraggedPiece();
            return;
        }
        dragging = false;

        int sourceCol = draggedPiece.Pos.x;
        int sourceRow = draggedPiece.Pos.y;

        // Create The Players Move
        ChessMove playersMove = new ChessMove(new Vector2Int(sourceCol, sourceRow), new Vector2Int(targetCol, targetRow));
        
        // Check the piece on the target square
        ChessPiece replacedPiece = pieces[targetCol, targetRow];

        // Check if the Move is valid - Also get any changes as a FullChessMove        
        FullChessMove fullChessMove = ChessMoveEvaluator.Evaluate(playersMove, lastPerformedMove, pieces, playerColor);// Check if it is a valid move - Validation needs current setup and the move to be made. If validating en passent also last move is needed (if we want to return players piece as if it is an illegal move when trying to do en passent)
        
        // Check if the returned move is valid - if not reset
        if (!fullChessMove.valid) {
            // Return the piece
            Debug.Log("The Move is not valid.");
            // Show the dragged again
            draggedPiece.Hide(false);

            // Hide ghost
            ghost.Hide(true);
            return;
        }

        // Here we got a valid move - Main move and potentially other move
        
        // Store this move as last performed for next check - this only counts for the computer move, after computer move this should be reset to another value
        lastPerformedMove = fullChessMove.performed;

        // Other
        if(fullChessMove.other != null) {
            Debug.Log("Action for the other piece");
            // Remove this piece
            if(fullChessMove.other.to.x == -1) {
                Debug.Log("Removing the other piece on ["+ fullChessMove.other.from.x+","+ fullChessMove.other.from.y+"] null = "+(pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] == null));
                if (pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] != null) {
                    Destroy(pieces[fullChessMove.other.from.x, fullChessMove.other.from.y].gameObject);
                    pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] = null;
                }
            }
            else {
                Debug.Log("Moving the other place");
                // Do the move
                ChessPiece otherPiece = pieces[fullChessMove.other.from.x, fullChessMove.other.from.y];

                // Move dragged to new position
                Vector3Int otherPosition = new Vector3Int(fullChessMove.other.to.x, fullChessMove.other.to.y,otherPiece.Type);
                otherPiece.SetPositionAndType(otherPosition, new Vector3(SquareSize / 2 + otherPosition.x * SquareSize, SquareSize / 2 + otherPosition.y * SquareSize, 0));
                Debug.Log("Nullifying position ["+fullChessMove.other.from.x+","+fullChessMove.other.from.y+"]");

                pieces[fullChessMove.other.to.x, fullChessMove.other.to.y] = otherPiece;
                pieces[fullChessMove.other.from.x, fullChessMove.other.from.y] = null;
            }
        }


        ShowDraggedPiece();

        // Move dragged to new position
        Vector3Int piecePosition = new Vector3Int(targetCol, targetRow, draggedPiece.Type);
        draggedPiece.SetPositionAndType(piecePosition, new Vector3(SquareSize / 2 + piecePosition.x * SquareSize, SquareSize / 2 + piecePosition.y * SquareSize, 0));

        // Set the piece index
        pieces[targetCol, targetRow] = draggedPiece;

        // Unset the piece's source position
        pieces[sourceCol, sourceRow] = null;

        // Checking promotion
        if ((draggedPiece.Type == 5 || draggedPiece.Type == 11) && targetRow == 7) { // white or black pawn - but since player always on its side only row 7 is a promotion
            // Player promotes a pawn
            OpenPromotionPanel(playersMove);
            return;
        }

        draggedPiece = null;

        // Check if this was correct or not
        CheckMove(playersMove);
    }

    private static (int targetCol, int targetRow) ToIndexPosition(Vector2 localPosition) => ((int)localPosition.x / SquareSize,(int)localPosition.y / SquareSize);

    private void ShowDraggedPiece()
    {
        // Return it to its origin
        dragging = false;

        // Show the dragged again
        draggedPiece.Hide(false);

        // Hide ghost
        ghost.Hide(true);
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
            Debug.Log("MOVE PLAYED THAT WAS NOT THE SUPPLIED WINNING ONE - CHECK FOR MATE");
            // Check for checkmate move everytime and if one is played it always are winning move
            if (winCondition.Count == 0 && CheckForMate()) {
                // Last move played, check for mate
                Win(true);
            }
            else {
                Win(false);
            }
        }
        else if (winCondition.Count == 0) {
            Win(true);
        }
        else {
            // Correct but not last move, do next computer move
            StartCoroutine(AnimateComputerMove());
        }
    }

    private bool CheckForMate()
    {
        // How do I check for mate?
        if (ChessMoveEvaluator.CheckCheck(lastPerformedMove, pieces, playerColor)) {
            Debug.Log("Oponent is at check - also need to check for mate");
            if(ChessMoveEvaluator.CheckIfMate(lastPerformedMove, pieces, playerColor)) {
                Debug.Log("Oponent is in mate");
                return true;
            }
            return false;
        }
        Debug.Log("Oponent is not in check");
        return false;
    }

    private void Win(bool didWin)
    {
        Debug.Log(didWin ? "YOU WIN" : "LOST");
        GameActive = false;
        winNotice.gameObject.SetActive(true);
        winNotice.SetWin(didWin);

        // Award Rating and reward
        Stats.ChangeMiniGameRating(GameType, didWin ? Stats.ChessGameWinRatingChange : Stats.ChessGameLossRatingChange);
        
        // Stats own Event Action StatsUpdated will run and update this game panel correctly
    }


    // RATING
    private void UpdateRating()
    {
        Debug.Log("UPDATING PLAYER RATING");
        playerRating.text = "Rating: " + Stats.MiniGameRating(GameType);
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


        Debug.Log("EventPosition DOWN = ["+eventData.position.x+" , "+eventData.position.y+"] - [" + squareHolder.transform.position.x + " , " + squareHolder.transform.position.y + "] = ["+localPosition.x+","+localPosition.y+"]");

        int col = (int)localPosition.x / SquareSize;
        int row = (int)localPosition.y / SquareSize;
        //Debug.Log("Clicking on ["+col+","+row+"]");


        if (isGenerator) {
            //OnPointerDownGenerator(eventData, col, row);
            return;
        }

        Debug.Log("Trying to get the piece on ["+col+","+row+"] = " + (pieces[col,row]!=null));

        // Get the piece
        if (pieces[col, row] == null) return;


        Debug.Log("Starting to Drag a piece");
        draggedPiece = pieces[col, row];

        // Only Let player drag his own pieces
        if(draggedPiece.Color != playerColor)
            return;


        // Deactivate the dragged item ad show the ghost
        draggedPiece.Hide(true);

        ghost.Hide(false);

        ghost.ChangeType(draggedPiece.Type);

        // Get the mouse position and activate the ghost there
        UpdateMouseLocalPosition(eventData);
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
