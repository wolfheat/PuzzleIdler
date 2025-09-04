using System;
using System.Linq;
using UnityEngine;

public class ChessProblemDatas : MonoBehaviour
{

    [SerializeField] private PuzzleDatabase database;
    //[SerializeField] private ChessDatabase chessDatabase;

    public static ChessProblemDatas Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public ChessPuzzleData GetRandomProblem(int rating = 1000)
    {
        int section = rating / 100;

        section = 11;


        Debug.Log("X");
        Debug.Log("database.data "+ database.data);
        Debug.Log("database.data.CountLength "+ database.data.Count);
        Debug.Log("database.data[section] " + database.data[section]);
        Debug.Log("database.data[section].Count " + database.data[section].values.Count);

        string selectedLevel = database.data[section].values[UnityEngine.Random.Range(0, database.data[section].values.Count)];
        Debug.Log("Selected random level from section: "+section+" that consists of "+ database.data[section].values.Count+" levels.");

        // Here we have the compact level as one string
        ChessPuzzleData data = GetStringAsPuzzleData(selectedLevel);

        return data;
    }

    private ChessPuzzleData GetStringAsPuzzleData(string selectedLevel)
    {
        string[] levelParts = selectedLevel.Split(',');

        ChessPuzzleData data = new ChessPuzzleData();

        data.name = levelParts[0];

        data.rating = Int32.Parse(levelParts[3]);

        data.solution = GetSolution(levelParts[2]);
        Debug.Log("Solution: "+ levelParts[2]+" => " + data.solution[0]+ data.solution[1]+ data.solution[2]+ data.solution[3]);

        data.setup = GetSetup(levelParts[1]);

        return data; 
    }

    private int[] GetSetup(string v)
    {
        // Parse the level setup
        string[] moves = v.Split(" ");

        int[] ans = new int[65];  

        // Get the moves
        string[] lines = moves[0].Split("/").Reverse().ToArray();

        int index = 0;

        // if it is a number skip this many steps = set to -1
        foreach (string line in lines) {
            char[] chars = line.ToCharArray();                
            for (int i = 0; i < chars.Length; i++) {
                // Test character here
                if (char.IsDigit(chars[i])) {
                    int skips = chars[i] - '0';
                    for (int j = 0; j < skips; j++) {
                        ans[index] = -1;
                        index++;
                    }
                    continue;
                }

                // Need to be a char here
                int pieceID = GetPieceStringAsIndex(chars[i]);
                ans[index] = pieceID;
                index++;
            }
        }
        
        // Add the turn
        ans[index] = moves[1] == "b" ? 1 : 0;

        if(index != 64)
            Debug.Log("Warning: Index is not 64: "+index);

        return ans;
    }

    private int GetPieceStringAsIndex(char v)
    {
        return v switch
        {
            'R' => 0,
            'N' => 1,
            'B' => 2,
            'K' => 3,
            'Q' => 4,
            'P' => 5,
            'r' => 6,
            'n' => 7,
            'b' => 8,
            'k' => 9,
            'q' => 10,
            'p' => 11,
            _ => 12
        };
    }

    private int[] GetSolution(string v)
    {
        // Parse the solution int correct values
        string[] moves = v.Split(" ");

        // Using only first step
        // Got a move in first spot that looks like d4e6
        Debug.Log("Chars = " + moves[0]);
        int[] ans = new int[moves.Length * 4];
        int index = 0;
        foreach(string move in moves) {
            char[] chars = move.ToCharArray();
            for (int i = 0; i < chars.Length; i++) {
                char c = chars[i];
                int asInt = i%2==0 ? c - 'a' : (c-'0');
                ans[index] = asInt;
                index++;
            }
        }
        return ans;
    }
}
