using System;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class SudokuProblemDatas : MonoBehaviour
{

    [SerializeField] private PuzzleDatabase sudokuPuzzleDatabase;


    public static SudokuProblemDatas Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public int[,] GetRandomProblem(int rating = 1000)
    {
        // Player rating can be 1000-2999 ???
        // Problem rating level can be 0 - 9


        int section = (rating - 1000) / 200;

        // 1000 => (0) / 200
        // 3000 => (2000) / 200 = 10
        bool problemFound = false;

        int[,] data = new int[9,9];


        if(sudokuPuzzleDatabase.data[section].values.Count == 0) {
            Debug.Log("Section is Empty - ");
            return data; // return empty level
        }

        int tries = 0;

        // run until a problem is found or 100 fails
        while (!problemFound && tries < 100) {
            tries++;
            int randomProblem = UnityEngine.Random.Range(0, sudokuPuzzleDatabase.data[section].values.Count);

            string problem = sudokuPuzzleDatabase.data[section].values[randomProblem];

            // Create a problem from this string
            if(TryParseStringToLevelData(problem, out data))
                problemFound = true;
        }
        
        return data;
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
}
