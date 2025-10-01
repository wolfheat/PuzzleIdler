using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Snake : MiniGameBase
{

    [SerializeField] private GameObject blockHolder;
    [SerializeField] private TetrisBlock blockPrefab;
    [SerializeField] private TetrisNextPieceView tetrisNextPieceView;

    private bool GameActive = false;

    const int BlockSize = 30;

    private int level = 1;
    private int lines = 0;
    private float stepTimeSpeedup = StepTime;

    private const int GameHeight = 20;
    private const int GameWidth = 30;

    TetrisBlock[,] blocks = new TetrisBlock[GameWidth, GameHeight]; // 2-4 blocks invisible?
    int[,] blocksData = new int[30, 20]; // 2-4 blocks invisible?

    private Queue<Vector2Int> snake = new();
    private Vector2Int snakePos = new();
    private PlayerInputDirection direction = PlayerInputDirection.Left;
    private PlayerInputDirection lastCompletedMoveDirection = PlayerInputDirection.Left;
    private int blocksToAdd = 4; // Starting with 4 blocks extra

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

    void Start()
    {
        SetBoxes2DArray();

        //ResetGame();
        Stats.StatsUpdated += OnStatsUpdated;
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
    private const float StepTimeDrop = 0.015f;

    private float steptimer = StepTime;

    private void Update()
    {
        // Player Step
        if (!GameActive)
            return;

        // AutoStep
        steptimer -= Time.deltaTime;
        if(steptimer <= 0) {
            steptimer = stepTimeSpeedup;
            TryStep(direction);
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

        direction = PlayerInputDirection.Left;

        // Place Snake
        PlaceSnakeAt(new Vector2Int(25,10));
        
        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);
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
        tetrisLinesText.text = snake.Count.ToString();
        tetrisLevelText.text = level.ToString();
        ratingPlusText.text = RatingGain().ToString();
    }

    private int RatingGain() => 1000 + (level-1)*100;

    TetrisPiece activePiece;

    int nextBlockType = -1;
    private int RandomBlockType() => UnityEngine.Random.Range(0, 7);

    private void PlaceSnakeAt(Vector2Int placePos)
    {
        // Place O at 4,0

        Debug.Log("Placing Snake at "+placePos);


        // All snakes are type (Blue) = L
        PlaceSnake(placePos);
    }

    private bool TryStep(PlayerInputDirection move)
    {
        Debug.Log("Try Step direction "+move);

        if (!GameActive)
            return false;

        Vector2Int moveDir = MoveToVector(move);

        //Debug.Log("Snake Peek = ["+snake.Peek().x+","+snake.Peek().y+"]");

        // Check if Move is Valid
        Vector2Int newPos = snakePos + moveDir;
        
        // If Valid remove old pos
        if (!CheckValidPositionForActivePiece(newPos)) {
            //Debug.Log("Invalid Move");
            //Lock
            Debug.Log("Game Ended At "+move);
            Win(false);
            return false;
        }

        // Move to New Pos
        MoveCurrentPiece(newPos);
        return false;
    }

    private Vector2Int MoveToVector(PlayerInputDirection move)
    {
        return (int)move switch
        {
            0 => new Vector2Int(-1, 0),
            1 => new Vector2Int(1, 0),
            2 => new Vector2Int(0, -1),
            3 => new Vector2Int(0, 1),
            _ => new Vector2Int(0, 0)
        };
    }

    private void TryRotate(int rotations = 0)
    {
        if (!ValidRotationForActivePiece(rotations)) {
            Debug.Log("Invalid Rotation");
            return;
        }
        
        RotateCurrentPiece();
    }

    // Only changes direction
    private void RotateCurrentPiece(bool isRight = true) => direction = (PlayerInputDirection)(((int)direction + (isRight ? 1 : -1)) % 4);


    private int[] ActualRotationsToPerform = { -1, 1, 2, 0 };

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
        // If moving remove the last one and place the new one
        if(blocksToAdd == 0)
            RemoveSnake();
        else
            blocksToAdd = blocksToAdd-1;

        PlaceSnake(newPos);

        // Save this as last directionperformed 
        lastCompletedMoveDirection = direction;
    }

    private void RemoveSnake()
    {
        Vector2Int pos = snake.Dequeue();
        // Make this the snake
        blocks[pos.x, pos.y].SetType(0);
    }

    private const int SnakeType = 2;

    private void PlaceSnake(Vector2Int pos)
    {
        snake.Enqueue(pos);

        // Make this the snake
        blocks[pos.x, pos.y].SetType(SnakeType);
        snakePos = pos;

        UpdateLevelAndLines();
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

    private void RemoveCurrentPiecePosBlocks()
    {
        foreach (Vector2Int pos in activePiece.CurrentRotationSpots) {
            Vector2Int boardPos = activePiece.pos + pos;
            blocks[boardPos.x, boardPos.y].SetType(0);
        }
    }

    private bool CheckValidPositionForActivePiece(Vector2Int placePos)
    {
        // Check outisde board
        if(placePos.x < 0 || placePos.y < 0 || placePos.x >= GameWidth || placePos.y >= GameHeight)
            return false;

        // Check occupied
        if (blocksData[placePos.x, placePos.y] != 0)
            return false;
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
    enum PlayerInputRotation{Left,Right,Up,None};

    private PlayerInputDirection movePerformed;
    private PlayerInputRotation rotationPerformed;

    private void PerformRotation()
    {
        if (rotationPerformed == PlayerInputRotation.None)
            return;

        int rotations = ActualRotationsToPerform[(int)rotationPerformed];

        // Do the rotation
        TryRotate(rotations);
        
        // Unset the rotation
        rotationPerformed = PlayerInputRotation.None;
    }

    private void DoPlayerInput()
    {
        PlayerInputDirection newDirection = ReadPlayerDirection();
        if (newDirection == PlayerInputDirection.None) 
            return;
        
        // Trying to continue ahead or 180 turn
        if ((int)lastCompletedMoveDirection < 2 && (int)newDirection < 2 || (int)lastCompletedMoveDirection >= 2 && (int)newDirection >= 2) 
            return;

        // Valid change do it
        direction = newDirection;
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
        Vector2 move = Inputs.Instance.PlayerControls.Player.Move.ReadValue<Vector2>();
        if(move.x < 0)
            return PlayerInputDirection.Left;
        if(move.x > 0)
            return PlayerInputDirection.Right;
        if(move.y < 0)
            return PlayerInputDirection.Down;        
        if(move.y > 0)
            return PlayerInputDirection.Up; // Shouldnt ever happen
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
