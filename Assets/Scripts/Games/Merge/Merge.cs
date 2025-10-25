using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class Merge : MiniGameBase
{

    [SerializeField] private GameObject tilePosPrefab; 
    [SerializeField] private MergeTile tilePrefab; 
    [SerializeField] private GameObject tilePosHolder; 
    [SerializeField] private GameObject tileHolder; 

    private GameObject[,] tilePositions;
    private MergeTile[,] tiles;

    private bool GameActive = false;

    // Extra Panels
    [SerializeField] private GameObject helpInfo;
    [SerializeField] private MiniGameChessWinNotice winNotice;

    // Rating Textfields
    [SerializeField] private TextMeshProUGUI playerRating;
    [SerializeField] private TextMeshProUGUI gameRatingValueText;
    [SerializeField] private TextMeshProUGUI playerRatingIncreaseText;
    [SerializeField] private GameObject playerRatingIncrease;

    int bestLevel = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateTileSpots();

        RestartGame();

        Stats.StatsUpdated += OnStatsUpdated;
    }

    private void OnStatsUpdated()
    {
        UpdateRating();
    }

    private void UpdateRating()
    {
        Debug.Log("Merge: Updating Rating");
        playerRating.text = Stats.MiniGameRating(GameType).ToString();
    }

    private void UpdateGameValueText() => gameRatingValueText.text = RatingGain(bestLevel).ToString();

    public void RestartGame()
    {
        ClearBoard();

        bestLevel = 1;
        UpdateGameValueText();

        SpawnRandomTile(2);

        GameActive = true;


        // Remove Win Screen Notice
        winNotice.gameObject.SetActive(false);

    }

    private void ClearBoard()
    {
        // Destroy all tiles
        for (int j = 0; j < tiles.GetLength(1); j++) {
            for (int i = 0; i < tiles.GetLength(0); i++) {
                if (tiles[i, j] != null) {
                    Destroy(tiles[i, j].gameObject);
                }   
            }
        }

        // Clear the tiles
        tiles = new MergeTile[4, 4]; 
    }

    private void OnEnable()
    {
        UpdateRating();
        Inputs.Instance.PlayerControls.Player.Move.performed += OnPlayerTryMove;
    }
    
    private void OnDisable()
    {
        Inputs.Instance.PlayerControls.Player.Move.performed -= OnPlayerTryMove;
    }

    private void OnPlayerTryMove(InputAction.CallbackContext context)
    {
        if (!GameActive) return;

        bool didMove = TryMove(context.ReadValue<Vector2>());
        if (didMove) {
            StartCoroutine(WaitForAnimation());
        }   
    }

    private IEnumerator WaitForAnimation()
    {
        // Limit player Input
        GameActive = false;
        yield return new WaitForSeconds(0.1f);
        GameActive = true;
        // Activate player Input

        // Need to wait for this to happen
        SpawnRandomTile();
        
    }

    private void SpawnRandomTile(int amt = 1)
    {
        // Get all free spots
        List<Vector2Int> freeSpots = new();

        for (int j = 0; j < tiles.GetLength(1); j++) {
            for (int i = 0; i < tiles.GetLength(0); i++) {
                if (tiles[i, j] == null)
                    freeSpots.Add(new Vector2Int(i, j));
            }
        }
        
        for (int i = 0; i < amt; i++) {
            if(freeSpots.Count == 0) break;
            int randomIndex = UnityEngine.Random.Range(0, freeSpots.Count);
            int randomLevel = UnityEngine.Random.Range(0, 3);
            int generateLevel = randomLevel >= 2 ? 2 : 1;

            // Takes care of updating the game value if any of the new generated pieces are better than any current pieces (only happens on start?)
            bestLevel = Math.Max(bestLevel, generateLevel);
            PlaceNewTileAt(freeSpots[randomIndex], generateLevel);
            freeSpots.RemoveAt(randomIndex);
            if (freeSpots.Count == 0 && CheckForLose()) {
                Win(false);
            }
        }

        // Makes sure Game Value is Updated
        UpdateGameValueText();
    }

    private void Win(bool didWin)
    {
        Debug.Log("YOU LOSE");
        GameActive = false;

        // Add gem if above X in Max Level

        int bestLevel = GetBestLevel();

        Debug.Log("BestLevel = "+bestLevel);

        // Add Gem
        if(bestLevel >= 10)
            Stats.GemGain(GameType);

        // Also make this general?
        winNotice.gameObject.SetActive(true);
        winNotice.SetWin(didWin);

        // Added Rating

        // Let the rating player got be the added value?

        int ratingAchieved = RatingGainActual(bestLevel);
        int currentRating = Stats.MiniGameRatings[(int)GameType];

        int increase = ratingAchieved - currentRating;
        Debug.Log("Merge game increase = "+increase+" new rating "+ratingAchieved);
        if (increase <= 0) {
            // Do not change rating if not better than current
            return;
        }

        // Award Rating and reward
        Stats.SetMiniGameValue(GameType, ratingAchieved);

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

    private int RatingGain(int bestLevel) => Math.Min(2999, Math.Max(0,(bestLevel - 1) * 200));
    private int RatingGainActual(int bestLevel) => Math.Max(1000, RatingGain(bestLevel));

    private int GetBestLevel()
    {
        int best = 1;
        for (int j = 0; j < tiles.GetLength(1) - 1; j++) {
            for (int i = 0; i < tiles.GetLength(0) - 1; i++) {
                if (tiles[i, j] == null) continue;
                best = Math.Max(best, tiles[i, j].Level);
            }
        }
        return best;
    }

    private bool CheckForLose()
    {
        // Reaching this when entire board is filled
        // Lost if no box has same level neighbor
        // Only check right and down
        for (int j = 0; j < tiles.GetLength(1); j++) {
            for (int i = 0; i < tiles.GetLength(0); i++) {

                if (j!= tiles.GetLength(1)-1 && tiles[i, j].Level == tiles[i, j + 1].Level)
                    return false;
                if (i!= tiles.GetLength(0)-1 && tiles[i, j].Level == tiles[i + 1, j].Level)
                    return false;
            }
        }
        return true;
    }

    private bool TryMove(Vector2 vector2)   
    {
        Debug.Log("Moving [" + vector2.x + "," + vector2.y + "]");
        Vector2Int move = new Vector2Int(Mathf.RoundToInt(vector2.x),-Mathf.RoundToInt(vector2.y));

        // Need to handle the last row first, then the next etc independent on direction. But combined boxes should not combine again during this motion
        Debug.Log("Y = "+move.y);
        // Default Directions
        int rowStartIndex = 0;
        int rowEndIndex = tiles.GetLength(1);
        int rowStep = 1;
        int colStartIndex = 0;
        int colEndIndex = tiles.GetLength(0);
        int colStep = 1;
        if(move.x == 1) {
            rowStartIndex = tiles.GetLength(0)-1;
            rowEndIndex = -1;
            rowStep = -1;
        }
        if(move.y == 1) {
            colStartIndex = tiles.GetLength(1)-1;
            colEndIndex = -1;
            colStep = -1;
        }

        bool change = false;
        // Only handle down?
        Debug.Log("Handle all tiles checking ");
        for (int j = colStartIndex; j != colEndIndex; j += colStep) {
            for (int i = rowStartIndex; i != rowEndIndex; i+=rowStep) {
                Debug.Log("Handle all tiles checking ["+i+","+j+"] " + (tiles[i, j] != null ? tiles[i,j].Value:""));
                // If there is a piece here move it in the direction until it hits another box, if same number merge else stop.
                // Save this info for the tile?
                // tiles have ref to all boxes, will be updated directly = final position
                // then each tile makes its move to reach this position, might end with destroying itself it merging, merged piece changes values? other way around?
                if(tiles[i, j] != null) {
                    // This is a Tile
                    MergeTile tile = tiles[i, j];
                    Vector2Int startPos = new Vector2Int(i,j);
                    Vector2Int endPos = startPos;

                    Vector2Int nextPos = startPos+move;

                    // Step until next step is outside or another tile
                    while (Inside(nextPos.x, nextPos.y) && tiles[nextPos.x,nextPos.y]==null) {
                        endPos = nextPos;
                        nextPos += move;
                    }
                    
                    if (Inside(nextPos.x, nextPos.y) && tiles[nextPos.x,nextPos.y].Level == tile.Level && !tiles[nextPos.x, nextPos.y].MergedThisFrame) { // Next piece is same level = merge
                        
                        // Merging - Currently destroying the tile when next to the merge spot
                        int newMergeValue = tiles[nextPos.x, nextPos.y].ChangeAnimationToMerge();

                        // Update best Level if better
                        if(newMergeValue > bestLevel) {
                            bestLevel = newMergeValue;
                            UpdateGameValueText();
                        }

                        // Mark the moved tile to be deleted
                        tile.SetAnimation(tilePositions[endPos.x,endPos.y].transform.localPosition,AnimationChangeType.Delete);

                        // Remove the moved tiles start position
                        tiles[startPos.x, startPos.y] = null;

                        change = true;
                    }
                    else {
                        // Either at the border or at a non merge                    
                        Debug.Log("Piece stopped at [" + endPos.x + ", " + endPos.y + "] ");
                        tile.SetAnimation(tilePositions[endPos.x, endPos.y].transform.localPosition, AnimationChangeType.Stop);

                        if (startPos == endPos)
                            continue;

                        // Place the tile here
                        tiles[startPos.x, startPos.y] = null;
                        tiles[endPos.x, endPos.y] = tile;

                        change = true;
                    }
                }

            }
        }
        return change;
    }

    private bool Inside(int i, int j) => i >= 0 && j >= 0 && i < tiles.GetLength(0) && j < tiles.GetLength(1);

    private void PlaceTile()
    {


        Debug.Log("Placing a tile");
        PlaceNewTileAt(new Vector2Int(0,0),1);
        PlaceNewTileAt(new Vector2Int(2,3),2);
        PlaceNewTileAt(new Vector2Int(1,3),1);
    }

    private void PlaceNewTileAt(Vector2Int pos, int level)
    {
        // Place a tile
        MergeTile tile = Instantiate(tilePrefab, tileHolder.transform);
        tile.transform.localPosition = tilePositions[pos.x, pos.y].transform.localPosition;
        tile.SetLevel(level);

        tiles[pos.x, pos.y] = tile;
    }

    private void CreateTileSpots()
    {
        tilePositions = new GameObject[4, 4];
        tiles = new MergeTile[4, 4];

        float tileSize = tilePosPrefab.GetComponent<RectTransform>().sizeDelta.x;

        for (int j = 0; j < tilePositions.GetLength(1); j++) {
            for (int i = 0; i < tilePositions.GetLength(0); i++) {
                Debug.Log("Spot is: [" + i + "," + j + "]");
                tilePositions[i,j] = Instantiate(tilePosPrefab,tilePosHolder.transform);
                tilePositions[i, j].transform.localPosition = new Vector2(tileSize/2 +tileSize * i ,-tileSize / 2 - tileSize * j);
            }
        }
    }

}
