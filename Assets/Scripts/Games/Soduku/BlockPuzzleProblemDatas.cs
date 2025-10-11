using System.Text;
using UnityEngine;

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
}
