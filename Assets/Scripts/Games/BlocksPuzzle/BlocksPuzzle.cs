using System;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BlocksPuzzle : MonoBehaviour
{
    public const int BlockSize = 30;
    private const int GameSize = 8;
    public const int BlockScale = 1;

    int[,] board = new int[GameSize, GameSize];
    TetrisBlock[,] boardBlocks = new TetrisBlock[GameSize,GameSize];

    [SerializeField] private GameObject boxHolder;
    [SerializeField] private TetrisBlock boxPrefab;

    public static BlocksPuzzle Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RestartGame();
    }

    public void RestartGame(bool resetPosition = false)
    {
        Debug.Log("BlocksPuzzle: Restart Game");

        ResetBoxes();
    }

    private void ResetBoxes()
    {
        Debug.Log("BlocksPuzzle: Creating Boxes");
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(1); j++) {
                board[j, i] = 1;
                boardBlocks[j, i] = Instantiate(boxPrefab, boxHolder.transform);
                boardBlocks[j, i].SetType(1);
                boardBlocks[j, i].transform.localScale = new Vector3(BlockScale, BlockScale);
                boardBlocks[j, i].transform.localPosition = new Vector3(i*BlockSize* BlockScale, -j*BlockSize* BlockScale);
            }
        }
    }

    private bool InsideBoard(int col, int row) => col >= 0 && row >= 0 && col < GameSize && row < GameSize;

    internal bool TryPlacePiece(MovablePiece activePiece)
    {
        // check for out of bounds here 

        // Figure out delta From mouse to the placement

        //Debug.Log("Trying to place piece "+activePiece+ " at position [" + endPosIndex.x + "," + endPosIndex.y + "]");

        // Need to read the tiles parts to see if any of them collide with any piece on the board
        // Need to know the main box in the piece and its position and use its rotation to know what boxes that will be placed

        TetrisBlock[] blocks = activePiece.GetAllTetrisBlocks;

        foreach (var block in blocks) {
            // Debug.Log("Piece has a box offset at ["+block.transform.localPosition.x+","+block.transform.localPosition.y+"]");
        }
        // Transpose this to the game area?

        Vector2 pieceLocalPosition = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>()) + PiecesHandler.Instance.Offset;

        Vector2 mousePieceHolderPosition = WolfheatProductions.Converter.GetMouseLocalPosition(activePiece.transform.parent.GetComponent<RectTransform>());

        Vector2[] positions = blocks.Select(x => new Vector2(x.transform.localPosition.x, x.transform.localPosition.y)).ToArray();
        //Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(Mathf.RoundToInt(x.transform.position.x / (BlockSize * BlockScale)), Mathf.RoundToInt(x.transform.position.y / (BlockSize * BlockScale)))).ToArray();

        foreach (var block in blocks) {
            //Debug.Log("Piece box actually at [" + (mouseLocalPosition.x + block.transform.localPosition.x) + "," + (mouseLocalPosition.y + block.transform.localPosition.y ) + "]");
        }

        Vector2Int[] indexPositions = blocks.Select(x => new Vector2Int(
             Mathf.FloorToInt((x.transform.localPosition.x + pieceLocalPosition.x) / (BlockSize * BlockScale)),
            -Mathf.FloorToInt((x.transform.localPosition.y + pieceLocalPosition.y) / (BlockSize * BlockScale)) - 1)).ToArray();


        Vector2[] worldPositions = blocks.Select(block =>
    pieceLocalPosition + (Vector2)block.transform.localPosition
).ToArray();

        Vector2Int[] gridIndices = worldPositions.Select(pos => new Vector2Int(
    Mathf.FloorToInt(pos.x / (BlockSize * BlockScale)),
    -Mathf.FloorToInt(pos.y / (BlockSize * BlockScale)) - 1
)).ToArray();

        Vector2 snappedPosition = new Vector2(gridIndices[0].x * (BlockSize * BlockScale), -gridIndices[0].y * (BlockSize * BlockScale));

        Vector2 localPiecePos = (Vector2)blocks[0].transform.localPosition;
        Vector2 delta = snappedPosition - (pieceLocalPosition + localPiecePos) + new Vector2(0.5f, -0.5f) * BlockSize * BlockScale;
        //Vector2 delta = snappedPosition - (mouseLocalPosition + localPiecePos) + new Vector2(0.5f,-0.5f);


        //Vector2 delta = new Vector2(indexPositions[0].x * (BlockSize * BlockScale) - positions[0].x, indexPositions[0].y * (BlockSize * BlockScale) - positions[0].y);

        //Vector2 placePos = mousePieceHolderPosition;
        Vector2 placePos = mousePieceHolderPosition + delta;


        // Now check these positions
        (bool valid, bool outside) = ValidatePlacementPosition(indexPositions);

        if(valid)
            PlacePieceOnValidSpot(activePiece, indexPositions, placePos);
        else {
            // Replace on old valid spot if there is one

            if (outside) {
                // Outside - Return Home
                activePiece.ReturnHome();
                return false;
            }

            // Inside return to previously placement inside
            OccupySpots(activePiece,activePiece.OccupySpots);
            return false;
            
        }
        return valid;
    }

    private (bool valid, bool outside) ValidatePlacementPosition(Vector2Int[] indexPositions)
    {
        foreach (var pos in indexPositions) {
            if (!InsideBoard(pos.x, pos.y)) {
                Debug.Log("Blocks: Piece Outside [" + pos.x + "," + pos.y + "]");
                return (false,true);
            }

            if (board[pos.y, pos.x] != 1) {
                Debug.Log("Blocks: Piece not Empty [" + pos.x + "," + pos.y + "]");
                return (false, false);
            }
            Debug.Log("Blocks: Valid placement board[" + pos.x + "," + pos.y + "] = " + board[pos.y, pos.x]);
        }

        return (true, false);
    }

    private void PlacePieceOnValidSpot(MovablePiece activePiece, Vector2Int[] indexPositions, Vector2 placePos)
    {
        // Occupy board
        OccupySpots(activePiece, indexPositions);

        // Save the board positions to the piece
        activePiece.SetOccupySpots(indexPositions);

        // Also move the original piece here and show it
        activePiece.gameObject.SetActive(true);
        activePiece.transform.localPosition = placePos;
    }

    private void OccupySpots(MovablePiece activePiece, Vector2Int[] indexPositions)
    {
        if (indexPositions == null) return;

        foreach (var pos in indexPositions) {
            board[pos.y, pos.x] = (int)activePiece.Type;
            boardBlocks[pos.y, pos.x].SetType(board[pos.y, pos.x]);
            Debug.Log("Blocks:  Piece [" + pos.x + "," + pos.y + "] => " + board[pos.y, pos.x]);
        }
    }

    internal void ClearBoardSpots(MovablePiece activePiece)
    {
        Vector2Int[] occypySpots = activePiece.OccupySpots;

        if(occypySpots == null || occypySpots.Length == 0) return;

        Debug.Log("Clearing all occypySpots "+occypySpots?.Length);

        if (occypySpots != null) {
            foreach (Vector2Int pos in occypySpots) {
                board[pos.y, pos.x] = (int)TetrisBlockType.Fixed;
                boardBlocks[pos.y, pos.x].SetType(board[pos.y, pos.x]);

                Debug.Log("Blocks: Unsetting [" + pos.x + "," + pos.y + "] = " + board[pos.y, pos.x]);
            }
        }
    }
}
