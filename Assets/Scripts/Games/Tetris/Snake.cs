using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using WolfheatProductions;

public class Snake : MiniGameBase
{

    [SerializeField] private GameObject blockHolder;
    [SerializeField] private SnakeBlock blockPrefab;
    [SerializeField] private TetrisNextPieceView tetrisNextPieceView;

    private bool GameActive = false;

    const int BlockSize = 30;

    private int minLevel = 1;
    private int level = 1;
    private int lines = 0;
    private float stepTimeSpeedup = StepTime;

    private const int GameHeight = 20;
    private const int GameWidth = 30;

    SnakeBlock[,] blocks = new SnakeBlock[GameWidth, GameHeight]; // 2-4 blocks invisible?

    private Queue<Vector2Int> snake = new();
    private Vector2Int snakePos = new();
    private PlayerInputDirection direction = PlayerInputDirection.Left;
    private PlayerInputDirection lastCompletedMoveDirection = PlayerInputDirection.Left;

    private const int SnakeLengthToBeginWith = 4;
    private int snakeLengthToAdd = SnakeLengthToBeginWith; // Starting with 4 blocks extra
    private Vector2Int StartPos = new Vector2Int(25, 10);

    // Rating Textfields
    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;
    [SerializeField] private TextMeshProUGUI problemRating;

    [SerializeField] private TextMeshProUGUI tetrisLevelText;
    [SerializeField] private TextMeshProUGUI tetrisLinesText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI ratingPlusText;

    // Extra Panels
    [SerializeField] private GameObject helpInfo;
    [SerializeField] private MiniGameChessWinNotice winNotice;

    private const float StepTime = 0.3f;
    private const float StepTimeDrop = 0.015f;

    private float steptimer = StepTime;

    private const int SnakeType = 6;
    private const int AppleType = 8;

    void Start()
    {
        SetBoxes2DArray();

        //ResetGame();
        Stats.StatsUpdated += OnStatsUpdated;
    }

    // Define game area
    private void SetBoxes2DArray()
    {
        for (int j = 0; j < blocks.GetLength(0); j++) {
            for (int i = 0; i < blocks.GetLength(1); i++) {
                SnakeBlock box = Instantiate(blockPrefab as SnakeBlock, blockHolder.transform);
                blocks[j, i] = box;
                box.transform.localPosition = new Vector2(j * BlockSize, -i * BlockSize);
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
            TryStep(direction);
        }

        // Perform Inputs
        DoPlayerInput();
    }


    public void ResetGame(int levelIn = 1)
    {
        // Make playr able to interract with board again
        GameActive = true;

        Debug.Log("Reset game");
        // Load a new problem of correct difficulty level
        //(int[,] level, int diff) = SudokuProblemDatas.Instance.GetRandomProblem(Stats.MiniGameRating(GameType));

        ClearBlocks();

        snakeLengthToAdd = SnakeLengthToBeginWith;
        snake.Clear();

        // Reset
        ResetGameStats(levelIn);

        direction = PlayerInputDirection.Left;

        // Place Snake        
        PlaceSnake(StartPos);

        PlaceRandomApple();
        
        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);
    }

    private void PlaceRandomApple()
    {
        Vector2Int[] validSpots = GetRandomFrePos();

        Vector2Int placeAt = validSpots[UnityEngine.Random.Range(0, validSpots.Length)];

        PlaceApple(placeAt);
    }

    // Find an empty space to place the apple
    private Vector2Int[] GetRandomFrePos() => Enumerable.Range(0, blocks.GetLength(0)).SelectMany(x => Enumerable.Range(0, blocks.GetLength(1)).Where(y => blocks[x, y].IsEmpty).Select(y => new Vector2Int(x, y))).ToArray();

    private void ResetGameStats(int levelIn)
    {
        minLevel = levelIn;
        level = 1;
        lines = 0;
        stepTimeSpeedup = StepTime;
        UpdateLevelAndLines();
        UpdateLevelSpeed();
    }

    private void UpdateLevelAndLines()
    {
        tetrisLinesText.text = snake.Count.ToString();
        //tetrisLevelText.text = level.ToString();

        // Test to make it red
        //tetrisLevelText.text = TextColor.ColorStringRed(level.ToString());

        
        speedText.text = Mathf.RoundToInt(100f/stepTimeSpeedup).ToString();
        ratingPlusText.text = RatingGain().ToString();
    }

    private int RatingGain()
    {
        // level 11-30 -> 20 / level
        // level 31-50 -> 40 / level
        // level 51-60 -> 80/ level
        // level 51-60 -> 100/ level
        int sum = 1000;
        if (level <= 10)
            return sum;
        if (level <= 30)
            return sum += (level - 10) * 20; // => 1000 + 20 * 20 = 1400
        sum += 20 * 20;
        if (level <= 50)
            return sum += (level - 30) * 40; // => 1500 + 20 * 40 = 2200
        sum += 20 * 40;
        return Math.Min(2999,sum += (level - 50) * 80); // => 2200 + 80 * 10 = 3000

        //return 1000 + (level - 1) * 100;
    }

    TetrisPiece activePiece;

    int nextBlockType = -1;
    private int RandomBlockType() => UnityEngine.Random.Range(0, 7);


    private bool TryStep(PlayerInputDirection move)
    {
        //Debug.Log("Try Step direction "+move);

        if (!GameActive)
            return false;

        Vector2Int moveDir = MoveToVector(move);

        //Debug.Log("Snake Peek = ["+snake.Peek().x+","+snake.Peek().y+"]");

        // Check if Move is Valid
        Vector2Int newPos = snakePos + moveDir;
        
        // If Valid remove old pos
        if (!CheckIfPlacementPositionIsValid(newPos)) {
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

    // Only changes direction
    private void RotateCurrentPiece(bool isRight = true) => direction = (PlayerInputDirection)(((int)direction + (isRight ? 1 : -1)) % 4);


    private int[] ActualRotationsToPerform = { -1, 1, 2, 0 };

    private static bool OutsideTetrisBoard(ref Vector2Int boardPos)
    {
        if (boardPos.x < 0 || boardPos.y < 0 || boardPos.x >= 10 || boardPos.y >= 22)
            return true;
        return false;
    }

    // Movement
    private void MoveCurrentPiece(Vector2Int newPos)
    {
        // If moving remove the last one and place the new one
        if(snakeLengthToAdd == 0)
            RemoveSnake();
        else
            snakeLengthToAdd = snakeLengthToAdd-1;

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


    private void PlaceSnake(Vector2Int pos)
    {
        // Apple check
        if (blocks[pos.x, pos.y].IsApple)
            EatApple();


        snake.Enqueue(pos);

        // Make this the snake
        blocks[pos.x, pos.y].SetType(SnakeType);
        snakePos = pos;

        UpdateLevelAndLines();
    }

    private void EatApple()
    {
        // Increase level and stats
        Debug.Log("Omnom + stats and level");
        level++;
        snakeLengthToAdd += SnakeLengthToBeginWith;

        UpdateLevelSpeed();

        // Create a new apple
        PlaceRandomApple();
    }

    private void UpdateLevelSpeed()
    {
        int levelForSpeed = Math.Max(minLevel,level);

        stepTimeSpeedup = Math.Max(0.01f, StepTime - levelForSpeed * 0.005f); // 0.28 tot which gives max speed at level 56
    }

    // Make this an apple
    private void PlaceApple(Vector2Int pos) => blocks[pos.x, pos.y].SetType(AppleType);

    private bool CheckIfPlacementPositionIsValid(Vector2Int placePos) => !(placePos.x < 0 || placePos.y < 0 || placePos.x >= GameWidth || placePos.y >= GameHeight || blocks[placePos.x, placePos.y].IsSnake);

    private void ClearBlocks()
    {
        Debug.Log("Snake - Clear");
        for (int j = 0; j < blocks.GetLength(0); j++) {
            for (int i = 0; i < blocks.GetLength(1); i++) {
                blocks[j, i].SetType(0);
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


    // Inputs
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

    // Data Handling
    private Vector2Int Vector2ToIntInverseY(Vector2 vector) => new Vector2Int(vector.x < 0 ? -1 : (vector.x > 0 ? 1 : 0), vector.y < 0 ? 1 : (vector.y > 0 ? -1 : 0));


    public void ReduceRating()
    {

        Stats.SetMiniGameValue(GameType, Math.Max(Stats.MiniGameRatings[(int)GameType]-100,1000));
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


    // RATING
    private void OnStatsUpdated() => UpdateRating();

    private void UpdateRating() => playerRating.text = Stats.MiniGameRating(GameType).ToString();

    private void ShowRatingIncreaseText(int increase) => StartCoroutine(ShowRatingIncreaseCO(increase));

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




}
