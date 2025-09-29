using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class MiniGameBase: MonoBehaviour
{
    [SerializeField] private MiniGame gameType = MiniGame.Chess;
    [SerializeField] protected MiniGame GameType => gameType;
}
public class MineSweeper : MiniGameBase
{
    // Game Area
    [Header("Game Area")]
    [SerializeField] private GameArea gameArea;
    [SerializeField] private Image image; // Image for game area - need this size to calculate the total size including the border of the total game area

    // Game Area Stuff
    [SerializeField] private SmileyButton smiley;
    [SerializeField] private GameObject smileyHolder;
    [SerializeField] private GameObject mineCount;
    [SerializeField] private GameObject timeCount;

    // Digi Displays
    [SerializeField] DigiDisplay mineDisplay;
    [SerializeField] DigiDisplay timeDisplay;

    private Vector2 gameBorderSize = new();

    // Rating Textfields
    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;

    // Extra Panels
    [SerializeField] private GameObject helpInfo;
    [SerializeField] private MiniGameChessWinNotice winNotice;

    [Header("EXTRAS")]
    [SerializeField] private int gameWidth;
    [SerializeField] private int gameHeight;
    

    public static MineSweeper Instance { get; private set; }
    public bool WaitForFirstMove { get; private set; } = true;

    void Awake()
    {
        Debug.Log("MS: Level Creator Start");
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        // Defining Border Dimentions so the total size can be calculated for each game size later
        gameBorderSize = new Vector2(image.sprite.border.x + image.sprite.border.z, image.sprite.border.w + image.sprite.border.y);

        Stats.StatsUpdated += OnStatsUpdated;
    }

    private void OnEnable()
    {
        UpdateRatingText();
        
    }

    private void OnStatsUpdated()
    {
        UpdateRatingText();
    }

    private void OnDisable()
    {
        Debug.Log(" -- OnDisable Level Creator -- ");

        USerInfo.BoardSizeChange -= OnPlaySizeChange;
    }

    public void AlignGameArea(bool keepZoom = false)
    {
        Debug.Log("LevelCreator - Align Game Area");
        //ScaleGameAreaBorder();
//        AlignSmileyAndCounterIcons();
        //if(!keepZoom)
        //    SetCameraOrthographicSize();
        //CenterGameArea();
    }

    public void OnPlaySizeChange()
    {
        Debug.Log("LevelCreator - Play size changed, restarting Game");
        //BottomInfoController.Instance.ShowDebugText("Play Size Changing to "+USerInfo.ActiveBordSize);
        RestartGame(false,true);
    }

    internal void ChangeGameSize(int type)
    {
        USerInfo.BoardType = (BoardTypes)(type + 1);
        Debug.Log("MS: Changing board type to " + type + " " + USerInfo.BoardType);
        RestartGame();
    }

    public void RestartGame(bool keepZoom = false,bool resetPosition = false)
    {
        // When restarting a game make a load screen that prohibits player from clicking anything
        //PanelController.Instance.ShowLoaderPanelGame();

        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

        Debug.Log("LevelCreator - RestartGame");
        gameArea.RestartGame(resetPosition);        

        //await Task.Run(() => gameArea.RestartGame(resetPosition));        

        AlignGameArea(keepZoom);

        Debug.Log("--- RestartGame USerInfo.UseRotatedExpert = " + USerInfo.UseRotatedExpert);
        if (USerInfo.BoardType == BoardTypes.Expert && USerInfo.UseRotatedExpert) {
            Debug.Log("Align TOP!");
            //CameraController.Instance.AlignTop();
        }
        //PanelController.Instance.RemoveLoaderPanelGame();

        return;
    }

    public void LoadedGameFinalizing(bool editorcreateMode = false)
    {
        AlignGameArea();
        SmileyButton.Instance.ShowNormal();
        if(!editorcreateMode)
            Timer.Instance.StartTimer();
    }

    internal void SetBorderSize(Vector2 vector2)
    {
        Debug.Log("Scaling Transform to incorporate the game area and borders");

        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 finalDimentions = vector2 + gameBorderSize;

        rectTransform.sizeDelta = finalDimentions;
    }

    internal void UpdateRating(int boardDifficulty)
    {
        // Award Rating and reward
        int increase = Stats.MinesweeperRatingGain(boardDifficulty);

        // Award Rating and reward
        Stats.ChangeMiniGameRating(MiniGame.MineSweeper, increase);

        // Also make this general?
        winNotice.gameObject.SetActive(true);
        winNotice.SetWin(true);

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

    public void ToggleHelpInfoPanel()
    {
        helpInfo.SetActive(!helpInfo.activeSelf);
    }


    // RATING
    private void UpdateRatingText()
    {
        playerRating.text = "Rating: " + Stats.MiniGameRating(GameType);     
        Debug.Log("UPDATING PLAYER RATING");
    }

}
