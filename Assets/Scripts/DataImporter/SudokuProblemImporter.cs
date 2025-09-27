using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SudokuProblemImporter : EditorWindow
{
    private const string CSV_PATH = "Assets/Data/sudoku-3m.csv";
    private const int DiffLevels = 10;
    //private const int MaxProblems = 10;
    private const int MaxProblems = 2000000;
    private const int MaxProblemsPerDiff = 5000;
    //private const string SCRIPTABLE_OBJECT_FOLDER = "Assets/Data/SudokuProblems/";

    [MenuItem("Tools/Import SudokuProblems.csv")]
    public static void ImportChessProblems()
    {
        Debug.Log("Updating Chess Problems.");
        if (!File.Exists(CSV_PATH)) {
            Debug.LogError("CSV file not found at: " + CSV_PATH);
            return;
        }

        string[] lines = ReadCsvWithSharedAccess(CSV_PATH);

        // Separate them into correct difficulty here?

        // 0.0-5.0 gives = 50 different diffs

        //Debug.Log("Loading sudoku problems from SCV file containing"+lines.Length+", but limiting problemd to "+MaxProblems);

        // Initiate Lists
        List<StringList> separatedLists = new List<StringList>();
        for (int i = 0; i < DiffLevels; i++)
            separatedLists.Add(new StringList());

        int problems = -1;
        foreach (string line in lines) {
            // Skips first Header Line
            problems++;
            if (problems == 0)
                continue;

            string[] values = line.Split(',');

            // Use the rating to define section DIFF 0-50 ?
            //Debug.Log("trying to parse " + values[4]+" to double then multiply by 10 to get an int");
            int diff = (int)(double.Parse(values[4], CultureInfo.InvariantCulture) * 1.9999); // 3.5 => 35

            // Skip any harder than diff 10
            if (diff >= 10)
                continue;

            // Skip any additional problem of this type if filled to max
            if(separatedLists[diff].values.Count >= MaxProblemsPerDiff)
                continue;

            // Only save the problem?
            string problem = values[1];

            // Add this line to correct section
            separatedLists[diff].values.Add(problem);
            if (problems > MaxProblems)
                break;
        }
        CreatePuzzleDatabaseFromCSV(separatedLists);

    }


#if UNITY_EDITOR
    public static void CreatePuzzleDatabaseFromCSV(List<StringList> data)
    {
        // If player entered a name use that, else take random
        
        var puzzleDatabase = ScriptableObject.CreateInstance<PuzzleDatabase>();
        
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < DiffLevels; i++) {
            sb.Append("," + data[i].values.Count);
        }
        Debug.Log("Sections: " + sb.ToString());

        puzzleDatabase.data = data;

        AssetDatabase.CreateAsset(puzzleDatabase, "Assets/ScriptableObjects/Games/Sudoku/PuzzleDatabase.asset");
        AssetDatabase.SaveAssets();
    }
#endif
    
    private static string[] ReadCsvWithSharedAccess(string path)
    {
        try {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
                List<string> lines = new List<string>();
                string line;
                while ((line = reader.ReadLine()) != null) {
                    lines.Add(line);
                }
                return lines.ToArray();
            }
        }
        catch (IOException e) {
            Debug.LogError("Error reading CSV file: " + path + "\n" + e.Message);
            return null;
        }
    }

}
