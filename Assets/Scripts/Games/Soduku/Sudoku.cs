using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using WolfheatProductions;
using static UnityEditor.PlayerSettings;
public class Sudoku : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler
{

    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private SodukoBox[] allBoxes;
    [SerializeField] private SodukoBox boxPrefab;
    [SerializeField] private GameObject boxHolder;

    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;

    [SerializeField] private TextMeshProUGUI problemRating;
    [SerializeField] private TextMeshProUGUI playerRating;

    [SerializeField] private ChessWinNotice winNotice;

    private const int BoxSize = 65;

    private SodukoBox[,] boxes = new SodukoBox[9, 9];

    SodukoBox hoverBox;

    private bool GameActive = false;


    void Start()
    {
        SetBoxes2DArray();

        ResetGame();
        Stats.StatsUpdated += OnStatsUpdated;
    }


    private void OnEnable()
    {
        UpdateRating();
        Inputs.NumberPressed += OnNumberPressed;
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
        playerRating.text = "Rating: " + Stats.SudokuRating;

        Debug.Log("SAVESYSTEM - Rating set to " + Stats.SudokuRating);
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


    private void SetBoxes2DArray()
    {
        for (int j = 0; j < boxes.GetLength(0); j++) {
            for (int i = 0; i < boxes.GetLength(1); i++) {
                SodukoBox box = Instantiate(boxPrefab, boxHolder.transform);
                boxes[j, i] = box;
            }
        }
    }

    public void RestartLevel()
    {
        if (!GameActive) return; // Dont allow restart on won game

        for (int j = 0; j < boxes.GetLength(0); j++) {
            for (int i = 0; i < boxes.GetLength(1); i++) {
                if (boxes[j, i].Fixed)
                    continue;
                boxes[j, i].SetAsFixed(0, false);
            }
        }
    }

    public void DecreaseRating()
    {
        Stats.DecreaseSudokuRating(100);
    }

    public void ResetGame()
    {
        // Make playr able to interract with board again
        GameActive = true;

        Debug.Log("Reset game");
        // Load a new problem of correct difficulty level
        (int[,] level, int diff) = SudokuProblemDatas.Instance.GetRandomProblem(Stats.SudokuRating);

        //Debug.Log("Loaded level " + level[0,0]+" level "+level.GetLength(0)+","+level.GetLength(1));

        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

        LoadLevel(level);

        UpdateLevelRating(diff);
    }

    private void LoadLevel(int[,] level)
    {
        Debug.Log("Loading level");
        for (int j = 0; j < boxes.GetLength(0); j++) {
            for (int i = 0; i < boxes.GetLength(1); i++) {
                if (level[j, i] != 0) {
                    boxes[j, i].SetAsFixed(level[j, i]);
                    Debug.Log("Fixed " + level[j, i]);
                    continue;
                }
                Debug.Log("Free");
                // Setting normal changable box
                boxes[j, i].SetAsFixed(0, false);
            }
        }

    }

    public void LoadEasyLevel()
    {
        // Make playr able to interract with board again
        GameActive = true;

        // Load a new problem of correct difficulty level
        (int[,] level, int diff) = SudokuProblemDatas.Instance.GetEasyLevel();


        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

        LoadLevel(level);

        UpdateLevelRating(diff);
    }
    private void OnDisable()
    {
        Inputs.NumberPressed -= OnNumberPressed;
    }

    private void OnNumberPressed(int number)
    {
        if (!GameActive)
            return;
        Debug.Log("Pressing number in Soduko " + number);
        if (hoverBox == null)
            return;

        if(hoverBox.RequestChangeType(number))
            CheckForWin();
    }

    private void CheckForWin()
    {
        if (EmptySpotsRemain())
            return;
        Debug.Log("Checking for win - number in all boxes");
        if (ValidSolution()) {
            Debug.Log("Valid Solution!");
            Win(true);
            
        }
    }


    private bool ValidSolution()
    {
        int[] colSums = new int[9];

        for (int j = 0; j < boxes.GetLength(0); j++) {
            int rowSum = 0;
            for (int i = 0; i < boxes.GetLength(1); i++) {
                rowSum += boxes[j, i].Number;
                colSums[i] += boxes[j, i].Number;
                if (boxes[j, i].Number == 0)
                    return true;
            }
            // Each Row
            if(rowSum != 45)
                return false;
        }

        // All colums
        foreach (int i in colSums)
            if (i != 45)
                return false
                    ;
        return true;
    }

    private void Win(bool didWin)
    {
        //Debug.Log(didWin ? "YOU WIN" : "LOST");
        
        Debug.Log("YOU WIN");
        GameActive = false;
        
        winNotice.gameObject.SetActive(true);
        
        winNotice.SetWin(didWin);

        // Award Rating and reward
        int increase = Stats.ChangeSudokuRating(didWin);

        UpdateRating();

        ShowRatingIncreaseText(increase);

        // Send save needed event
        Debug.Log("SAVESYSTEM - Trigger Save");
        SavingUtility.playerGameData.TriggerSave();
    }

    private bool EmptySpotsRemain()
    {
        for (int j = 0; j < boxes.GetLength(0); j++) {
            for (int i = 0; i < boxes.GetLength(1); i++) {
                if (boxes[j, i].Number == 0)
                    return true;
            }
        }
        return false;
    
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!GameActive)
            return;

        // Clicking should loop through all numbers?
        Vector2Int pos = Converter.GetMouseLocalPositionIndex(eventData, rectTransform, BoxSize);
        Debug.Log("Mouse Down on soduko ["+pos.y+","+pos.x+"]");
        // Find this box and update to next number?

        SodukoBox activeBox = boxes[pos.y, pos.x];

        bool rightClick = eventData.button == PointerEventData.InputButton.Right;

        //Check if this is fixed
        if (!activeBox.RequestNextNumber(rightClick)) {
            Debug.Log("This box is fixed, cant change number");
            return;
        }

        // The box was changed
        Debug.Log("Box changed to "+activeBox.Number);

        CheckForWin();

    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!GameActive)
            return;
        // When moving over and pressing any number that though appear?
        // Update active soduko box

        Vector2Int pos = Converter.GetMouseLocalPositionIndex(eventData, rectTransform, BoxSize);
        if (!ValidPos(pos))
            return;

        hoverBox = boxes[pos.y, pos.x];
    }

    private bool ValidPos(Vector2Int pos) => pos.x >= 0 && pos.y >= 0 && pos.x < 9 && pos.y < 9;
}
