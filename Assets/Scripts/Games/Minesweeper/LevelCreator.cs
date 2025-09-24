using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class LevelCreator : MonoBehaviour
{
    [SerializeField] private GameArea gameArea;

    [SerializeField] private int gameWidth;
    [SerializeField] private int gameHeight;
    
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI amtText;
    [SerializeField] private TextMeshProUGUI playerIDText;
    [SerializeField] private TextMeshProUGUI appRef;

    //[SerializeField] private GameBox mineBoxPrefab;
    //[SerializeField] private GameBox unclearedBoxPrefab;
    //[SerializeField] private GameBox underlayBoxPrefab;

    //[SerializeField] private GameBox[] numberPrefabs;

    //[SerializeField] private LevelCompletionScreen levelCompletionPanel;
    [SerializeField] private GameObject boxHolder;
    [SerializeField] private GameObject underLaying;
    [SerializeField] private GameObject borderArea;
    [SerializeField] private GameObject playArea;
    [SerializeField] private SpriteRenderer borderAreaRenderer;
    [SerializeField] private BoxCollider2D borderAreaCollider;
    [SerializeField] private GameObject objects;
    [SerializeField] private GameObject origo;
    [SerializeField] private GameObject alignPosition;
    Vector3 align = new Vector3(0.5f, -0.5f, 0); 
    //Vector3 align = new Vector3(0f, 0f, 0f);
    Vector2 borderAddon = new Vector3(0.3f, 0.81f);
    //Vector2 borderAddon = new Vector3(0.8f, 1.31f);
    //Vector3 borderAlign = new Vector3(0.17f, -0.72f, 0);

    Vector3 boxScale = new Vector3(0.48f, 0.48f, 1f);

    //Vector3 boxScale = new Vector3(0.5882f, 0.5882f, 1f);

    [SerializeField] private SmileyButton smiley;
    [SerializeField] private GameObject smileyHolder;
    [SerializeField] private GameObject mineCount;
    [SerializeField] private GameObject timeCount;
    private int mineCountAmount=0;
    private int totalmines=0;
    [SerializeField] DigiDisplay mineDisplay;
    [SerializeField] DigiDisplay timeDisplay;

    Vector2[] sizePositions = new Vector2[3] { new Vector2(-2.2f,2.94f), new Vector2(-2.2f, 2.94f) , new Vector2(-2.2f, 2.94f) };


    int[] gameSizes = new int[10] { 5, 7, 10, 13, 16, 20, 24, 30, 35, 42 };

    float[] sizeScales = new float[3] { 0.832f, 0.832f, 0.832f};


    // Should there be one actual board and one visual board? yes? Only mines are necessarey to describe actual board


    public static LevelCreator Instance { get; private set; }
    public bool WaitForFirstMove { get; private set; } = true;
    public bool EditMode { get; set; } = false;
    public bool EditModeB { get; set; } = false;

    int[,] mines;
    GameBox[,] underlayBoxes = new GameBox[0,0];
    GameBox[,] overlayBoxes = new GameBox[0, 0];
    private int opened;
    private int totalToOpen;
    private Vector2Int swapBox;

    void Start()
    {

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //gameArea = new GameArea();

        //gameArea.RestartGame();

        // Add size change listener
        Debug.Log(" -- Registrating OnPlayerSignedInSuccess -- In Level Creator -- " + gameObject.GetInstanceID(), this);
        //FirestoreManager.LoadComplete += OnLoadLevelComplete;
        USerInfo.BoardSizeChange += OnPlaySizeChange;
    }
    private void OnDisable()
    {
        Debug.Log(" -- OnDisable Level Creator -- ");

        USerInfo.BoardSizeChange -= OnPlaySizeChange;
    }

    public void SetAppRef(string s) => appRef.text = s;
    private void AlignBoxesAnchor()
    {
        // Align GameArea
        objects.transform.position = origo.transform.position;
        objects.transform.localScale = Vector3.one * 0.5f;
    }

    private void AlignSmileyAndCounterIcons()
    {

        // Place smiley at half width of game area
        mineCount.transform.localPosition = new Vector3(0.5f, smileyHolder.transform.localPosition.y, 0);
        smileyHolder.transform.localPosition = new Vector3(borderAreaRenderer.size.x / 2, smileyHolder.transform.localPosition.y, 0);
        smiley.SetColliderWidth(gameArea.SmileyColliderWidth());
        timeCount.transform.localPosition = new Vector3(borderAreaRenderer.size.x - 0.5f, smileyHolder.transform.localPosition.y, 0);
    }

    /*
    private void SizeGameArea(bool sizeFromSettings = true,bool resetMines = true)
    {

        if (sizeFromSettings)
        {
            int boardSize = USerInfo.Instance.BoardSize;
            // Change to Use Userinfo?
            Debug.Log("Load Game Size " + boardSize);
            // Load info of current size
            gameWidth = boardSize;
            gameHeight = boardSize;
            totalmines = gameSizes[boardSize - 6]; // -6 since the lowest setting a gamearea can be is 6 and the index starsts at 0

            // Set mines array
            if (resetMines)
                mines = new int[gameWidth, gameHeight];
        }
        else
            Debug.Log("Sizing game Area from Loaded File instead of settings");

        DefineUnderAndOverBoxes();

        // Set correct size of the border

        // Screen alignments
        AlignGameArea();
    }*/

    public void AlignGameArea(bool keepZoom = false)
    {
        Debug.Log("LevelCreator - Align Game Area");
        ScaleGameAreaBorder();
        AlignSmileyAndCounterIcons();
        if(!keepZoom)
            SetCameraOrthographicSize();
        CenterGameArea();
    }

    private void ScaleGameAreaBorder() => borderAreaRenderer.size = GameAreaMaster.Instance.MainGameArea.BorderAreaRendererWidth();

    private void CenterGameArea() => playArea.transform.position = new Vector3(-borderAreaRenderer.size.x / 2 * playArea.transform.localScale.x, borderAreaRenderer.size.y / 2 * playArea.transform.localScale.y, 0);

    private void SetCameraOrthographicSize()
    {
        //Debug.Log(" SetCameraOrthographicSize");
        float screenRatio = (float)Screen.height / Screen.width;
        float targetWidthInWorldUnits = borderAreaRenderer.size.x;
        float orthographicSize = targetWidthInWorldUnits * screenRatio / 2;
        Camera.main.orthographicSize = orthographicSize;

        borderAreaCollider.size = borderAreaRenderer.size;
        borderAreaCollider.offset = new Vector2(borderAreaRenderer.size.x/2, -borderAreaRenderer.size.y / 2);

        // Set the camarecontroller limits
        //CameraController.Instance.OriginalOrthogonalSize = orthographicSize;
        //CameraController.Instance.MaxZoomPosition = orthographicSize; // 3 seems good?

    }


    /*
    public void OnRequestLoadLevel(InputAction.CallbackContext context)
    {
        Debug.Log("Loading Level requested");
        LoadRandomLevel();
    }*/

    public void OnUnselectAllClickedNonMine()
    {
        Debug.Log("OnUnselectAllClickedNonMine");

        // Unload All but mines
        int[,] flagged = GameAreaMaster.Instance.MainGameArea.GetFlaggedArray();
        // This keeps the mines
        //OnToggleCreate(false,false); 

        totalmines = 0;
        mineCountAmount = 0;
        GameAreaMaster.Instance.MainGameArea.OnCreateBack(flagged);

    }

    public void OnChangeSizeCreate(int[,] flagged)
    {
        Debug.Log("OnChangeSizeCreate");
        //OnToggleCreate(false,true);
        Debug.Log("Changing Size Create");
        // Place flags and ghost mines
        
        GameAreaMaster.Instance.MainGameArea.OnCreateBack(flagged);
    }

    public void OnPlaySizeChange()
    {
        Debug.Log("LevelCreator - Play size changed, restarting Game");
        //BottomInfoController.Instance.ShowDebugText("Play Size Changing to "+USerInfo.Instance.ActiveBordSize);
        RestartGame(false,true);
    }

    public void RestartGame(bool keepZoom = false,bool resetPosition = false)
    {
        // When restarting a game make a load screen that prohibits player from clicking anything
        //PanelController.Instance.ShowLoaderPanelGame();

        Debug.Log("LevelCreator - RestartGame");
        gameArea.RestartGame(resetPosition);        

        //await Task.Run(() => gameArea.RestartGame(resetPosition));        

        AlignGameArea(keepZoom);

        Debug.Log("--- RestartGame USerInfo.Instance.UseRotatedExpert = " + USerInfo.Instance.UseRotatedExpert);
        if (USerInfo.Instance.BoardType == BoardTypes.Expert && USerInfo.Instance.UseRotatedExpert) {
            Debug.Log("Align TOP!");
            //CameraController.Instance.AlignTop();
        }
        //PanelController.Instance.RemoveLoaderPanelGame();

        return;
    }

    private void DefineUnderAndOverBoxes()
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
        underlayBoxes = new GameBox[gameWidth, gameHeight];
        overlayBoxes = new GameBox[gameWidth, gameHeight];

    }

    public void LoadedGameFinalizing(bool editorcreateMode = false)
    {
        AlignGameArea();
        SmileyButton.Instance.ShowNormal();
        if(!editorcreateMode)
            Timer.Instance.StartTimer();
    }

    public void ApplyFlagged(int[,] flagged)
    {
        Debug.Log("ApplyFlagged");
    }


    /*
    private void PreOpenAndFlag(int[,] gameLoaded)
    {
        TotalMines();
        Debug.Log("Open correct Boxes");
        // Go through all mines and flagg all un-flagged 
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                //Debug.Log("gameLoaded["+i+","+j+"] = "+ gameLoaded[i, j]);
                if (gameLoaded[i,j]==2)
                    overlayBoxes[i, j].Mark();
                else if (gameLoaded[i, j] == 3)
                {
                    overlayBoxes[i, j].RemoveAndSetUnderActive();
                    underlayBoxes[i, j].MakeInteractable();
                    opened++;
                }
            }
        }

        UpdateMineCount();
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

    private void UpdateNumbers()
    {
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                if (mines[i, j] != -1)
                {
                    underlayBoxes[i, j].SetType(mines[i,j]);
                }
            }
        }

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
                    Debug.Log("Mine at "+i+","+j+" since its marked");
                    mines[i, j] = -1;
                }
            }
        }
    }



    public void RestartGame()
    {
        gameArea.RestartGame();
        return;

        SizeGameArea();

        RandomizeMines();
        DrawLevel();
        ResetLevel();
        AlignBoxesAnchor();
        SmileyButton.Instance.ShowNormal();
        Timer.Instance.ResetCounterAndPause();
        WaitForFirstMove = true;
        USerInfo.Instance.levelID = "RANDOM " + gameWidth + "x" + gameHeight;
        levelText.text = USerInfo.Instance.levelID;
        amtText.text = ""+FirestoreManager.Instance.LoadedAmount;
        USerInfo.Instance.currentType = GameType.Normal;
        BackgroundController.Instance.SetColorNormal();

    }


    public bool OpenBox(Vector2Int pos)
    {
        //Debug.Log("Open Box "+pos);
        if (EditMode)
        {
            OpenBoxEditMode(pos);
            return true;
        }
        //Debug.Log("Open box "+pos);
        if (WaitForFirstMove)
        {
            //Start the timer
            Timer.Instance.StartTimer();
            WaitForFirstMove = false;

            // If this is a mine swap it and recalculate the level
            if (mines[pos.x, pos.y] == -1)
            {
                SwapAndRecalculateLevel(pos);
            }

        }
        if (Timer.Instance.Paused)
        {
            Debug.Log("Timer Paused skip");
        }
        // If allready open skip
        //if (!overlayBoxes[pos.x, pos.y].Active)
        

        //Debug.Log("Opening pos "+pos+" total opened = "+opened+"/"+totalToOpen);
        if (mines[pos.x, pos.y] == -1)
        {
            //Debug.Log("Bust");
            overlayBoxes[pos.x, pos.y].Bust();
            BustLevel();
            return false;
        }
        if(mines[pos.x, pos.y] == 0)
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
            overlayBoxes[pos.x,pos.y].RemoveAndSetUnderActive();
            opened++;
        }
        // If last opened is a number check if game is cleared?
        if (opened == totalToOpen && !Timer.Instance.Paused)
        {
            WinLevel();                
        }
        return true;
    }

    private void OpenBoxEditMode(Vector2Int pos)
    {
        Debug.Log("Toggle Edit Mode Mine "+pos);    
        
        // Toggle Mine
        mines[pos.x, pos.y] = mines[pos.x, pos.y] == -1?0:-1;
        overlayBoxes[pos.x, pos.y].RightClick();
    }

    private void BustLevel()
    {
        // Pause the timer
        Timer.Instance.Pause();
        LevelBusted = true;

        Debug.Log("Bust Level");
        // Go through all flagged boxes and change wrongly marked to red flags and show all unmarked mines
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // If flagged and wrong change to red flag
                if (overlayBoxes[i, j].Marked && mines[i, j] != -1)
                    overlayBoxes[i, j].ShowWrongFlag();
                else if (mines[i,j]==-1 && overlayBoxes[i, j].gameObject.activeSelf && !overlayBoxes[i, j].Marked && !overlayBoxes[i, j].Busted)
                    overlayBoxes[i, j].ShowMine();
            }
        }
        SmileyButton.Instance.ShowBust();
    }
    
    private void WinLevel()
    {
        // Pause the timer
        Timer.Instance.Pause();

        Debug.Log("Win Level "+Timer.TimeElapsed);
        // Go through all mines and flagg all un-flagged 
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // If flagged and wrong change to red flag
                if (overlayBoxes[i, j].Marked && mines[i, j] != -1)
                    overlayBoxes[i, j].ShowWrongFlag();
                //else if (mines[i,j]==-1 && overlayBoxes[i, j].gameObject.activeSelf)
                else if (mines[i,j]==-1)
                    overlayBoxes[i, j].Mark();
            }
        }
        SmileyButton.Instance.ShowWin();

        LevelBusted = false;

        // Open Completion Panel - Pick correct one depending on level type
        PanelController.Instance.ShowLevelComplete();
        
    }

    private void ResetLevel()
    {
        Debug.Log("Resetting under and over boxes in arrays");

        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                overlayBoxes[i, j].Reset();

                // Make underlaying
                underlayBoxes[i, j].SetType(mines[i, j]);
            }
        }
        TotalMines();
        UpdateMineCount();
    }

    private void SwapAndRecalculateLevel(Vector2Int pos)
    {
        Debug.Log("Mine at first click swap "+pos+" for "+swapBox);
        mines[pos.x, pos.y] = 0;
        mines[swapBox.x, swapBox.y] = -1;

        // Make underlaying
        RecalculateNeighbors(pos);
        RecalculateNeighbors(swapBox);

    }

    private void DrawLevel()
    {
        Debug.Log("Creating new under and over boxes in arrays");
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                // Make uncleared
                GameBox box = Instantiate(unclearedBoxPrefab, boxHolder.transform);
                box.transform.localPosition = new Vector3(i, -j, 0)+align;
                box.transform.localScale = boxScale;
                box.Pos = new Vector2Int(i,j);
                overlayBoxes[i, j] = box;

                // Make underlaying
                GameBox underlayBox = Instantiate(underlayBoxPrefab, underLaying.transform);
                underlayBox.transform.localPosition = new Vector3(i, -j, 0) + align;
                underlayBox.transform.localScale = boxScale;
                underlayBox.Pos = new Vector2Int(i,j);
                underlayBoxes[i, j] = underlayBox;
                underlayBox.SetType(mines[i, j]);
                if(i==3 && j== 3)
                    Debug.Log("Setting UnderLayBox to "+ mines[i, j]);
            }
        }
        Timer.Instance.Pause();
    }

    private void RandomizeMines()
    {
        mineCountAmount = 0;

        // Place total Mines at random positions A (Get all positions and take one at random)

        int[] allPos = Enumerable.Range(0, gameHeight * gameWidth).ToArray();
        // Fisher-Yates scramble
        allPos = FisherYatesScramble(allPos);
        int row = allPos[allPos.Length - 1] / gameWidth;
        int col = allPos[allPos.Length - 1] % gameWidth;
        swapBox = new Vector2Int(row, col);


        for (int i = 0; i < totalmines; i++)
        {
            row = allPos[i] / gameWidth;
            col = allPos[i] % gameWidth;
            mines[row, col] = -1;
        }
        mineCountAmount = totalmines;
        totalToOpen = gameWidth * gameHeight - totalmines;
        DetermineNumbersFromNeighbors();
        // Set minecount
        UpdateMineCount();
    }

    private void DetermineNumbersFromNeighbors()
    {
        // Determine numbers
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                if (mines[i, j] != -1) {
                    mines[i, j] = Neighbors(i, j);
                    //Debug.Log("Neighbor for "+i+","+j+" = " + mines[i,j]);
                }
            }
        }
    }

    private int[] FisherYatesScramble(int[] allPos)
    {
        int n = allPos.Length;
        for(int i = 0;i < n; i++)
        {
            int temp = allPos[i];
            // Rendom pos
            int random = Random.Range(0,n);
            allPos[i] = allPos[random];
            allPos[random] = temp;

        }
        return allPos;
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
                    mines[i, j] = Neighbors(i, j);
                underlayBoxes[i,j].SetType(mines[i, j]);
            }
        }
    }
    
    private int Neighbors(int iCenter, int jCenter)
    {
        int amt = 0;
        // Determine neighbors
        for (int i = iCenter-1; i <= iCenter+1; i++)
        {
            for (int j = jCenter-1; j <= jCenter+1; j++)
            {
                if (i == iCenter && j == jCenter)
                    continue;
                if(i<0 || j<0 || i>=gameWidth || j >= gameHeight)
                    continue;
                if (mines[i,j]==-1)
                    amt++;  
            }
        }
        return amt;
    }


    public void Chord(Vector2Int pos)
    {
        //Debug.Log("Charding levelcreator at "+pos);
        if (Chordable(pos))
        {
            //Debug.Log("Chardable");
            OpenAllNeighbors(pos);
        }
    }

    private void OpenAllNeighbors(Vector2Int pos)
    {
        int iCenter = pos.x;
        int jCenter = pos.y;

        for (int i = iCenter - 1; i <= iCenter + 1; i++)
        {
            for (int j = jCenter - 1; j <= jCenter + 1; j++)
            {
                if (i == iCenter && j == jCenter)
                    continue;
                if (i < 0 || j < 0 || i >= gameWidth || j >= gameHeight)
                    continue;
                if (overlayBoxes[i, j].gameObject.activeSelf && !overlayBoxes[i, j].Marked)
                {
                    //Debug.Log("Opening overlayBox ["+i+","+j+"] since it is active");
                    OpenBox(new Vector2Int(i,j));
                }
            }
        }
    }

    private bool Chordable(Vector2Int pos)
    {
        // Is chardable if X amount of mines are marked around it
        int number = underlayBoxes[pos.x, pos.y].value;
        return number == MarkedNeighbors(pos.x,pos.y);
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
                if (overlayBoxes[i,j].Marked)
                    amt++;
            }
        }
        return amt;
    }

    public void TotalMines()
    {
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

    public void UpdateMineCount()
    {
        if (EditMode)
        {
            Debug.Log("Edit Mode Update flagged Mines to show mine count");
            //Show Mines Total
            mineDisplay.ShowValue(TotalFlaggedMines());
            return;
        }
        //Debug.Log("Showing minecount "+mineCountAmount);
        // Set minecount
        mineDisplay.ShowValue(mineCountAmount);
    }

    private int TotalFlaggedMines()
    {
        int flagged = 0;
        for (int j = 0; j < gameHeight; j++)
        {
            for (int i = 0; i < gameWidth; i++)
            {
                
                if(overlayBoxes[i, j].Marked)
                    flagged++;
            }
        }
        Debug.Log("Flagged amount = "+flagged);
        return flagged;
    }

    public bool IsMine(Vector2Int pos)
    {
        return mines[pos.x, pos.y] == -1;
    }
    */

}
