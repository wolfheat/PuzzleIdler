using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class BlockPuzzleProblemDatas : MonoBehaviour
{

    //[SerializeField] private PuzzleDatabase blocksPuzzleDatabase;
    [SerializeField] private string easyLevel;
    [SerializeField] private string easyLevelB;
    [SerializeField] private string easyLevelC;
    [SerializeField] private string[] easyLevels;

    public static BlockPuzzleProblemDatas Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public (bool[,], int[]) LoadEasyA() => GetSpecificProblem(easyLevel);
    public (bool[,], int[]) LoadEasyB() => GetSpecificProblem(easyLevelB);
    public (bool[,] , int[]) GetSpecificProblem(string problem)
    {
        Debug.Log("Parsing easy level: " + problem);
        (bool[,] gameArea, int[] pieces) = ParseBlocksPuzzleString(problem);

        //PrintLevel(gameArea);

        return (gameArea, pieces);
    }
    
    public (bool[,] , int[]) GetRandomProblem(int rating = 1000)
    {
        // Player rating can be 1000-2999 ???
        // Problem rating level can be 0 - 9

        int section = (rating - 1000) / 200;

        // 1000 => (0) / 200
        // 3000 => (2000) / 200 = 10
        bool problemFound = false;

        int[,] data = new int[16,16];

        int tries = 0;
        

        //int randomProblem = UnityEngine.Random.Range(0, blocksPuzzleDatabase.data[section].values.Count);

        //Debug.Log("Parsing easy level: "+easyLevel);
        (bool[,] gameArea, int[] pieces) = ParseBlocksPuzzleString(easyLevel);

        //PrintLevel(gameArea);

        return (gameArea, pieces);
    }

    private void PrintLevel(bool[,] gameArea)
    {
        Debug.Log("Printing Loaded Level: ");
        for (int j = 0; j < gameArea.GetLength(1); j++) {
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < gameArea.GetLength(0); i++) {
                sb.Append((gameArea[j,i]?1:0)+",");
            }
            sb.Append("]\n");
            Debug.Log(sb.ToString());
        }
    }

    private (bool[,] gameArea, int[] pieces) ParseBlocksPuzzleString(string level)
    {
        // Use the level to create the gamearea and pieces
        string[] parts = level.Split(',');

        // Pieces
        int[] pieces = new int[7];
        for (int i = 0; i < parts[0].Length; i++) {
            if (parts[0][i] == '.') continue;
            pieces[i] = parts[0][i] - 'A' + 1;
        }

        bool[,] gameArea = new bool[16,16];
        int index = 0;
        // GameArea
        foreach (char occupyAmtChar in parts[1]) {
            bool isOccupied = char.IsUpper(occupyAmtChar);
            int amtChars = occupyAmtChar - ( isOccupied ? 'A' : 'a')+1;
            //Debug.Log("Parsing char: "+ occupyAmtChar+" = "+amtChars);
            for (int i = 0; i < amtChars; i++) {
                int col = index % 8;
                int row = index / 8;
                gameArea[col,row] = isOccupied;
                index++;
            }
        }

        return (gameArea,pieces);
    }

    private bool TryParseStringToLevelData(string problem, out int[,] data)
    {
        char[] chars = problem.ToCharArray();
        data = new int[9,9];

        if(chars.Length != 81) 
            return false;

        for (int i = 0; i < chars.Length; i++) {
            char c = chars[i];
            int val = 0;
            if (c != '.') {
                val = c - '0';
            }
            int col = i / 9;
            int row = i % 9;
            data[col, row] = val;
        }
        return true;
    }

    internal (int[,] level, int diff) GetEasyLevel()
    {
        TryParseStringToLevelData(easyLevel, out int[,] data);
        return (data, 0);
    }

    internal (bool[,] gameAreaLoaded, int[] piecesLoaded) GetRandomEasyLevel()
    {
        int index = Random.Range(0, easyLevels.Length);
        return GetSpecificProblem(easyLevels[index]);
    }

    internal (bool[,] level, int[] pieces) GenerateRandomLevel(int difficulty)
    {
        // Generating A Level
        bool[,] level = new bool[16, 16]; //BlocksPuzzle.GetEmptyLevelBoard();
        int[] pieces = new int[7]; //BlocksPuzzle.GetEmptyLevelBoard();

        int placed = 0;


        while (placed < difficulty) {

            // Place one piece
            int randomType = Random.Range(0, 7);

            // Get a random piece type
            Debug.Log(" Placing piece of type " + randomType);
            TetrisPiece piece = GetRandomPiece(randomType);

            // Place first piece in the center


            List<(Vector2Int, int, int)> valids = new List<(Vector2Int, int, int)>();
            for (int rot = 0; rot < piece.occupySpots.Count; rot++) {
                List<Vector2Int> blockPositions = piece.occupySpots[rot];
                // Get all valid placements        


                for (int j = 0; j < level.GetLength(1); j++) {
                    for (int i = 0; i < level.GetLength(0); i++) {
                        if (placed == 0 && i != 5 && j != 5) continue; // Limits first piece to only be placed at 4,4
                        Vector2Int placePos = new Vector2Int(j, i);
                        Vector2Int[] boxPositions = blockPositions.Select(x => x + placePos).ToArray();
                        (bool valid, bool outside, int neigbors) = ValidatePlacementPosition(boxPositions);
                        if (valid) {
                            // this is a possible placement
                            valids.Add((placePos, rot, neigbors));
                        }
                    }
                }
                if(valids.Count == 0)
                    break;

            }
            // Order the placements
            valids = valids.OrderByDescending(x => x.Item3).ToList();

            // Get a random of the top 5 placements
            int randomPlacement = Random.Range(0, Mathf.Min(1, valids.Count));

            (Vector2Int finalPlacement, int finalRot, int finalneighbors)  = valids[randomPlacement];

            Debug.Log("AMT " + valids.Count + " positions");
            Debug.Log("The piece " + piece.Type + " can be placed on " + valids.Count + " positions, first valid is [" + finalPlacement.x + "," + finalPlacement.y + "] with rotation " + finalRot);

            // Add this piece to the result
            pieces[(int)piece.Type - 2]++;

            // Place on placement with most neighbors at random
            // Place it in the level
            Vector2Int[] boxPositionsFinal = piece.occupySpots[finalRot].Select(x => x + finalPlacement).ToArray();
            OccupySpots(boxPositionsFinal);

            placed++;
        }

        return (level, pieces);

        // Local Methods        
        void OccupySpots(Vector2Int[] indexPositions)
        {
            if (indexPositions == null) return;

            foreach (var pos in indexPositions) {
                level[pos.y, pos.x] = true;
            }
        }

        (bool valid, bool outside, int neigbors) ValidatePlacementPosition(Vector2Int[] indexPositions)
        {
            int neighbors = 0;
            //Debug.Log("Validating box positions");
            foreach (var pos in indexPositions) {
                if (!InsideBoard(pos.x, pos.y)) {
                    //Debug.Log("Blocks: Piece Outside [" + pos.x + "," + pos.y + "]");
                    return (false, true,0);
                }

                if (level[pos.y, pos.x]) {
                    //Debug.Log("Blocks: Piece positions is not empty [" + pos.x + "," + pos.y + "]");
                    return (false, false,0);
                }

                // Add a neighbor if available
                foreach (Vector2Int neighbor in neighborDir) {
                    Vector2Int neighborPos = neighbor + pos;
                    if (InsideBoard(neighborPos.x,neighborPos.y) && level[neighborPos.x,neighborPos.y]) {
                        neighbors++;
                    }
                }
            }
            //Debug.Log("Blocks: Piece is valid placed ");    
            return (true, false, neighbors);
        }

        bool InsideBoard(int col, int row) => col >= 0 && row >= 0 && col < 8 && row < 8;

    }

    private Vector2Int[] neighborDir = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };


    private static TetrisPiece GetRandomPiece(int randomType)
    {

        // Make the piece
        return randomType switch
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
    }
}
