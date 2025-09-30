using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tetris : MiniGameBase
{

    [SerializeField] private GameObject blockHolder;
    [SerializeField] private TetrisBlock blockPrefab;

    private bool GameActive = false;

    const int BlockSize = 30;

    TetrisBlock[,] blocks = new TetrisBlock[10, 22]; // 2-4 blocks invisible?
    int[,] blocksData = new int[10, 22]; // 2-4 blocks invisible?

    // Rating Textfields
    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;
    [SerializeField] private TextMeshProUGUI problemRating;


    // Extra Panels
    [SerializeField] private GameObject helpInfo;
    [SerializeField] private MiniGameChessWinNotice winNotice;

    void Start()
    {
        SetBoxes2DArray();

        ResetGame();
        Stats.StatsUpdated += OnStatsUpdated;

        //Inputs.Instance.PlayerControls.Player.Move.performed += OnPlayerMoveInput;

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


    private const float StepTime = 0.3f;
    private float steptimer = StepTime;

    private void Update()
    {
        // AutoStep
        steptimer -= Time.deltaTime;
        if(steptimer <= 0) {
            steptimer = StepTime;
            TryStep(Vector2Int.up);
        }

        // Player Step
        if (GameActive)
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
        Debug.Log("UPDATING PLAYER RATING");
        playerRating.text = "Rating: " + Stats.MiniGameRating(GameType);

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


        PlaceNextBlock();
        //Debug.Log("Loaded level " + level[0,0]+" level "+level.GetLength(0)+","+level.GetLength(1));

        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

        //LoadLevel(level);

        //UpdateLevelRating(diff);
    }

    TetrisPiece activePiece;

    private void PlaceNextBlock()
    {
        // Place O at 4,0
        int type = UnityEngine.Random.Range(0, 7);

        TetrisPiece piece = type switch
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
        activePiece.pos = new Vector2Int(4, 2);

        if (!CheckValidPositionForActivePiece(activePiece.pos)) {
            Debug.Log("Player Lose");
            Win(false);
        }

        // Show the piece there anyway - makes the lose look good
        PlacePiece();
        
    }

    private void TryDropFully()
    {
        Vector2Int down = InputToMove(PlayerInputDirection.Down);
        while (!TryStep(down)) {
            //Debug.Log("Dropped one step");
        } 
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

    private void TryRotate(bool right = true)
    {
        //Debug.Log("Rotate");
        // Resave Current position

        // If Valid remove old pos
        if (!CheckValidRotationForActivePiece(right)) {
            Debug.Log("Invalid Rotation");
            return;
        }
        
        RotateCurrentPiece(right);
    }

    private void RotateCurrentPiece(bool right = true)
    {
        RemoveCurrentPiecePosBlocks();
        //Debug.Log("Rotations starts at "+activePiece.pos);
        activePiece.Rotate(right);
        PlacePiece();
        //Debug.Log("Rotations ends at "+activePiece.pos);
    }

    private bool CheckValidRotationForActivePiece(bool right = true)
    {
        foreach (Vector2Int pos in activePiece.NextRotationSpots(right)) {
            Vector2Int boardPos = activePiece.pos + pos;
            if (boardPos.x < 0 || boardPos.y < 0 || boardPos.x >= 10 || boardPos.y >= 22)
                return false;
            if (blocksData[boardPos.x, boardPos.y] != 0)
                return false;
        }
        return true;
    }
    private void MoveCurrentPiece(Vector2Int newPos)
    {
        RemoveCurrentPiecePosBlocks();
        activePiece.pos = newPos;
        PlacePiece();
    }

    private void PlacePiece(bool fixate = false)
    {
        // Placing piece at current pos
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
        if(fixate)
            CheckForTetris();
    }

    private void CheckForTetris()
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

    }

    private void RemoveCurrentPiecePosBlocks()
    {
        foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
            Vector2Int boardPos = activePiece.pos + pos;
            blocks[boardPos.x, boardPos.y].SetType(0);
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

    private float holdTimer = 0;
    private const float HoldTimeSensibility = 0.1f;
    private bool heldAtLeastOneFullStep = false;
    private bool holdPerformed = false;


    enum PlayerInputDirection{Left,Right,Up,Down, None, RotateLeft };
    private PlayerInputDirection movePerformed;

    private void DoPlayerInput()
    {

        // Releasing a button
        if(holdPerformed) {
            // Was holding something but release it
            if (!AnyTetrisMoveKeyPressed()) {
                //Debug.Log("Tetris: Releasing "+movePerformed);
                // Releasing rotate, do rotate
                if (movePerformed == PlayerInputDirection.Up || movePerformed == PlayerInputDirection.RotateLeft) {
                    TryRotate(movePerformed == PlayerInputDirection.Up);
                    holdTimer = 0;
                    heldAtLeastOneFullStep = false;
                    holdPerformed = false;
                    return;
                }

                // Releasing down - check if enough time elapsed
                if (movePerformed == PlayerInputDirection.Down && !heldAtLeastOneFullStep) {
                    //Debug.Log("Tetris: Drop");
                   // Drop
                    TryDropFully();
                    heldAtLeastOneFullStep = false;
                    holdTimer = 0;
                    holdPerformed = false;
                    movePerformed = PlayerInputDirection.None;
                    return;
                }

                // Do one step here if not passed the threashold
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

            if (movePerformed == PlayerInputDirection.Up) {
                //Debug.Log("Tetris: Holding rotation - do nothing until release");
                return; // Dont do anything until player releases this button
            }

            // Step is held

            holdTimer += Time.deltaTime;

            if (holdTimer >= HoldTimeSensibility) {
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

    private bool AnyTetrisMoveKeyPressed() => Inputs.Instance.PlayerControls.Player.Move.IsPressed() || Inputs.Instance.PlayerControls.Player.RotateLeft.IsPressed();

    private bool ReadRotate() => Inputs.Instance.PlayerControls.Player.RotateLeft.WasPerformedThisFrame();

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
        Vector2 move = Inputs.Instance.PlayerControls.Player.Move.ReadValue<Vector2>();

        if(move.x < 0)
            return PlayerInputDirection.Left;
        if(move.x > 0)
            return PlayerInputDirection.Right;
        if(move.y > 0)
            return PlayerInputDirection.Up;
        if(move.y < 0)
            return PlayerInputDirection.Down;
        if(Inputs.Instance.PlayerControls.Player.RotateLeft.IsPressed())
            return PlayerInputDirection.RotateLeft;
        return PlayerInputDirection.None;
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

    private void TryDropStep()
    {
        Debug.Log("Drop");
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
        int increase = 100;

        // Award Rating and reward
        Stats.ChangeMiniGameRating(GameType, increase);
        
        // Popup - also make reusable TODO
        ShowRatingIncreaseText(increase);
    }

    //public static float GetDistanceV2(this Vector2 main, Vector2 other) => (main - other).sqrMagnitude;


    
}
