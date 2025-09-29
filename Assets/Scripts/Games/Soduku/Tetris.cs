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

    void Start()
    {
        SetBoxes2DArray();

        ResetGame();
        Stats.StatsUpdated += OnStatsUpdated;

        Inputs.Instance.PlayerControls.Player.Move.performed += OnPlayerMoveInput;

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
        steptimer -= Time.deltaTime;
        if(steptimer<= 0) {
            steptimer = StepTime;
            TryStep(Vector2Int.up);
        }   
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


    public void RestartLevel()
    {
        if (!GameActive) return; // Dont allow restart on won game
        /*
        for (int j = 0; j < boxes.GetLength(0); j++) {
            for (int i = 0; i < boxes.GetLength(1); i++) {
                if (boxes[j, i].Fixed)
                    continue;
                boxes[j, i].SetAsFixed(0, false);
            }
        }*/
    }

    public void ResetGame()
    {
        // Make playr able to interract with board again
        GameActive = true;

        Debug.Log("Reset game");
        // Load a new problem of correct difficulty level
        //(int[,] level, int diff) = SudokuProblemDatas.Instance.GetRandomProblem(Stats.MiniGameRating(GameType));

        ClearBlocks();


        PlaceTestBlock();
        //Debug.Log("Loaded level " + level[0,0]+" level "+level.GetLength(0)+","+level.GetLength(1));

        // Remove Win Screen Notice
        //winNotice.gameObject.SetActive(false);

        //LoadLevel(level);

        //UpdateLevelRating(diff);
    }

    TetrisPiece activePiece;

    private void PlaceTestBlock()
    {
        // Place O at 4,0
        TetrisPiece piece = new OPiece();

        activePiece = piece;
        activePiece.pos = new Vector2Int(4, 2);

        PlacePiece();
    }

    public void TryStep(Vector2Int move)
    {
        if (!GameActive)
            return;

        Debug.Log("Try Move");

        // Check if Move is Valid
        // Check all positions the new position would occupy
        Vector2Int newPos = activePiece.pos + move;
        //Vector2Int newPos = activePiece.pos + (x <0 ? Vector2Int.left : Vector2Int.right);

        // If Valid remove old pos
        if (!CheckValidPositionForActivePiece(newPos)) {
            Debug.Log("Invalid Move");
            if (move.y > 0) {
                //Lock
                PlacePiece(fixate: true);
                PlaceTestBlock();
            }
            return;
        }

        // Move to New Pos
        MoveCurrentPiece(newPos);
    }

    private void TryRotate()
    {
        Debug.Log("Rotate");


        // If Valid remove old pos
        if (!CheckValidRotationForActivePiece()) {
            Debug.Log("Invalid Rotation");
            return;
        }
        
        RotateCurrentPiece();
    }

    private void RotateCurrentPiece()
    {
        RemoveCurrentPiecePosBlocks();
        activePiece.Rotate();
        PlacePiece();
    }

    private bool CheckValidRotationForActivePiece()
    {
        foreach (Vector2Int pos in activePiece.NextRotationSpots) {
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

    private void OnPlayerMoveInput(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        Debug.Log("Player Move "+movement.x);


        if (movement.y > 0)
            TryRotate();
        else 
            TryStep(Vector2ToIntInverseY(movement));
    }

    private Vector2Int Vector2ToIntInverseY(Vector2 vector) => new Vector2Int(vector.x < 0 ? -1 : (vector.x > 0 ? 1 : 0), vector.y < 0 ? 1 : (vector.y > 0 ? -1 : 0));

    private void TryDropStep()
    {
        Debug.Log("Drop");
    }


    private void Win(bool didWin)
    {
        //Debug.Log(didWin ? "YOU WIN" : "LOST");
        
        Debug.Log("YOU WIN");
        GameActive = false;
        
        // Also make this general?
        //winNotice.gameObject.SetActive(true);        
        //winNotice.SetWin(didWin);

        // Added Rating
        int increase = didWin ? Stats.SudokuGameWinRatingChange : Stats.SudokuGameLossRatingChange;

        // Award Rating and reward
        Stats.ChangeMiniGameRating(GameType, increase);
        
        // Popup - also make reusable TODO
        ShowRatingIncreaseText(increase);
    }

    //public static float GetDistanceV2(this Vector2 main, Vector2 other) => (main - other).sqrMagnitude;


    
}
