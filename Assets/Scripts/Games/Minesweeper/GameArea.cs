using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using WolfheatProductions;

public class GameArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] private bool isOnlyView = false;
    [SerializeField] private GameBox unclearedBoxPrefab;
    [SerializeField] private UnderGameBox underlayBoxPrefab;
    [SerializeField] private GameObject boxHolder;
    [SerializeField] private GameObject underLaying;
    [SerializeField] private RectTransform rectTransform;


    Vector2 localPosition;

    //Vector3 align = new Vector3(0.5f, -0.5f, 0);
    Vector3 align = new Vector3(0f, -0f, 0);

    Vector2 borderAddon = new Vector3(0.3f, 0.81f);

    Vector3 boxScale = new Vector3(1f, 1f, 1f);
    
    const int BoxSize = 40;

    private int gameWidth = 6;
    private int gameHeight = 6;
    private int mineCountAmount = 0;
    private int totalmines = 0;
    private int opened;

    public bool GamePaused { get; private set; }

    int[,] mines;
    UnderGameBox[,] underlayBoxes = new UnderGameBox[0, 0];
    GameBox[,] overlayBoxes = new GameBox[0, 0];

    private int totalToOpen;
    private Vector2Int swapBox;
    public bool LevelBusted { get; private set; }
    public int B3V { get; private set; }
    public int Clicks { get; private set; }
    public int TotalClicks { get; private set; }

    public static GameArea Instance { get; private set; }
    public float GameWidth { get; set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rectTransform = GetComponent<RectTransform>();
    }


    private void Start()
    {
        RestartGame();
    }

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI amtText;
    [SerializeField] private GameObject objects;
    [SerializeField] private GameObject origo;

    [SerializeField] DigiDisplay mineDisplay;
    [SerializeField] DigiDisplay timeDisplay;

    public void RestartGame(bool resetPosition = false)
    {
        Debug.Log("Restarting game!");


        SizeGameArea();


        RandomizeMines();

        DrawLevel();

        opened = 0;

        GamePaused = false;
        USerInfo.WaitForFirstMove = true;
        Timer.Instance.ResetCounterAndPause();

        return;

        //ResetLevel(resetPosition);
        AlignBoxesAnchor(resetPosition);
        SmileyButton.Instance.ShowNormal();
        USerInfo.levelID = gameWidth + "x" + gameHeight;

        // Exchange this
        levelText.text = USerInfo.levelID;
        //amtText.text = "" + FirestoreManager.Instance.LoadedAmount;

        Clicks = 0;
        TotalClicks = 0;
        USerInfo.currentType = GameType.Normal;

    }

    public void PrintAllMines(string prefix, int[,] array)
    {
        StringBuilder sb = new(prefix+"\n[");
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
                sb.Append((i == 0 ? "[" : "") + array[i, j] + (i != gameWidth - 1 ? "," : "]\n"));
        } 
        sb.Append(']');
        Debug.Log(sb.ToString());
    }
    
    public void PrintFirstRowMines(string prefix, int[,] array)
    {
        StringBuilder sb = new(prefix+" [");
        for (int i = 0; i < gameWidth; i++)
            sb.Append(array[i, 0] + i != gameWidth - 1 ? "," : "");
        sb.Append(']');
        Debug.Log(sb.ToString());
    }

    public void ResetBoard()
    {
        SizeGameArea(false);
        //mines = gameLoaded;
        ResetBoxes(); // Restes the Boxes
        DetermineNumbersFromNeighbors(); // Sets all numbers
        DrawLevel();
        if (isOnlyView)
            return;
        Timer.Instance.ResetCounterAndPause();

        //SmileyButton.Instance.SetSmileyTypeFromLevelAmountLoaded(FirestoreManager.Instance.ActiveChallengeLevels.Count);
    }

    public void AlignBoxesAnchor(bool resetCamera = true)
    {
        // Align GameArea
        objects.transform.position = origo.transform.position;
        objects.transform.localScale = Vector3.one * 0.5f;
        
        //if(resetCamera)
        //    CameraController.Instance.ResetCamera();
        //objects.transform.localScale = Vector3.one * 0.5f;
    }

    
    public void ResetAllNonMine()
    {
        Debug.Log("Resetting under and over boxes in arrays");

        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                if (mines[i,j]!=-1)
                    overlayBoxes[i, j].Reset();
            }
        }
        TotalMines();
        UpdateMineCount();
    }

    public void TotalMines()
    {
        if (isOnlyView)
            return;
        totalToOpen = gameWidth * gameHeight - totalmines;
        mineCountAmount = totalmines;
        opened = 0;
    }
    public void DecreaseMineCount()
    {
        mineCountAmount--;
        UpdateMineCount();
    }

    public void IncreaseMineCount()
    {
        mineCountAmount++;
        UpdateMineCount();
    }

    public void DrawLevel()
    {
        //Debug.Log("Creating new under and over boxes in arrays ["+gameWidth+","+gameHeight+"] ");
        //Debug.Log("OverlayBoxes size = ["+overlayBoxes.GetLength(0)+","+overlayBoxes.GetLength(1)+"] ");
        //Debug.Log("Underlayboxes size = ["+underlayBoxes.GetLength(0)+","+underlayBoxes.GetLength(1)+"] ");
        //BottomInfoController.Instance.ShowDebugText("DrawLevel "+overlayBoxes.GetLength(0)+"x"+overlayBoxes.GetLength(1));

        // New system for painting level


        ClearAllBoxes();

        // Negative Y positive X

        Debug.Log("MS:  Drawing Level");
        //Debug.Log("MS: Level size: "+gameHeight+","+gameWidth);

        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                GameBox box = Instantiate(unclearedBoxPrefab, boxHolder.transform);
                box.transform.localPosition = new Vector3(i*BoxSize, -j * BoxSize, 0) + align;
                //Debug.Log("OverBox placed at local "+ box.transform.localPosition);
                box.transform.localScale = boxScale;
                box.Pos = new Vector2Int(i, j);
                overlayBoxes[i, j] = box;

                // Make underlaying
                UnderGameBox underlayBox = Instantiate(underlayBoxPrefab, underLaying.transform);
                //underlayBox.SetOrderingLeyer(0);
                underlayBox.transform.localPosition = new Vector3(i * BoxSize, -j * BoxSize, 0) + align;
                underlayBox.transform.localScale = boxScale;
                underlayBox.Pos = new Vector2Int(i, j);
                underlayBoxes[i, j] = underlayBox;
                underlayBox.SetType(mines[i, j]);
                
            }
        }
        //Timer.Instance.Pause();
    }

    private void ClearAllBoxes()
    {
        foreach(Transform box in boxHolder.transform) {
            Destroy(box.gameObject);
        }
        
        foreach(Transform box in underLaying.transform) {
            Destroy(box.gameObject);
        }
    }

    private void RandomizeMines()
    {
        mineCountAmount = 0;

        // Place total Mines at random positions A (Get all positions and take one at random)
        Debug.Log("***** Randomize Mines "+gameWidth+"*"+gameHeight);
        int[] allPos = Enumerable.Range(0, gameHeight * gameWidth).ToArray();
        
        // Fisher-Yates scramble
        allPos = Converter.FisherYatesScramble(allPos);

        // Set last position as a swapbox cause this is free unless all positons are mines
        int row = allPos[allPos.Length - 1] % gameWidth;
        int col = allPos[allPos.Length - 1] / gameWidth;
        swapBox = new Vector2Int(row, col);
        Debug.Log("Game area size = "+mines.GetLength(0)+" "+mines.GetLength(1));
        Debug.Log("Swap box at = " + ((allPos.Length - 1) % gameWidth) + ","+ ((allPos.Length - 1) / gameWidth));

        //Debug.Log("RandomizeMine is picked from row = "+row+" and col = "+col+" allpos size = "+ allPos.Length+" Total Mines = "+totalmines);
        // allPos stores all positions 0-Size-1 of the game area
        // Takes out a number representating a spot in the grid
        for (int i = 0; i < totalmines; i++)
        {
            row = allPos[i] % gameWidth;
            col = allPos[i] / gameWidth;
            mines[row, col] = -1;
        }
        mineCountAmount = totalmines;
        totalToOpen = gameWidth * gameHeight - totalmines;
        DetermineNumbersFromNeighbors();
        // Set minecount
        UpdateMineCount();
    }

    public void AddClicks()
    {
        Clicks++;
    }
    public void AddTotalClicks()
    {
        TotalClicks++;
    }
    public bool Chord(Vector2Int pos)
    {
        bool wasted = true;
        Debug.Log("Chording levelcreator at "+pos);
        if (Chordable(pos))
        {
            Debug.Log("Chardable");
            wasted = OpenAllNeighbors(pos);
        }
        return wasted;
    }

    private bool Chordable(Vector2Int pos)
    {
        // Is chardable if X amount of mines are marked around it
        int number = underlayBoxes[pos.x, pos.y].value;
        Debug.Log("underlayBoxes[pos.x, pos.y].value = " + underlayBoxes[pos.x, pos.y].value);
        //Debug.Log("Checking amt of marked neighbors = "+ MarkedNeighbors(pos.x, pos.y));
        return number == MarkedNeighbors(pos.x, pos.y);
    }

    private int MarkedNeighbors(int iCenter, int jCenter)
    {
        int amt = 0;
        // Determine neighbors
        for (int i = iCenter - 1; i <= iCenter + 1; i++)
        {
            for (int j = jCenter - 1; j <= jCenter + 1; j++)
            {
                if (i == iCenter && j == jCenter)
                    continue;
                if (i < 0 || j < 0 || i >= gameWidth || j >= gameHeight)
                    continue;
                if (overlayBoxes[i, j].Marked)
                    amt++;
            }
        }
        return amt;
    }

    public void TryOpenBox(Vector2Int pos,bool playerInitiated)
    {
        Debug.Log("BOX: Trying to open pos "+pos);
        // this methods ties to open the box and uses that info to check for complete game



        bool didOpen =  OpenBox(pos,playerInitiated);

        if(GamePaused)
            return; // Do not continue if it busted

        if (didOpen) {
            opened++;
            Debug.Log("BOX: Trying to open pos "+pos+" success: " + opened+" ["+pos+" , "+totalToOpen+"]");
            

            if(opened == totalToOpen) {
                Debug.Log("BOX: All Opened = WIN");
                WinBustLevel(GameResult.Win);
            }
        }
        else {
            Debug.Log("BOX: Trying to open pos "+pos+" FAILED: " + opened+" ["+pos+" , "+totalToOpen+"]");
        }
    }

    public bool OpenBox(Vector2Int pos, bool playerInitiated)
    {
        // Wait for first click to start timer
        if (USerInfo.WaitForFirstMove) {
            //Start the timer
            //Timer.Instance.StartTimer();
            USerInfo.WaitForFirstMove = false;
            Debug.Log("TIMER: Starting Minesweeper Timer");
            Timer.Instance.StartTimer();


            // If this is a mine swap it and recalculate the level
            if (mines[pos.x, pos.y] == -1) {
                Debug.Log("TIMER: Swapping this Mine");
                SwapAndRecalculateLevel(pos);
            }
        }



        // marked or allready open
        if (overlayBoxes[pos.x, pos.y].Marked)
            return false;

        // not marked or closed

        // unmarked and closed 
        if (overlayBoxes[pos.x, pos.y].IsClickable()) {
            overlayBoxes[pos.x, pos.y].Click();


            if (mines[pos.x, pos.y] == -1) {
                Debug.Log("BUST");

                WinBustLevel(GameResult.Bust);
                // this ends the game with a loss
                return true; // No need to count opened ones since player lose?
            }

            if (mines[pos.x, pos.y] == 0) {
                Debug.Log("Chord");

                OpenAllNeighbors(new Vector2Int(pos.x, pos.y));
                // this ends the game with a loss
                return true; // No need to count opened ones since player lose?
            }
            return true;

        }
        // If clicking an already opened box = chord it

        if (overlayBoxes[pos.x, pos.y].IsOpen) {

            Debug.Log("Box is not marked - so chord it");
            Chord(pos);
            return false;
        }
        Debug.Log("Shold never happen?");
        return false;        
    }

    private bool BasicOpeningBox(Vector2Int pos)
    {
        if (mines[pos.x, pos.y] == -1)
        {
            //Debug.Log("Bust");
            overlayBoxes[pos.x, pos.y].Bust();
            WinBustLevel(GameResult.Bust);
            return false;
        }
        if (mines[pos.x, pos.y] == 0)
        {
            //Debug.Log("Opening");
            overlayBoxes[pos.x, pos.y].RemoveAndSetUnderActive();
            opened++;
            OpenAllNeighbors(pos);
        }
        else
        {
            //Debug.Log("Number ");
            underlayBoxes[pos.x, pos.y].MakeInteractable();
            overlayBoxes[pos.x, pos.y].RemoveAndSetUnderActive();
            opened++;
        }
        return true;
    }

    private void EditToggleMine(Vector2Int pos)
    {
        throw new NotImplementedException();
    }

    enum GameResult{Win,Bust}

    private void WinBustLevel(GameResult result)
    {
        // Pause the timer
        Timer.Instance.Pause();

        //Debug.Log("Level Ended at time: " + Timer.TimeElapsed);
        Debug.Log("Game ended with "+result);
        // Go through all mines and flagg all un-flagged 
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                switch (result) {
                    case GameResult.Win:
                        if (mines[i, j] == -1)
                            overlayBoxes[i, j].Mark();
                        break;
                    case GameResult.Bust:
                        if (overlayBoxes[i, j].Marked && mines[i, j] != -1)
                            overlayBoxes[i, j].ShowWrongFlag();
                        else if (mines[i, j] == -1 && overlayBoxes[i, j].gameObject.activeSelf && !overlayBoxes[i, j].Marked && !overlayBoxes[i, j].Busted)
                            overlayBoxes[i, j].ShowMine();
                        break;
                }

            }
        }

        GamePaused = true;

        //LevelBusted = result == GameResult.Bust;
        B3V = Calculate3BV();


        switch (result) {
            case GameResult.Win:
                SmileyButton.Instance.ShowWin();

                MineSweeper.Instance.UpdateRating(USerInfo.BoardDifficulty);                

                /*
                // Add Stats
                if(USerInfo.currentType == GameType.Normal)
                    SavingUtility.gameSettingsData.NormalWon++;
                else if(USerInfo.currentType == GameType.Challenge)
                    SavingUtility.gameSettingsData.ChallengeWon++;        
                */
                break;
            case GameResult.Bust:
                SmileyButton.Instance.ShowBust();
                // Add Stats
                /*
                if (USerInfo.currentType == GameType.Normal)
                    SavingUtility.gameSettingsData.NormalLost++;
                else if (USerInfo.currentType == GameType.Challenge)
                    SavingUtility.gameSettingsData.ChallengeLost++;
                */
                break;
            default:
                break;
        }
        //SavingUtility.Instance.SaveAllDataToFile(); 
        //PanelController.Instance.ShowLevelComplete(result == GameResult.Win);
    }

    private int Calculate3BV()
    {
        // Go through all tiles, when finding an opening not used expand and use surroundings
        int[,] unused = new int[mines.GetLength(0),mines.GetLength(1)];
        int bv3Count = 0;
        for (int i = 0; i < mines.GetLength(0); i++)
        {
            for (int j = 0; j < mines.GetLength(1); j++)
            {
                if (unused[i, j] == 1)
                    continue;
                if (underlayBoxes[i,j].value == 0)
                {
                    // Found an unused opening = GROW
                    unused[i, j] = 1; // use this
                    bv3Count++;
                    Grow(new List<Vector2Int>() { underlayBoxes[i,j].Pos});
                }
            }
        }
        // Now all nonmine unused are clicks
        for (int i = 0; i < mines.GetLength(0); i++)
        {
            for (int j = 0; j < mines.GetLength(1); j++)
            {
                if (unused[i, j] == 0 && mines[i, j] != -1)
                    bv3Count++;
            }
        }
        return bv3Count;

        // Local Grow Method
        void Grow(List<Vector2Int> list)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>(list);
            while (queue.Count > 0)
            {
                Vector2Int pos = queue.Dequeue();  
                foreach (var step in steps)
                {
                    Vector2Int neighbor = pos + step;
                    if(neighbor.x<0|| neighbor.y<0||neighbor.x>=gameWidth||neighbor.y>=gameHeight)
                        continue;
                    if (underlayBoxes[neighbor.x, neighbor.y].value == 0 && unused[neighbor.x,neighbor.y]==0)
                    {
                        unused[neighbor.x, neighbor.y] = 1; // use it
                        queue.Enqueue(neighbor);
                    }
                    else
                    {
                        // Has to be a number
                        unused[neighbor.x, neighbor.y] = 1; // use it
                    }
                }
            }
        }
    }
    private Vector2Int[] steps = new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1) };

    private void HighLightAllNeighbors(Vector2Int pos)
    {
        UnsetAllPressedBoxes();

        if(!ValidCurrentPos())
            return;

        if (overlayBoxes[pos.x, pos.y].Marked || !(Mouse.current.leftButton.IsPressed() || Mouse.current.leftButton.IsPressed())) {
            // highlight neigbors
            return;
        }

        Debug.Log("HighLight all neighbors for "+pos);

        int iCenter = pos.x;
        int jCenter = pos.y;

        if (overlayBoxes[iCenter,jCenter].IsClickable()) {
            overlayBoxes[iCenter,jCenter].SetAsPressed();
            return;
        }

        for (int i = iCenter - 1; i <= iCenter + 1; i++)
        {
            for (int j = jCenter - 1; j <= jCenter + 1; j++)
            {
                if (i == iCenter && j == jCenter)
                    continue;
                if (i < 0 || j < 0 || i >= gameWidth || j >= gameHeight)
                    continue;

                if (overlayBoxes[i, j].IsClickable())
                {
                    overlayBoxes[i,j].SetAsPressed();
                }
            }
        }
    }

    private void UnsetAllPressedBoxes()
    {
        for (int j = 0; j < gameHeight; j++) {
            for (int i = 0; i < gameWidth; i++) {
                if (!overlayBoxes[i,j].IsClickable())
                    continue;

                overlayBoxes[i, j].UnPress();
            }
        }
    }

    private bool OpenAllNeighbors(Vector2Int pos)
    {
        Debug.Log("Opening all neighbors for "+pos);
        int iCenter = pos.x;
        int jCenter = pos.y;
        bool wasted = true; 

        for (int i = iCenter - 1; i <= iCenter + 1; i++)
        {
            for (int j = jCenter - 1; j <= jCenter + 1; j++)
            {
                if (i == iCenter && j == jCenter)
                    continue;
                if (i < 0 || j < 0 || i >= gameWidth || j >= gameHeight)
                    continue;

                if (overlayBoxes[i, j].IsClickable())
                {
                    TryOpenBox(new Vector2Int(i, j),false);

                    wasted = false;
                }
            }
        }
        return wasted;

    }

    private void SwapAndRecalculateLevel(Vector2Int pos)
    {
        Debug.Log("Mine at first click swap " + pos + " for " + swapBox);
        mines[pos.x, pos.y] = 0;
        mines[swapBox.x, swapBox.y] = -1;

        // Make underlaying
        RecalculateNeighbors(pos);
        RecalculateNeighbors(swapBox);

    }

    private int NeighborsCount(int iCenter, int jCenter)
    {
        int amt = 0;
        // Determine neighbors
        for (int i = iCenter - 1; i <= iCenter + 1; i++)
        {
            for (int j = jCenter - 1; j <= jCenter + 1; j++)
            {
                if (i == iCenter && j == jCenter)
                    continue;
                if (i < 0 || j < 0 || i >= gameWidth || j >= gameHeight)
                    continue;
                if (mines[i, j] == -1)
                    amt++;
            }
        }
        return amt;
    }

    private void RecalculateNeighbors(Vector2Int pos)
    {
        int iCenter = pos.x;
        int jCenter = pos.y;

        // Determine numbers
        for (int i = iCenter - 1; i <= iCenter + 1; i++)
        {
            for (int j = jCenter - 1; j <= jCenter + 1; j++)
            {
                if (i < 0 || j < 0 || i >= gameWidth || j >= gameHeight)
                    continue;
                if (mines[i, j] != -1)
                    mines[i, j] = NeighborsCount(i, j);
                underlayBoxes[i, j].SetType(mines[i, j]);
            }
        }
    }

    public void UpdateMineCount()
    {
        if (isOnlyView)
            return;
        if (USerInfo.currentType == GameType.Create)
        {
            //Show Mines Total
            if(USerInfo.EditMode == 0)
                mineDisplay.ShowValue(TotalAllMines());
            else
                mineDisplay.ShowValue(TotalUnFlagged());
            return;
        }
        //Debug.Log("Showing minecount "+mineCountAmount);
        // Set minecount
        mineDisplay?.ShowValue(mineCountAmount);
    }

    private int TotalAllMines()
    {
        int all = 0;
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {

                if (mines[i,j]==-1)
                    all++;
            }
        }
        return all;
    }
    
    private int TotalUnFlagged()
    {
        int unFlagged = 0;
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {

                if (mines[i,j]==-1 && overlayBoxes[i, j] != null && !overlayBoxes[i, j].Marked)
                    unFlagged++;
            }
        }
        return unFlagged;
    }
    
    private bool SomeOpened()
    {
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                if (mines[i,j]!=-1 && !overlayBoxes[i, j].gameObject.activeSelf)
                    return true;
            }
        }
        return false;
    }
    
    private int TotalClickable()
    {
        int clickable = 0;
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {

                if (mines[i,j]!=-1 && overlayBoxes[i, j].gameObject.activeSelf)
                    clickable++;
            }
        }
        return clickable;
    }

    private void DetermineNumbersFromNeighbors()
    {
        // Determine numbers
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                if (mines[i, j] != -1)
                {
                    mines[i, j] = NeighborsCount(i, j);
                    //Debug.Log("Neighbor for "+i+","+j+" = " + mines[i,j]);
                }
            }
        }
    }

    public void SizeGameArea(bool sizeFromSettings = true, bool resetMines = true)
    {
        Debug.Log("MS: Sizing Game Area ");
        // Not sure if it should be here
        LevelBusted = false;
        
        if (sizeFromSettings)
        {
            int boardSize = USerInfo.ActiveBordSize;
            Debug.Log("**** Loading game of type "+ USerInfo.BoardType);
            switch (USerInfo.BoardType)
            {
                case BoardTypes.Slider:
                    // Load info of current size
                    gameWidth = boardSize;
                    gameHeight = boardSize;
                    //float minePercent = (11 + boardSize * 0.5f) / 100;
                    totalmines = (int)(boardSize * boardSize * (11 + boardSize * 0.5f)/100);
                    break;
                case BoardTypes.Beginner:
                    Debug.Log("**** Loading beginner");
                    gameWidth = 8;
                    gameHeight = 8;
                    totalmines = 10;
                    break;
                case BoardTypes.Intermediate:
                    Debug.Log("**** Loading intermediate");
                    gameWidth = 16;
                    gameHeight = 16;
                    totalmines = 40;
                    break;
                case BoardTypes.Expert:
                    Debug.Log("**** Loading expert");
                    gameWidth = 30;
                    gameHeight = 16;
                    totalmines = 99;
                    break;
            }

            Debug.Log("Game Area - Load Game of Size [" + gameWidth + "x" + gameHeight + "]");

            // Use calculation instead of array to acomodate for any size

            // Mine% = 11 + size*0.5 => TotalMines = boardSize*boardSize*11 + size*0.5

            //totalmines = gameSizes[boardSize - 6]; // -6 since the lowest setting a gamearea can be is 6 and the index starsts at 0

            // Set mines array
            if (resetMines)
                mines = new int[gameWidth, gameHeight];
            else
            {
                // Copy last mines to the new array
                int[,] newMines = new int[gameWidth, gameHeight];
                int maxWidth = Math.Min(gameWidth, mines.GetLength(0));
                int maxHeight = Math.Min(gameHeight, mines.GetLength(1));
                for (int j = 0; j < maxHeight; j++)
                {
                    for (int i = 0; i < maxWidth; i++)
                    {
                        newMines[i, j] = mines[i, j];
                    }
                }
                mines = newMines;
            }
        }
        ResetBoxes();

        SetBorderSize();


    }

    private void SetBorderSize()
    {

        Debug.Log("MS: Set border Size");

        MineSweeper.Instance.SetBorderSize(new Vector2(BoxSize * gameWidth, BoxSize * gameHeight));
    }

    private void ResetBoxes()
    {
        for (int i = 0; i < underlayBoxes.GetLength(0); i++)
        {
            for (int j = 0; j < underlayBoxes.GetLength(1); j++)
            {
                if (underlayBoxes[i, j] != null)
                    Destroy(underlayBoxes[i, j].gameObject);
                if (overlayBoxes[i, j] != null)
                    Destroy(overlayBoxes[i, j].gameObject);
            }
        }

        // Set new boxes arrays size
        underlayBoxes = new UnderGameBox[gameWidth, gameHeight];
        overlayBoxes = new GameBox[gameWidth, gameHeight];

    }

    public void OnCreateBack(int[,] flagged)
    {
        // Flag all Mines
        FlagMinesForEditA(flagged);
    }

    public bool IsMine(Vector2Int pos)
    {
        return mines[pos.x, pos.y] == -1;
    }

    private void FlagAllMines()
    {
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                if (mines[i, j] == -1)
                    overlayBoxes[i, j].Mark();
            }
        }
    }
    
    private void FlagMinesForEditA(int[,] flags)
    {
        int maxHeight = Math.Min(gameHeight, flags.GetLength(1));
        int maxWidht = Math.Min(gameWidth, flags.GetLength(0));


        for (int j = 0; j < maxHeight; j++)
        {
            for (int i = 0; i < maxWidht; i++)
            {
                // Make uncleared
                if (mines[i, j] == -1)
                {
                    if (flags[i,j] == 1)
                        overlayBoxes[i, j].Mark();
                    else
                        overlayBoxes[i, j].SetAsHiddenMine();
                }
            }
        }
    }

    public void OnCreateNext()
    {
        SetMinesFromFlags();

        // TODO Figure out why Numbers are not added correctly

        DetermineNumbersFromNeighbors();

        UpdateNumbers();

        // Set minecount
        mineCountAmount = 0;
        UpdateMineCount();
    }

    private void SetMinesFromFlags()
    {
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                if (overlayBoxes[i, j].Marked)
                {
                    Debug.Log("Mine at " + i + "," + j + " since its marked");
                    mines[i, j] = -1;
                }
            }
        }
    }
    private void UpdateNumbers(bool keepCollidersActive = false)
    {
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                if (mines[i, j] != -1)
                {
                    underlayBoxes[i, j].SetType(mines[i, j],keepCollidersActive);
                }
            }
        }

    }

    public Vector2 BorderAreaRendererWidth() => new Vector2(gameWidth / 2f + borderAddon.x, gameHeight / 2f + borderAddon.y);
    
    public float SmileyColliderWidth() => gameWidth / 2f + borderAddon.x;

    public int[,] GetFlaggedArray()
    {
        int[,] flagged = new int[gameWidth, gameHeight];
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                if (overlayBoxes[i,j].Marked)
                    flagged[i, j] = 1;
            }
        }
        return flagged;
    }


    public bool UnSolved(Vector2Int pos) => overlayBoxes[pos.x, pos.y].UnSolved();



    public string ValidateLevel()
    {
        // Checks that a level is Valid and return the result
        // Is there at least one clickable spot
        if (TotalClickable() == 0)
            return "You can not save a level with all boxes opened.";
        if(!SomeOpened())
            return "You can not save a level with no boxes opened.";
        return "Valid";
    }

    internal void UpdateTheme()
    {
        UpdateNumbers(true);
                        
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                
                overlayBoxes[i, j].UpdateSprite();
                
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GamePaused) 
            return;

        UpdatePressedNeighbors(eventData);

        // Store the downpress only to use for fast move
        lastMouseDownPos = GetMouseLocalPosition(eventData);
    }

    private void UpdatePressedNeighbors(PointerEventData eventData)
    {
        // Store this new position
        currentMousePos = GetMouseLocalPosition(eventData);

        // Checking for pressed highlighting
        HighLightAllNeighbors(currentMousePos);
    }

    Vector2Int lastMouseDownPos = new Vector2Int();
    Vector2Int currentMousePos = new Vector2Int();

    float lastChangedPosTime = 0;

    public void OnPointerMove(PointerEventData eventData)
    {
        if (GamePaused) 
            return;
        
        UpdatePressedNeighbors(eventData);


        /*
        // Get the pointer position
        Vector2Int mousePos = GetMouseLocalPosition(eventData);


        // Right Clicking already opened box
        if ((Mouse.current.leftButton.IsPressed() || Mouse.current.leftButton.IsPressed()) && !overlayBoxes[mousePos.x, mousePos.y].Marked) {
            // highlight neigbors
            HighLightAllNeighbors(mousePos);
            return;
        }
        */
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Disregard inputs when paused
        if(GamePaused)
            return; 

        UnsetAllPressedBoxes();

        UpdateMouseLocalPosition(eventData);

        Debug.Log("Pointer up on this spot "+localPosition);

        int xPos = (int)localPosition.x / BoxSize;
        int yPos = (int)-localPosition.y / BoxSize;
        if (xPos < 0 || xPos >= gameWidth || yPos < 0 || yPos >= gameHeight) {
            Debug.Log("Invalid minesweeper game area click - return");
            return;
        }

        Debug.Log("Clicking index ["+xPos+","+yPos+"]");

        // Opening 
        // Have to know if this is an open spot or not
        // If cleard do 
        // if not open or bust
            Debug.Log("No Mine: "+ mines[xPos, yPos]);

        Vector2Int pos = new Vector2Int(xPos, yPos);

        if (eventData.button == PointerEventData.InputButton.Right) {
            
            // If it was released after moving off a correct spot
            //float timePassedSincePosChange = Time.time - lastChangedPosTime;
            if(ValidCurrentPos() && overlayBoxes[pos.x, pos.y].IsOpen) {
                // Instead right clik the last position instead
                Debug.Log("RIGHT CLICKING LAST POS INSTEAD OF RELEASING RIGHT BUTTON OVER NOTHING");
                Debug.Log("Last position = "+lastMouseDownPos.x+","+lastMouseDownPos.y+" <- mark this");
                Debug.Log("This position = "+pos.x+","+pos.y);

                MarkBox(lastMouseDownPos);
                return;
            }

            MarkBox(pos);
            return;
        }
        TryOpenBox(pos,true);
    }

    private bool ValidCurrentPos() => currentMousePos.x >= 0 && currentMousePos.x < gameWidth && currentMousePos.y >= 0 && currentMousePos.y < gameHeight;

    private void MarkBox(Vector2Int pos)
    {
        Debug.Log("Marking "+pos+" boxes size = "+overlayBoxes.GetLength(0)+""+overlayBoxes.GetLength(1));
        // 11,9 is outside range why?
        if (overlayBoxes[pos.x, pos.y].IsOpen)
            return;
        overlayBoxes[pos.x, pos.y].RightClick();
    }

    private Vector2Int GetMouseLocalPosition(Vector2 pos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            pos,
            Camera.main,
            out pos
        );

        int xPos = (int)pos.x / BoxSize;
        int yPos = (int)-pos.y / BoxSize;

        return new Vector2Int(xPos, yPos);
    }
    
    private Vector2Int GetMouseLocalPosition(PointerEventData eventData)
    {
        Vector2 pos = new();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        int xPos = (int)pos.x / BoxSize;
        int yPos = (int)-pos.y / BoxSize;

        return new Vector2Int(xPos,yPos);
    }
    
    private void UpdateMouseLocalPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPosition
        );
    }
}
