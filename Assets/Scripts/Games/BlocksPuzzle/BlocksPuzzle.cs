using System;
using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlocksPuzzle : MiniGameBase
{
    public const int BlockSize = 30;
    private const int GameSize = 8;
    public const int BlockScale = 1;

    int[,] board = new int[GameSize, GameSize];
    TetrisBlock[,] boardBlocks = new TetrisBlock[GameSize,GameSize];

    [SerializeField] private MovablePiece[] movablePiecePrefabs;

    [SerializeField] private GameObject pieceHolder;
    [SerializeField] private Transform[] pieceHolderPositions;

    [SerializeField] private GameObject boxHolder;
    [SerializeField] private TetrisBlock boxPrefab;

    // Extra Panels
    [SerializeField] private GameObject helpInfo;
    [SerializeField] private MiniGameChessWinNotice winNotice;

    // Rating Textfields
    [SerializeField] private TextMeshProUGUI nextRatingGain;

    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;

    // Debug Texfields
    [SerializeField] private TextMeshProUGUI levelCreateTextfield;

    [SerializeField] private Toggle snapToggle;

    public static BlocksPuzzle Instance { get; private set; }

    public bool GameActive { get; set; }
    public static bool Snap { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        UpdateRating();

        // read snap toggle setting
        UpdateSnap();
    }

    public void UpdateSnap()
    {
        Snap = snapToggle.isOn;
    }

    private void Start()
    {
        Stats.StatsUpdated += OnStatsUpdated;

        RestartGame();
    }

    private void OnStatsUpdated()
    {
        UpdateRating();
    }


    private void UpdateRating()
    {
        Debug.Log("Blocks: Updating Rating");
        playerRating.text = Stats.MiniGameRating(GameType).ToString();

        // Update next rating value

        int ratingGain = Stats.BlockPuzzleRatingGain();

        nextRatingGain.text = ratingGain.ToString();

    }

    public void RestartGame(bool resetPosition = false)
    {
        Debug.Log("BlocksPuzzle: Restart Game");

        //ResetBoxes();


        // Also handle all placable Pieces
        //ResetAllPieces();


        // Load a predefined easy level
        LoadEasyLevel();

    }
    
    public void RestartGeneratedGame()
    {
        Debug.Log("BlocksPuzzle: Restart Generated Game");

        // Load a generated level

        int diff = 5 + (Stats.MiniGameRatings[(int)GameType] - 1000) / 200;

        Debug.Log("rating 1000 = "+ (5 + (1000 - 1000) / 200));
        Debug.Log("rating 1400 = "+ (5 + (1400 - 1000) / 200));
        Debug.Log("rating 1700 = "+ (5 + (1700 - 1000) / 200));
        Debug.Log("rating 1999 = "+ (5 + (1999 - 1000) / 200));
        Debug.Log("rating 2000 = "+ (5 + (2000 - 1000) / 200));
        Debug.Log("rating 2100 = "+ (5 + (2100 - 1000) / 200));

        // Level depends on rating, difficulty is the amount of pieces that should go from 1000 rating = 5 to 3000 = 10 ?

        GenerateLevel(diff);
    }

    private void GenerateLevel(int difficulty)
    {
        // Generate the level and pieces


        


        (bool[,] level, int[] pieces) = BlockPuzzleProblemDatas.Instance.GenerateRandomLevel(difficulty);

        LoadLevel(level,pieces);
    }

    public void LoadLevelCreate()
    {
        bool[,] level = GetEmptyLevelBoard();

        // Load empty area
        int[] pieces = { 1, 1, 1, 1, 1, 1, 1 };
        // Load all pieces
        LoadLevel(level, pieces);
    }

    public static bool[,] GetEmptyLevelBoard()
    {
        bool[,] level = new bool[16, 16];


        for (int j = 0; j < level.GetLength(1); j++) {
            for (int i = 0; i < level.GetLength(0); i++) {
                level[i, j] = true;
            }
        }

        return level;
    }

    public void LoadEasyLevel()
    {
        (bool[,] gameAreaLoaded, int[] piecesLoaded) = BlockPuzzleProblemDatas.Instance.GetRandomEasyLevel();
        LoadLevel(gameAreaLoaded, piecesLoaded);
    }

    private void LoadPieces(int[] piecesLoaded)
    {
        RemoveAllPieces();

        // Remove all current pieces and add the new ones
        int index = 0;
        for (int i = 0; i < piecesLoaded.Length; i++) {
            for (int j = 0; j < piecesLoaded[i]; j++) {
                MovablePiece newPiece = Instantiate(movablePiecePrefabs[i],pieceHolder.transform);
                newPiece.SetHome(pieceHolderPositions[index].position);
                index++;
            }   
        }

        //Debug.Log("First actual position is [" + pieceHolderPositions[0].position.x + "," + pieceHolderPositions[0].position.y + "]");
        //Debug.Log("First local position is [" + pieceHolderPositions[0].localPosition.x + "," + pieceHolderPositions[0].localPosition.y + "]");

    }

    private void RemoveAllPieces()
    {
        foreach (var movablePiece in pieceHolder.GetComponentsInChildren<MovablePiece>()) {
            Destroy(movablePiece.gameObject);
        }
    }

    public void LoadEasyLevelB()
    {
        (bool[,] gameAreaLoaded, int[] piecesLoaded) = BlockPuzzleProblemDatas.Instance.LoadEasyB();

        LoadLevel(gameAreaLoaded, piecesLoaded);
    }

    private void LoadLevel(bool[,] gameAreaLoaded, int[] piecesLoaded)
    {
        GameActive = true;

        // Load gameArea
        ResetBoxes(gameAreaLoaded);
        LoadPieces(piecesLoaded);

        // Reset Ghost
        PiecesHandler.Instance.ResetGame();

        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

    }

    [ContextMenu("Game Area string Print")]
    public void PrintLevelGameAreaString()
    {
        string gameAreaString = ReadOccupiedSpotsAsGameAreaString();
        Debug.Log("GameArea = "+gameAreaString);

        levelCreateTextfield.text = gameAreaString;
    }

    private string ReadOccupiedSpotsAsGameAreaString()
    {
        // Decoding is done
        // alternating occypied / not occupied as a = 0
        StringBuilder sb = new StringBuilder();

        Debug.Log("BlocksPuzzle: Creating Boxes");
        int count = 0;
        bool readingAsOccupied = true;

        // Seven different pieces that can be placed, check how many are of each type
        int[] pieces = new int[7];

        // Fix for first non occupied
        if(board[0, 0] != ((int)TetrisBlockType.Ghost)) {
            readingAsOccupied = !readingAsOccupied;
        }

        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(1); j++) {
                bool spotOccupied = board[j, i] != ((int)TetrisBlockType.Ghost);
                
                Debug.Log("Reading = ["+j+","+i+"] = "+(spotOccupied?1:0));

                // Count pieces
                if (spotOccupied) {
                    int type = (int)(board[j, i]-2);
                    pieces[type]++;
                }

                // Found what we looking for
                if (spotOccupied == readingAsOccupied ) {
                    count++;
                    Debug.Log("Count = "+count);
                    continue;
                }

                // Swap
                Debug.Log("Append = "+count+" => "+ CountToChar(count, readingAsOccupied));
                sb.Append(CountToChar(count,readingAsOccupied));
                readingAsOccupied = !readingAsOccupied;
                count = 1;
            }
        }

        // Read last
        if (count > 0) {
            Debug.Log("Last append = "+count+" => "+ CountToChar(count, readingAsOccupied));
            sb.Append(CountToChar(count, readingAsOccupied));
        }

        // Pieces
        StringBuilder sb2 = new StringBuilder();
        foreach (int amt in pieces) {
            int pieceAmt = amt/4;
            sb2.Append(pieceAmt == 0?'.':(char)('A'+(pieceAmt - 1)));
        }
        sb2.Append(',');
        
        return sb2.ToString() + sb.ToString();

        static string CountToChar(int count, bool readingAsOccupied)
        {
            // Occupied = 'A' , Unoccupied = 'a'
            StringBuilder sb = new StringBuilder();
            while (count > 0) {
                int characterVal = (count > 26 ? 26 : count);
                sb.Append((char)((readingAsOccupied ? 'A':'a') + (characterVal-1)));
                count -= characterVal;
            }
            return sb.ToString();
        }
    }

    private void ResetAllPieces()
    {
        foreach (var block in pieceHolder.GetComponentsInChildren<MovablePiece>()) {
            block.ReturnHome();
        }
    }

    private void ResetBoxes()
    {
        Debug.Log("BlocksPuzzle: Creating Boxes");
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(1); j++) {
                board[j, i] = (int)TetrisBlockType.Ghost;
                if(boardBlocks[j, i] == null)
                    boardBlocks[j, i] = Instantiate(boxPrefab, boxHolder.transform);
                boardBlocks[j, i].SetType(board[j,i]);
                boardBlocks[j, i].transform.localScale = new Vector3(BlockScale, BlockScale);
                boardBlocks[j, i].transform.localPosition = new Vector3(i*BlockSize* BlockScale, -j*BlockSize* BlockScale);
            }
        }
    }
    
    private void ResetBoxes(bool[,] levelToLoad)
    {
        Debug.Log("BlocksPuzzle: Creating Boxes");
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(1); j++) {
                board[j, i] = levelToLoad[j,i]? (int)TetrisBlockType.Ghost : (int)TetrisBlockType.Fixed;
                if(boardBlocks[j, i] == null)
                    boardBlocks[j, i] = Instantiate(boxPrefab, boxHolder.transform);
                boardBlocks[j, i].SetType(board[j,i]);
                boardBlocks[j, i].transform.localScale = new Vector3(BlockScale, BlockScale);
                boardBlocks[j, i].transform.localPosition = new Vector3(i*BlockSize* BlockScale, -j*BlockSize* BlockScale);
            }
        }
    }

    private bool InsideBoard(int col, int row) => col >= 0 && row >= 0 && col < GameSize && row < GameSize;

    internal bool TryPlacePiece(MovablePiece activePiece)
    {
        // Need to read the tiles parts to see if any of them collide with any piece on the board
        // Need to know the main box in the piece and its position and use its rotation to know what boxes that will be placed

        TetrisBlock[] blocks = activePiece.TetrisBlocks;

        // Try to only work in local reference system

        // All blocks local position inside the piece - Can work with these offsets if valid after rotation
        Vector2[] positions = blocks.Select(x => new Vector2(x.transform.localPosition.x, x.transform.localPosition.y)).ToArray();

        // Works with placing the piece at the correct offset
        Vector2 pieceLocalGameAreaDropPosition = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>()) - PiecesHandler.Instance.Offset;


        // WorldPiecePosition
        Vector2 worldPiecePosition = GetComponent<RectTransform>().TransformPoint(pieceLocalGameAreaDropPosition);

        //

        float rotation = activePiece.transform.localEulerAngles.z;
        float radians = rotation * Mathf.Deg2Rad;

        // Helper function to rotate a 2D point around origin
        Vector2 RotatePoint90(Vector2 point, int rotation)
        {
            switch (rotation % 4) {
                case 1: // 90°
                    return new Vector2(-point.y, point.x);
                case 2: // 180°
                    return new Vector2(-point.x, -point.y);
                case 3: // 270°
                    return new Vector2(point.y, -point.x);
                default: // 0°
                    return point;
            }
        }

        /*
        // Compute rotated positions for each block
        Vector2Int[] indexPositions = blocks.Select(x => {
            Vector2 rotated = RotatePoint90(x.transform.localPosition, activePiece.Rotation);
            return new Vector2Int(
                Mathf.FloorToInt((rotated.x + pieceLocalGameAreaDropPosition.x) / (BlockSize * BlockScale)),
                -Mathf.FloorToInt((rotated.y + pieceLocalGameAreaDropPosition.y) / (BlockSize * BlockScale)) - 1
            );
        }).ToArray();
        */


        Vector2 rotated0 = RotatePoint90(blocks[0].transform.localPosition, activePiece.Rotation);

        // Get the indexes the piece will occupy - WORKS
        Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(Mathf.FloorToInt((x.transform.localPosition.x + pieceLocalGameAreaDropPosition.x) / (BlockSize * BlockScale)),-Mathf.FloorToInt((x.transform.localPosition.y + pieceLocalGameAreaDropPosition.y) / (BlockSize * BlockScale)) - 1)).ToArray();


        Vector2 block0dropPositionRotated = new Vector2(rotated0.x + pieceLocalGameAreaDropPosition.x, rotated0.y + pieceLocalGameAreaDropPosition.y);

        Vector2 block0dropPosition = new Vector2(blocks[0].transform.localPosition.x + pieceLocalGameAreaDropPosition.x, blocks[0].transform.localPosition.y + pieceLocalGameAreaDropPosition.y);

        //Vector2 adjusted = new Vector2(indexPositions[0].x*BlockSize, -indexPositions[0].y*BlockSize);
        Vector2 adjusted = new Vector2((BlockSize / 2) + indexPositions[0].x*BlockSize, -(BlockSize / 2) -indexPositions[0].y*BlockSize);

        // Figure out how much this moves the piece
        Vector2 pieceMoveDelta = (adjusted - block0dropPosition)*1.5f;

        Vector2 gameAreaPiecePosition = worldPiecePosition + pieceMoveDelta;

        // Now convert from world space -> piece parent local space
        Vector2 localPosInPieceParent = activePiece.transform.parent.GetComponent<RectTransform>().InverseTransformPoint(gameAreaPiecePosition);

        // Now check these positions
        (bool valid, bool outside) = ValidatePlacementPosition(indexPositions);

        if (valid) {
            PlacePieceOnValidSpot(activePiece, indexPositions);

            // DOES NOT WORK
            activePiece.transform.localPosition = localPosInPieceParent;
            //activePiece.transform.position = worldPiecePosition;

            // Evaluate if winning the game
            if (EvaluateWin())
                Win(true);


        }
        else {
            // Replace on old valid spot if there is one

            if (outside) {
                // Outside - Return Home
                activePiece.ReturnHome();
                return false;
            }

            // Inside return to previously placement inside
            OccupySpots(activePiece,activePiece.OccupySpots);
            activePiece.ResetRotation();
            return false;
            
        }
        return valid;
    }

    private bool EvaluateWin()
    {
        // Check if any value is unplaced
        for (int j = 0; j < board.GetLength(0); j++) {
            for (int i = 0; i < board.GetLength(1); i++) {
                if (board[i, j] == (int)TetrisBlockType.Ghost)
                    return false;
            }
        }
        return true;
    }

    private void Win(bool didWin)
    {
        Debug.Log(didWin ? "YOU WIN" : "LOST");

        Debug.Log("YOU LOSE");
        GameActive = false;

        // Also make this general?
        winNotice.gameObject.SetActive(true);
        winNotice.SetWin(didWin);

        // Added Rating

        // Let the rating player got be the added value?

        int ratingAchieved = Stats.BlockPuzzleRatingGain();
        int currentRating = Stats.MiniGameRatings[(int)GameType];

        Stats.GemGain(GameType);


        int increase = Math.Min(2999, currentRating + ratingAchieved) - currentRating;

        if (increase <= 0) {
            // Do not change rating if not better than current
            return;
        }

        // Award Rating and reward
        Stats.ChangeMiniGameRating(GameType, ratingAchieved);

        // Popup - also make reusable TODO
        ShowRatingIncreaseText(increase);
    }

    private void ShowRatingIncreaseText(int increase)
    {
        StartCoroutine(ShowRatingIncreaseCO(increase));

    }

    private IEnumerator ShowRatingIncreaseCO(int increase)
    {
        playerRatingIncrease.SetActive(true);
        playerRatingIncreaseText.text = "+" + increase;

        //Time the visability

        float timer = 2f;

        // Handle fade

        while (timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }

        playerRatingIncrease.SetActive(false);

    }

    /*
    internal bool TryPlacePiece(MovablePiece activePiece)
    {


        // check for out of bounds here 

        // Figure out delta From mouse to the placement

        //Debug.Log("Trying to place piece "+activePiece+ " at position [" + endPosIndex.x + "," + endPosIndex.y + "]");

        // Need to read the tiles parts to see if any of them collide with any piece on the board
        // Need to know the main box in the piece and its position and use its rotation to know what boxes that will be placed

        TetrisBlock[] blocks = activePiece.GetAllTetrisBlocks;

        // Try to only work in local reference system

        // All blocks local position inside the piece - Can work with these offsets if valid after rotation
        Vector2[] positions = blocks.Select(x => new Vector2(x.transform.localPosition.x, x.transform.localPosition.y)).ToArray();


        // The mouse inside moving offset is stored in the PieceHandler each time a piece is picked up
        // PiecesHandler.Instance.Offset

        // Figure out 
        // Figure out what indeces the piece will occypy of dropped

        // We know those indeces from the positions of the blocks inside the piece where at the piece we picked it up and where on the gamearea the mouse are

        // Works with placing the piece at the correct offset
        Vector2 pieceLocalGameAreaDropPosition = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>()) - PiecesHandler.Instance.Offset;

        // WorldPiecePosition
        Vector2 worldPiecePosition = GetComponent<RectTransform>().TransformPoint(pieceLocalGameAreaDropPosition);

        Debug.Log("WorldPosition = ["+(int)worldPiecePosition.x+" , "+ (int)worldPiecePosition.y+"]");
        
        
        



        // Get the indexes the piece will occupy - WORKS
        Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(Mathf.FloorToInt((x.transform.localPosition.x + pieceLocalGameAreaDropPosition.x) / (BlockSize * BlockScale)),-Mathf.FloorToInt((x.transform.localPosition.y + pieceLocalGameAreaDropPosition.y) / (BlockSize * BlockScale)) - 1)).ToArray();

        //Debug.Log("Piece drop at local position = [" + pieceLocalGameAreaDropPosition.x+","+ pieceLocalGameAreaDropPosition.y+"]");
        //Debug.Log("Block 0 at new index position = [" + indexPositions[0].x+","+ indexPositions[0].y+"]");

        Vector2 block0dropPosition = new Vector2((blocks[0].transform.localPosition.x + pieceLocalGameAreaDropPosition.x), (blocks[0].transform.localPosition.y + pieceLocalGameAreaDropPosition.y));


        Debug.Log("Block 0 dropped at position = [" + block0dropPosition.x + ","+ block0dropPosition.y + "]");
        Debug.Log("Block 0 dropped at index = [" + indexPositions[0].x + ","+ indexPositions[0].y + "]");

        Vector2 adjusted = new Vector2((BlockSize / 2) + indexPositions[0].x*BlockSize, -(BlockSize / 2) -indexPositions[0].y*BlockSize);

        Debug.Log("Block 0 dropped adjusted pos= [" + adjusted.x + ","+ adjusted.y + "]");

        
        // Figure out how much this moves the piece
        Vector2 pieceMoveDelta = adjusted - block0dropPosition;

        Debug.Log("PieceMoveDelta = ["+ (int)pieceMoveDelta.x+","+ (int)pieceMoveDelta.y+"] = should be +15, +15 ??");

        //Vector2 gameAreaPosition = transform.position;
        //Debug.Log("Game Area WorldPosition = ["+ gameAreaPosition.x+" , "+ gameAreaPosition.y+"]");

        Vector2 gameAreaPiecePosition = worldPiecePosition + pieceMoveDelta;

        Debug.Log("Piece adjusted WorldPosition = ["+ (int)gameAreaPiecePosition.x+" , "+ (int)gameAreaPiecePosition.y+"]");


        Debug.Log("Local Block as WorldPosition = ["+ (int)worldPiecePosition.x+" , "+ (int)worldPiecePosition.y+"]");
        
        // Adjusted piece local position
        //Vector2 pieceAdjustedLocalPosition = worldPiecePosition;
        //Vector2 pieceAdjustedLocalPosition = worldPiecePosition - pieceMoveDelta;

        //Debug.Log("Adjusted WorldPosition = ["+worldPiecePosition.x+" , "+worldPiecePosition.y+"]");

        // Get this position in the piece own reference system so it can be placed accordingly


        // Convert from game area local space -> world space
        //Vector3 worldDropPos = GetComponent<RectTransform>().TransformPoint(pieceAdjustedLocalPosition);

        // Now convert from world space -> piece parent local space
        Vector2 localPosInPieceParent = activePiece.transform.parent.GetComponent<RectTransform>().InverseTransformPoint(gameAreaPiecePosition);

        // Get the piece position on the pieceHolder for this placement
        Vector2 placePos = localPosInPieceParent;
        //Vector2 placePos = WolfheatProductions.Converter.GetMouseLocalPosition(activePiece.transform.parent.GetComponent<RectTransform>(),pieceLocalGameAreaDropPosition +(Vector2)GetComponent<RectTransform>().position);


        //Vector2 mousePieceHolderPosition = WolfheatProductions.Converter.GetMouseLocalPosition(activePiece.transform.parent.GetComponent<RectTransform>());

        //Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(Mathf.RoundToInt(x.transform.position.x / (BlockSize * BlockScale)), Mathf.RoundToInt(x.transform.position.y / (BlockSize * BlockScale)))).ToArray();

        //foreach (var block in blocks) {
        //    //Debug.Log("Piece box actually at [" + (mouseLocalPosition.x + block.transform.localPosition.x) + "," + (mouseLocalPosition.y + block.transform.localPosition.y ) + "]");
        //}




        // Generated worldposition method
        //Vector2[] worldPositions = blocks.Select(block =>pieceLocalGameAreaPosition + (Vector2)block.transform.localPosition).ToArray();

        // Generated indeces method
        //Vector2Int[] gridIndices = worldPositions.Select(pos => new Vector2Int(Mathf.FloorToInt(pos.x / (BlockSize * BlockScale)),-Mathf.FloorToInt(pos.y / (BlockSize * BlockScale)) - 1)).ToArray();


        // Figure out movement from dropped position to the end position - Checking only one box
        //Vector2 snappedPosition = new Vector2(gridIndices[0].x * (BlockSize * BlockScale), -gridIndices[0].y * (BlockSize * BlockScale));

        //Vector2 localPiecePos = (Vector2)blocks[0].transform.localPosition;
        
        //Vector2 delta = snappedPosition - (pieceLocalGameAreaPosition + localPiecePos);
        
        //Vector2 delta = snappedPosition - (pieceLocalPosition + localPiecePos) + new Vector2(0.5f, -0.5f) * BlockSize * BlockScale;
        
        //Vector2 delta = snappedPosition - (mouseLocalPosition + localPiecePos) + new Vector2(0.5f,-0.5f);


        //Vector2 delta = new Vector2(indexPositions[0].x * (BlockSize * BlockScale) - positions[0].x, indexPositions[0].y * (BlockSize * BlockScale) - positions[0].y);

        //Vector2 placePos = snappedPosition + (Vector2)transform.localPosition;
        //Vector2 placePos = mousePieceHolderPosition;
        //Vector2 placePos = pieceLocalGameAreaPosition - delta;    

        // Place pos is wrong somehow


        // Now check these positions
        (bool valid, bool outside) = ValidatePlacementPosition(indexPositions);

        if (valid) {
            PlacePieceOnValidSpot(activePiece, indexPositions, placePos);

            // DOES NOT WORK
            activePiece.transform.localPosition = placePos;
            //activePiece.transform.localPosition = (Vector2)activePiece.transform.localPosition - pieceMoveDelta;
        }
        else {
            // Replace on old valid spot if there is one

            if (outside) {
                // Outside - Return Home
                activePiece.ReturnHome();
                return false;
            }

            // Inside return to previously placement inside
            OccupySpots(activePiece,activePiece.OccupySpots);
            return false;
            
        }
        return valid;
    }
    */


    private (bool valid, bool outside) ValidatePlacementPosition(Vector2Int[] indexPositions)
    {
        foreach (var pos in indexPositions) {
            if (!InsideBoard(pos.x, pos.y)) {
                //Debug.Log("Blocks: Piece Outside [" + pos.x + "," + pos.y + "]");
                return (false,true);
            }

            if (board[pos.y, pos.x] != (int)TetrisBlockType.Ghost) {
                Debug.Log("Blocks: Piece not Empty [" + pos.x + "," + pos.y + "] = "+ board[pos.y, pos.x]+" = "+ (TetrisBlockType)board[pos.y, pos.x]);
                return (false, false);
            }
            //Debug.Log("Blocks: Valid placement board[" + pos.x + "," + pos.y + "] = " + board[pos.y, pos.x]);
        }

        return (true, false);
    }

    private void PlacePieceOnValidSpot(MovablePiece activePiece, Vector2Int[] indexPositions)
    {
        // Occupy board
        OccupySpots(activePiece, indexPositions);

        // Save the board positions to the piece
        activePiece.SetOccupySpots(indexPositions);

        // Also move the original piece here and show it
        activePiece.gameObject.SetActive(true);
        //activePiece.transform.localPosition = placePos;
    }

    private void OccupySpots(MovablePiece activePiece, Vector2Int[] indexPositions)
    {
        if (indexPositions == null) return;

        foreach (var pos in indexPositions) {
            board[pos.y, pos.x] = (int)activePiece.Type;
            boardBlocks[pos.y, pos.x].SetType(board[pos.y, pos.x]);
            //Debug.Log("Blocks:  Piece [" + pos.x + "," + pos.y + "] => " + board[pos.y, pos.x]);
        }
    }

    internal void ClearBoardSpots(MovablePiece activePiece)
    {
        Vector2Int[] occypySpots = activePiece.OccupySpots;

        if(occypySpots == null || occypySpots.Length == 0) return;

        //Debug.Log("Clearing all occypySpots "+occypySpots?.Length);

        if (occypySpots != null) {
            foreach (Vector2Int pos in occypySpots) {
                board[pos.y, pos.x] = (int)TetrisBlockType.Ghost;
                boardBlocks[pos.y, pos.x].SetType(board[pos.y, pos.x]);

                //Debug.Log("Blocks: Unsetting [" + pos.x + "," + pos.y + "] = " + board[pos.y, pos.x]);
            }
        }
    }
}

//public class BlocksPuzzle : MonoBehaviour
//{
//    public const int BlockSize = 30;
//    private const int GameSize = 8;
//    public const int BlockScale = 1;

//    int[,] board = new int[GameSize, GameSize];
//    TetrisBlock[,] boardBlocks = new TetrisBlock[GameSize,GameSize];

//    [SerializeField] private GameObject boxHolder;
//    [SerializeField] private TetrisBlock boxPrefab;

//    public static BlocksPuzzle Instance { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null) {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//    }

//    private void Start()
//    {
//        RestartGame();
//    }

//    public void RestartGame(bool resetPosition = false)
//    {
//        Debug.Log("BlocksPuzzle: Restart Game");

//        ResetBoxes();
//    }

//    private void ResetBoxes()
//    {
//        Debug.Log("BlocksPuzzle: Creating Boxes");
//        for (int i = 0; i < board.GetLength(0); i++) {
//            for (int j = 0; j < board.GetLength(1); j++) {
//                board[j, i] = 1;
//                boardBlocks[j, i] = Instantiate(boxPrefab, boxHolder.transform);
//                boardBlocks[j, i].SetType(1);
//                boardBlocks[j, i].transform.localScale = new Vector3(BlockScale, BlockScale);
//                boardBlocks[j, i].transform.localPosition = new Vector3(i*BlockSize* BlockScale, -j*BlockSize* BlockScale);
//            }
//        }
//    }

//    private bool InsideBoard(int col, int row) => col >= 0 && row >= 0 && col < GameSize && row < GameSize;

//    internal bool TryPlacePiece(MovablePiece activePiece)
//    {
//        // check for out of bounds here 

//        // Figure out delta From mouse to the placement

//        //Debug.Log("Trying to place piece "+activePiece+ " at position [" + endPosIndex.x + "," + endPosIndex.y + "]");

//        // Need to read the tiles parts to see if any of them collide with any piece on the board
//        // Need to know the main box in the piece and its position and use its rotation to know what boxes that will be placed

//        TetrisBlock[] blocks = activePiece.GetAllTetrisBlocks;

//        foreach (var block in blocks) {
//            // Debug.Log("Piece has a box offset at ["+block.transform.localPosition.x+","+block.transform.localPosition.y+"]");
//        }
//        // Transpose this to the game area?

//        Vector2 pieceLocalPosition = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>()) + PiecesHandler.Instance.Offset;

//        Vector2 mousePieceHolderPosition = WolfheatProductions.Converter.GetMouseLocalPosition(activePiece.transform.parent.GetComponent<RectTransform>());

//        Vector2[] positions = blocks.Select(x => new Vector2(x.transform.localPosition.x, x.transform.localPosition.y)).ToArray();
//        //Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(Mathf.RoundToInt(x.transform.position.x / (BlockSize * BlockScale)), Mathf.RoundToInt(x.transform.position.y / (BlockSize * BlockScale)))).ToArray();

//        foreach (var block in blocks) {
//            //Debug.Log("Piece box actually at [" + (mouseLocalPosition.x + block.transform.localPosition.x) + "," + (mouseLocalPosition.y + block.transform.localPosition.y ) + "]");
//        }

//        Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(
//             Mathf.FloorToInt((x.transform.localPosition.x + pieceLocalPosition.x) / (BlockSize * BlockScale)),
//            -Mathf.FloorToInt((x.transform.localPosition.y + pieceLocalPosition.y) / (BlockSize * BlockScale)) - 1)).ToArray();


//        Vector2[] worldPositions = blocks.Select(block =>
//    pieceLocalPosition + (Vector2)block.transform.localPosition
//).ToArray();

//        Vector2Int[] gridIndices = worldPositions.Select(pos => new Vector2Int(
//    Mathf.FloorToInt(pos.x / (BlockSize * BlockScale)),
//    -Mathf.FloorToInt(pos.y / (BlockSize * BlockScale)) - 1
//)).ToArray();

//        Vector2 snappedPosition = new Vector2(gridIndices[0].x * (BlockSize * BlockScale), -gridIndices[0].y * (BlockSize * BlockScale));

//        Vector2 localPiecePos = (Vector2)blocks[0].transform.localPosition;
//        Vector2 delta = snappedPosition - (pieceLocalPosition + localPiecePos) + new Vector2(0.5f, -0.5f) * BlockSize * BlockScale;
//        //Vector2 delta = snappedPosition - (mouseLocalPosition + localPiecePos) + new Vector2(0.5f,-0.5f);


//        //Vector2 delta = new Vector2(indexPositions[0].x * (BlockSize * BlockScale) - positions[0].x, indexPositions[0].y * (BlockSize * BlockScale) - positions[0].y);

//        //Vector2 placePos = mousePieceHolderPosition;
//        Vector2 placePos = mousePieceHolderPosition + delta;


//        // Now check these positions
//        (bool valid, bool outside) = ValidatePlacementPosition(indexPositions);

//        if(valid)
//            PlacePieceOnValidSpot(activePiece, indexPositions, placePos);
//        else {
//            // Replace on old valid spot if there is one

//            if (outside) {
//                // Outside - Return Home
//                activePiece.ReturnHome();
//                return false;
//            }

//            // Inside return to previously placement inside
//            OccupySpots(activePiece,activePiece.OccupySpots);
//            return false;
            
//        }
//        return valid;
//    }

//    private (bool valid, bool outside) ValidatePlacementPosition(Vector2Int[] indexPositions)
//    {
//        foreach (var pos in indexPositions) {
//            if (!InsideBoard(pos.x, pos.y)) {
//                Debug.Log("Blocks: Piece Outside [" + pos.x + "," + pos.y + "]");
//                return (false,true);
//            }

//            if (board[pos.y, pos.x] != 1) {
//                Debug.Log("Blocks: Piece not Empty [" + pos.x + "," + pos.y + "]");
//                return (false, false);
//            }
//            Debug.Log("Blocks: Valid placement board[" + pos.x + "," + pos.y + "] = " + board[pos.y, pos.x]);
//        }

//        return (true, false);
//    }

//    private void PlacePieceOnValidSpot(MovablePiece activePiece, Vector2Int[] indexPositions, Vector2 placePos)
//    {
//        // Occupy board
//        OccupySpots(activePiece, indexPositions);

//        // Save the board positions to the piece
//        activePiece.SetOccupySpots(indexPositions);

//        // Also move the original piece here and show it
//        activePiece.gameObject.SetActive(true);
//        activePiece.transform.localPosition = placePos;
//    }

//    private void OccupySpots(MovablePiece activePiece, Vector2Int[] indexPositions)
//    {
//        if (indexPositions == null) return;

//        foreach (var pos in indexPositions) {
//            board[pos.y, pos.x] = (int)activePiece.Type;
//            boardBlocks[pos.y, pos.x].SetType(board[pos.y, pos.x]);
//            Debug.Log("Blocks:  Piece [" + pos.x + "," + pos.y + "] => " + board[pos.y, pos.x]);
//        }
//    }

//    internal void ClearBoardSpots(MovablePiece activePiece)
//    {
//        Vector2Int[] occypySpots = activePiece.OccupySpots;

//        if(occypySpots == null || occypySpots.Length == 0) return;

//        Debug.Log("Clearing all occypySpots "+occypySpots?.Length);

//        if (occypySpots != null) {
//            foreach (Vector2Int pos in occypySpots) {
//                board[pos.y, pos.x] = (int)TetrisBlockType.Fixed;
//                boardBlocks[pos.y, pos.x].SetType(board[pos.y, pos.x]);

//                Debug.Log("Blocks: Unsetting [" + pos.x + "," + pos.y + "] = " + board[pos.y, pos.x]);
//            }
//        }
//    }
//}
