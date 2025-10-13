using System;
using System.Collections;

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tetris : MiniGameBase
{

    [SerializeField] private GameObject blockHolder;
    [SerializeField] private TetrisBlock blockPrefab;
    [SerializeField] private TetrisNextPieceView tetrisNextPieceView;

    private bool GameActive = false;

    const int BlockSize = 30;

    private int level = 1;
    private int lines = 0;
    private float stepTimeSpeedup = StepTime;

    TetrisBlock[,] blocks = new TetrisBlock[10, 22]; // 2-4 blocks invisible?
    int[,] blocksData = new int[10, 22]; // 2-4 blocks invisible?

    private int[] ActualRotationMapping = { -1, 1, 2, 0 };

    // Rating Textfields
    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;
    [SerializeField] private TextMeshProUGUI problemRating;

    [SerializeField] private TextMeshProUGUI tetrisLevelText;
    [SerializeField] private TextMeshProUGUI tetrisLinesText;
    [SerializeField] private TextMeshProUGUI ratingPlusText;

    // Extra Panels
    [SerializeField] private GameObject helpInfo;
    [SerializeField] private MiniGameChessWinNotice winNotice;


    TetrisPiece activePiece;
    TetrisPiece activePiecePreview;

    private bool showDropPreview = true;
    private bool allowDropMove = true;

    int nextBlockType = -1;

    private const float StepTime = 0.3f;
    private const float StepTimeDrop = 0.015f;

    private float steptimer = StepTime;


    private float holdTimer = 0;
    private const float HoldTimeSensibility = 0.1f;
    private bool heldAtLeastOneFullStep = false;
    private bool holdPerformed = false;

    enum PlayerInputDirection { Left, Right, Up, Down, None, RotateLeft };
    enum PlayerInputRotation { Left, Right, Up, None };

    private PlayerInputDirection movePerformed;
    private PlayerInputRotation rotationPerformed;


    void Start()
    {
        SetBoxes2DArray();

        //ResetGame();
        Stats.StatsUpdated += OnStatsUpdated;

        Inputs.Instance.PlayerControls.Player.Rotate.performed += OnPlayerRotateInput;
    }

    private void SetBoxes2DArray()
    {
        for (int j = 0; j < blocks.GetLength(0); j++) {
            for (int i = 0; i < blocks.GetLength(1); i++) {
                TetrisBlock box = Instantiate(blockPrefab, blockHolder.transform);
                blocks[j, i] = box;
                box.transform.localPosition = new Vector2(j* BlockSize, -i* BlockSize);
            }
        }
    }

    private void OnEnable()
    {
        UpdateRating();
        //Inputs.NumberPressed += OnNumberPressed;
    }



    private void Update()
    {
        // Player Step
        if (!GameActive)
            return;

        // AutoStep
        steptimer -= Time.deltaTime;
        if(steptimer <= 0) {
            steptimer = stepTimeSpeedup;
            TryStep(Vector2Int.up);
        }

        // Perform Inputs
        DoPlayerInput();
    }

    private void OnStatsUpdated()
    {
        UpdateRating();
    }

    // RATING
    private void UpdateLevelRating(int diff)
    {
        Debug.Log("UPDATING Problem rating");
        problemRating.text = "Difficulty Level: " + (diff+1);
    }

    private void UpdateRating()
    {
        Debug.Log("UPDATING PLAYER RATING - "+GameType+" = "+(int)GameType);

        playerRating.text = Stats.MiniGameRating(GameType).ToString();

        Debug.Log("SAVESYSTEM - Rating set to " + Stats.MiniGameRating(GameType));
        //problemRating.text = "Problem: " + Stats.ChessRating;


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


    public void ResetGame()
    {
        
        // Make playr able to interract with board again
        GameActive = true;

        Debug.Log("Reset game");
        // Load a new problem of correct difficulty level
        //(int[,] level, int diff) = SudokuProblemDatas.Instance.GetRandomProblem(Stats.MiniGameRating(GameType));

        ClearBlocks();

        // Reset
        ResetGameStats();

        // Set new first block
        nextBlockType = RandomBlockType();

        PlaceNextBlock();
        //Debug.Log("Loaded level " + level[0,0]+" level "+level.GetLength(0)+","+level.GetLength(1));

        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

        //LoadLevel(level);


        //UpdateLevelRating(diff);
    }

    private void ResetGameStats()
    {
        level = 1;
        lines = 0;
        stepTimeSpeedup = StepTime;
        UpdateLevelAndLines();
    }

    private void UpdateLevelAndLines()
    {
        tetrisLinesText.text = lines.ToString();
        tetrisLevelText.text = level.ToString();
        ratingPlusText.text = RatingGain().ToString();
    }

    private int RatingGain() => 1000 + (level-1)*100;
    private int RandomBlockType() => UnityEngine.Random.Range(0, 7);

    private void PlaceNextBlock()
    {
        // Place O at 4,0
        
        int nextPiece = RandomBlockType();

        TetrisPiece piece = nextBlockType switch
        {
            0 => new IPiece(),
            1 => new JPiece(),
            2 => new LPiece(),
            3 => new OPiece(),
            4 => new SPiece(),
            5 => new TPiece(),
            6 => new ZPiece(),
            _ => new OPiece()
        };
        
        activePiece = piece;
        SetPiecePos(new Vector2Int(4, 2));
        

        if (!CheckValidPositionForActivePiece(activePiece.pos)) {
            Debug.Log("Player Lose");
            Win(false);
        }

        // Show the piece there anyway - makes the lose look good
        PlacePiece();

        // Show Next Piece
        nextBlockType = nextPiece;
        tetrisNextPieceView.ShowPiece(nextBlockType);
    }

    private void SetPiecePos(Vector2Int newPos)
    {
        activePiece.pos = newPos;
    }

    private void TryDropFully()
    {
        Vector2Int down = InputToMove(PlayerInputDirection.Down);

        Vector2Int nextPos = activePiece.pos;
        while (CheckValidPositionForActivePiece(nextPos+down)) {
            nextPos += down;
        }

        // If allowing move after drop, dont fixate it
        if (allowDropMove) {
            MoveCurrentPiece(nextPos);
            steptimer = StepTime;
            return;
        }
        
        PlacePiece(fixate: true);
        PlaceNextBlock();        
 
        Debug.Log("Dropped FULLY");
    }

    public bool TryStep(Vector2Int move,bool playerInitiated = false)
    {
        if (!GameActive)
            return false;

        if(playerInitiated)
            Debug.Log("Try Move ["+move.x+","+move.y+"]");

        // Check if Move is Valid
        // Check all positions the new position would occupy
        Vector2Int newPos = activePiece.pos + move;
        //Vector2Int newPos = activePiece.pos + (x <0 ? Vector2Int.left : Vector2Int.right);

        // If Valid remove old pos
        if (!CheckValidPositionForActivePiece(newPos)) {
            //Debug.Log("Invalid Move");
            if (move.y > 0) {
                //Lock
                PlacePiece(fixate: true);
                PlaceNextBlock();
                return true;
            }
            return false;
        }

        // Move to New Pos
        MoveCurrentPiece(newPos);   
        return false;
    }

    private void TryRotate(int rotations = 0)
    {
        if (!ValidRotationForActivePiece(rotations)) {
            Debug.Log("Invalid Rotation");
            return;
        }
        
        RotateCurrentPiece(rotations);
    }

    private void RotateCurrentPiece(int rotations)
    {
        RemoveCurrentPiecePosBlocks();
        //Debug.Log("Rotations starts at "+activePiece.pos);
        activePiece.Rotate(rotations);
        PlacePiece();
        //Debug.Log("Rotations ends at "+activePiece.pos);
    }


    private bool ValidRotationForActivePiece(int rotations = 0)
    {
        // If trying to do 180° check if it has more than 2 states
        if (rotations == 2 && !activePiece.CanTurn())
            return false;

        foreach (Vector2Int pos in activePiece.NextRotationSpots(rotations)) {
            Vector2Int boardPos = activePiece.pos + pos;
            if (OutsideTetrisBoard(ref boardPos))
                return false;
            if (BlockIsEmpty(boardPos))
                return false;
        }
        return true;
    }

    private bool BlockIsEmpty(Vector2Int boardPos) => blocksData[boardPos.x, boardPos.y] != 0;

    private static bool OutsideTetrisBoard(ref Vector2Int boardPos)
    {
        if (boardPos.x < 0 || boardPos.y < 0 || boardPos.x >= 10 || boardPos.y >= 22)
            return true;
        return false;
    }

    private void MoveCurrentPiece(Vector2Int newPos)
    {
        RemoveCurrentPiecePosBlocks();
        SetPiecePos(newPos);
        PlacePiece();
    }

    private void PlacePiece(bool fixate = false)
    {
        // handle drop-preview also?

        if (showDropPreview && !fixate) { // No need to show preview if fixating
            ShowDropPreviewVisuals();
        }

        // Placing piece at current pos
        PlacePieceVisuals(fixate);

        if (fixate) {
            int removedLines = CheckForTetris();

            AddLines(removedLines);
            UpdateLevelAndLines();
        }
    }

    private void PlacePieceVisuals(bool fixate)
    {
        Vector2Int placePos = activePiece.pos;
        foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
            Vector2Int boardPos = placePos + pos;
            int type = (int)activePiece.Type;

            // Fixates if not to be moved again
            if (fixate) {
                type = 1;
                blocksData[boardPos.x, boardPos.y] = type;
            }
            blocks[boardPos.x, boardPos.y].SetType(type);
        }
    }

    private void ShowDropPreviewVisuals()
    {
        // Figure out the lowest point this piece can drop to
        lowestPos = activePiece.pos;

        // While still valid go down
        while (CheckValidPositionForActivePiece(lowestPos + Vector2Int.up)) {
            lowestPos += Vector2Int.up;
        }
        if (activePiece.pos != lowestPos) {
            // Show the piece at lowestpoint

            foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
                Vector2Int boardPos = lowestPos + pos;
                blocks[boardPos.x, boardPos.y].SetType((int)TetrisBlockType.Ghost);
            }
        }
    }

    private void AddLines(int removedLines)
    {
        lines += removedLines;
        level = 1 + lines / 8;
        stepTimeSpeedup = StepTime - Math.Min(level-1,15)* StepTimeDrop;
    }

    private int CheckForTetris()
    {
        // Just placed a piece - check for tetris removed lines
        // Check from bottom up and move down

        Debug.Log("Tetris: - Clear Lines");
        int removeLines = 0;
        for (int j = blocks.GetLength(1) - 1; j >= 0; j--) {
            int blockAmt = 0;
            bool removed = false;
            for (int i = 0; i < blocks.GetLength(0); i++) {
                if(blocksData[i, j] == 1)
                    blockAmt++;
            }

            if(blockAmt == 10) {
                removeLines++;
                removed = true;
            }

            // Row moved down
            if (!removed) {
                // Drop this line to bottom since it shall stay
                if (removeLines == 0)
                    continue;

                // Copy them down

                // Copy from line j to j - removedlines
                int targetLine = j + removeLines;

                //Debug.Log("Moving from line "+j+" to "+targetLine);

                for (int i = 0; i < blocks.GetLength(0); i++) {
                    int movedPieceIndex = blocksData[i, j];
                    blocks[i, targetLine].SetType(movedPieceIndex);
                    blocks[i, j].SetType(0);
                    blocksData[i, targetLine] = movedPieceIndex;
                    blocksData[i, j] = 0;
                }
            }
        }
        return removeLines;

    }

    Vector2Int lowestPos = Vector2Int.zero;

    private void RemoveCurrentPiecePosBlocks()
    {
        foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
            Vector2Int boardPos = activePiece.pos + pos;
            blocks[boardPos.x, boardPos.y].SetType(0);
        }

        if (!showDropPreview) return;

        // Just shows the ghost if not at bottom 
        if (activePiece.pos != lowestPos) {
            // Show the piece at lowestpoint
            foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
                Vector2Int boardPos = lowestPos + pos;
                blocks[boardPos.x, boardPos.y].SetType(0);
            }
        }
    
    }

    private bool CheckValidPositionForActivePiece(Vector2Int placePos)
    {
        foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
            Vector2Int boardPos = placePos + pos;
            if(boardPos.x < 0 || boardPos.y < 0 || boardPos.x >= 10 || boardPos.y >= 22 )
                return false;
            if (blocksData[boardPos.x, boardPos.y] != 0)
                return false;
        }
        return true;
    }

    private void ClearBlocks()
    {
        Debug.Log("Tetris - Clear");
        for (int j = 0; j < blocks.GetLength(0); j++) {
            for (int i = 0; i < blocks.GetLength(1); i++) {
                blocksData[j,i] = 0;
                blocks[j, i].SetType(blocksData[j,i]);
            }
        }
    }

    private void OnPlayerRotateInput(InputAction.CallbackContext context)
    {
        if (!GameActive) return;
        
        ReadRotationInput(context);

        PerformRotation();

        Debug.Log("Rotating: " + rotationPerformed);
    }

    private void ReadRotationInput(InputAction.CallbackContext context)
    {

        // Read the rotation value
        Vector2 value = context.ReadValue<Vector2>();
        if (value.y < 0)
            rotationPerformed = PlayerInputRotation.Up;
        else if (value.x < 0)
            rotationPerformed = PlayerInputRotation.Left;
        else if (value.x > 0)
            rotationPerformed = PlayerInputRotation.Right;
        else {
            rotationPerformed = PlayerInputRotation.None;
        }
    }

    private void PerformRotation()
    {
        if (rotationPerformed == PlayerInputRotation.None)
            return;

        int rotations = ActualRotationMapping[(int)rotationPerformed];

        // Do the rotation
        TryRotate(rotations);
        
        // Unset the rotation
        rotationPerformed = PlayerInputRotation.None;
    }

    private void DoPlayerInput()
   {
        // Releasing a button
        if (holdPerformed) {
            // Was holding something but release it
            if (!AnyTetrisMoveKeyPressed()) {
                //Debug.Log("Tetris: Releasing "+movePerformed);
                // Releasing rotate, do rotate

                // Releasing down - check if enough time elapsed
                if (movePerformed == PlayerInputDirection.Down && (allowDropMove || !heldAtLeastOneFullStep)) {
                    //Debug.Log("Tetris: Drop");
                   // Drop
                    TryDropFully();
                    heldAtLeastOneFullStep = false;
                    holdTimer = 0;
                    holdPerformed = false;
                    movePerformed = PlayerInputDirection.None;
                    return;
                }

                // Do one step here if not passed the threshold
                if (!heldAtLeastOneFullStep) {
                    //Debug.Log("Tetris: Tapped " + movePerformed + " - perform it");
                    //Do a step here
                    holdTimer = 0;
                    heldAtLeastOneFullStep = false;
                    TryStep(InputToMove(movePerformed));
                }


                // Else stop timers
                heldAtLeastOneFullStep = false;
                holdTimer = 0;
                holdPerformed = false;
                movePerformed = PlayerInputDirection.None;
                return;
            }

            PlayerInputDirection currentDirection = ReadPlayerDirection();

            // New Input - Reset
            if (movePerformed != currentDirection) {
                //Debug.Log("Tetris: Held " + movePerformed+" changed to "+currentDirection);
                heldAtLeastOneFullStep = false;
                holdTimer = 0;
                movePerformed = currentDirection;
                return;
            }


            // Step is held

            holdTimer += Time.deltaTime;

            if (holdTimer >= HoldTimeSensibility && !(allowDropMove && currentDirection == PlayerInputDirection.Down)) {
                Debug.Log("Tetris: Held " + movePerformed+" until limit - perfrom it");
                //Do a step here
                holdTimer = 0;
                heldAtLeastOneFullStep = true;
                TryStep(InputToMove(currentDirection));
            }
            return;
        }

        if(!AnyTetrisMoveKeyPressed())
            return;

        // Starting to Hold something
        PlayerInputDirection newDirection = ReadPlayerDirection();
                
        // New Input - Reset
        heldAtLeastOneFullStep = false;
        holdTimer = 0;
        holdPerformed = true;
        movePerformed = newDirection;
        return;


    }

    private bool AnyTetrisMoveKeyPressed() => Inputs.Instance.PlayerControls.Player.TetrisMove.IsPressed();

    private static Vector2Int InputToMove(PlayerInputDirection dir)
    {
        return dir switch
        {
            PlayerInputDirection.Left => new Vector2Int(-1, 0),
            PlayerInputDirection.Right => new Vector2Int(1, 0),
            PlayerInputDirection.Down => new Vector2Int(0, 1),
            PlayerInputDirection.Up => new Vector2Int(0, -1),
            PlayerInputDirection.RotateLeft => new Vector2Int(0, 0),
            _ => new Vector2Int(-1, 0) // Left
        };
    }
    private PlayerInputDirection ReadPlayerDirection()
    {
        Vector2 move = Inputs.Instance.PlayerControls.Player.TetrisMove.ReadValue<Vector2>();
        if(move.x < 0)
            return PlayerInputDirection.Left;
        if(move.x > 0)
            return PlayerInputDirection.Right;
        if(move.y < 0)
            return PlayerInputDirection.Down;        

        return PlayerInputDirection.None; // Shouldnt ever happen
    }

    private Vector2Int GetLimitedMove(InputAction.CallbackContext context)
    {

        Vector2 movement = context.ReadValue<Vector2>();        
        // Clamp if two directions?
        if (movement.x != 0)
            movement = new Vector2(movement.x, 0);
        return Vector2ToIntInverseY(movement);
    }

    private Vector2Int Vector2ToIntInverseY(Vector2 vector) => new Vector2Int(vector.x < 0 ? -1 : (vector.x > 0 ? 1 : 0), vector.y < 0 ? 1 : (vector.y > 0 ? -1 : 0));

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

        int ratingAchieved = RatingGain();
        int currentRating = Stats.MiniGameRatings[(int)GameType];

        int increase = ratingAchieved-currentRating;

        if(increase <= 0) {
            // Do not change rating if not better than current
            return;
        }

        // Award Rating and reward
        Stats.SetMiniGameValue(GameType, ratingAchieved);
        
        // Popup - also make reusable TODO
        ShowRatingIncreaseText(increase);
    }

    //public static float GetDistanceV2(this Vector2 main, Vector2 other) => (main - other).sqrMagnitude;


    
}
