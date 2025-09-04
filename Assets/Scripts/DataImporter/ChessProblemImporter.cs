using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEditor;
using UnityEngine;

public class ChessProblemImporter :EditorWindow
{
    private const string CSV_PATH = "Assets/Data/ChessProblems.csv";
    private const string SCRIPTABLE_OBJECT_FOLDER = "Assets/Data/ChessProblems/";

    [MenuItem("Tools/Import ChessProblems.csv")]
    public static void ImportChessProblems()
    {
        Debug.Log("Updating Chess Problems.");
        if (!File.Exists(CSV_PATH)) {
            Debug.LogError("CSV file not found at: " + CSV_PATH);
            return;
        }

        if (!Directory.Exists(SCRIPTABLE_OBJECT_FOLDER)) {
            Directory.CreateDirectory(SCRIPTABLE_OBJECT_FOLDER);
            AssetDatabase.Refresh();
        }

        //string[] lines = File.ReadAllLines(CSV_PATH);
        string[] lines = ReadCsvWithSharedAccess(CSV_PATH);

        // Separate them into correct difficulty here?

        // 0-100 max 4000 steps 100 = 40

        // Initiate Lists
        List<StringList> separatedLists = new List<StringList>();
        for (int i = 0; i < 40; i++)
            separatedLists.Add(new StringList());

        foreach (string line in lines) {

            string[] values = line.Split(',');

            // Use the rating to define section
            int section = Int32.Parse(values[3]) / 100;

            // Add thi line to correct section
            separatedLists[section].values.Add(line);
        }
        CreatePuzzleDatabaseFromCSV(separatedLists);

        /*

        int updatedItems = 0;
        for (int i = 1; i < lines.Length; i++) // Skip header row
        {
            Debug.Log("Line " + i + ": " + lines[i]);
            string[] values = lines[i].Split(';');
            if (values.Length < 2) continue; // Ensure we have at least itemName & LatinName
            if (values[0].Length == 0) continue;

            // DATA LOOKS LIKE THIS
            //      0	          1           2         3        4        5       6        7      8        9     10        11
            // Latin Name	Swedish Name	Info	Vanlighet	Root	Stem	Leafs	Flower	Seeds	Berries	Avoid	Förvildad

            string latinName = values[0].Trim();
            string itemName = values[1].Trim();
            string info = FinalizeInfo(values[2]);
            int commonness = Int32.Parse(values[3]);
            Debug.Log("parsing commonness [" + latinName + "]: " + commonness);
            // Edibles 4-9
            int[] edible = new int[9];
            for (int j = 4; j < 9 + 4; j++) {
                int index = j - 4;
                //Debug.Log("parsing:" + values[j]);
                int value = (j >= values.Length || values[j].Length == 0) ? 0 : Int32.Parse(values[j]);
                edible[index] = value;
            }
            bool feral = values[13] != "";
            bool protectedPlant = values[14] != "";

            int treeBush = values[15].Length > 0 ? Int32.Parse(values[15]) : 0;
            string medicinal = values[16];
            int army = values[17].Length > 0 ? Int32.Parse(values[17]) : 0;
            int allergenic = values[18].Length > 0 ? Int32.Parse(values[18]) : 0;

            // Find all matching images
            Sprite[] sprites = FindSpritesForPlant(latinName);

            // Save the asset
            string assetPath = SCRIPTABLE_OBJECT_FOLDER + latinName + ".asset";

            if (sprites.Length == 0) {
                Debug.LogWarning($"No images found for {latinName}");

                DeletePlantDataIfAvailable(assetPath);
                continue;
            }

            // Create or update ScriptableObject
            QuestionData plantData = CreateOrUpdatePlantData(itemName, latinName, info, commonness, sprites, edible, feral, protectedPlant, treeBush, medicinal, army, allergenic);
            updatedItems++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Updating Plant Data from Plants.cvs: COMPLETE [" + updatedItems + "]");

        */
    }


#if UNITY_EDITOR
    public static void CreatePuzzleDatabaseFromCSV(List<StringList> data)
    {
        // If player entered a name use that, else take random
        
        var puzzleDatabase = ScriptableObject.CreateInstance<PuzzleDatabase>();
        Debug.Log("Adding data for puzzles with sections: " + data.Count);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 40; i++) {
            sb.Append("," + data[i].values.Count);
        }
        Debug.Log("Sections: " + sb.ToString());

        puzzleDatabase.data = data;

        AssetDatabase.CreateAsset(puzzleDatabase, "Assets/ScriptableObjects/Games/Chess/PuzzleDatabase.asset");
        AssetDatabase.SaveAssets();
    }
#endif
    
    /*
#if UNITY_EDITOR
    public static void CreatePuzzleAssetFromCSV(List<string>[] data)
    {
        // If player entered a name use that, else take random
        string name = data[0];
        int rating = Int32.Parse(data[3]);
        string setup = data[1];
        string solution = data[2];

        var puzzle = ScriptableObject.CreateInstance<ChessPuzzleStringData>();
        puzzle.rating = rating;
        puzzle.setup = setup;
        puzzle.solution = solution;

        Debug.Log("GENERATED ASSET: " + name+" Rating: "+rating);

        AssetDatabase.CreateAsset(puzzle, "Assets/ScriptableObjects/Games/Chess/" + name + ".asset");
        AssetDatabase.SaveAssets();
    }
#endif
    */

    private static string FinalizeInfo(string info)
    {
        // Removet any brackets section
        string removedBrackets = RemoveBrackets(info);

        // remove any other unwanted stuff...


        return removedBrackets;
    }

    private static string RemoveBrackets(string info)
    {
        int brackets = 0;

        char[] chars = info.ToCharArray();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < chars.Length; i++) {
            char c = chars[i];
            if (c == '[') {
                brackets++;
                continue;
            }
            else if (c == ']') {
                brackets--;
                continue;
            }
            else if (brackets == 0) {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }



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

    /*
    private static Sprite[] FindSpritesForPlant(string latinName)
    {
        List<Sprite> spriteList = new List<Sprite>();

        string[] files = Directory.GetFiles(IMAGE_FOLDER, latinName + "*.*", SearchOption.TopDirectoryOnly);

        foreach (string file in files) {
            if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg")) {

                //string assetPath = Application.dataPath +"Assets/";
                string assetPath = file.Replace(Application.dataPath, "").Replace("\\", "/");
                //Debug.Log("Asset Path: "+assetPath);

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null) {
                    //Debug.Log("Loaded sprite: "+latinName+" at "+assetPath);
                    spriteList.Add(sprite);
                }
                else
                    Debug.LogWarning("Could not load sprite: " + latinName + " at " + assetPath);
            }
        }

        return spriteList.ToArray();
    }

    private static void DeletePlantDataIfAvailable(string assetPath)
    {
        if (AssetDatabase.AssetPathExists(assetPath)) {
            AssetDatabase.DeleteAsset(assetPath);
        }
    }
    
    private static QuestionData CreateOrUpdatePlantData(string itemName, string latinName, string info, int commonness, Sprite[] sprites, int[] edible, bool feral, bool protectedPlant, int treeBush, string medicinal, int army, int allergenic)
    {
        //Debug.Log("Create Or Update Plant Data: " + latinName);

        string assetPath = SCRIPTABLE_OBJECT_FOLDER + latinName + ".asset";
        QuestionData plantData = AssetDatabase.LoadAssetAtPath<QuestionData>(assetPath);

        bool dataExists = plantData != null;

        if (!dataExists) {
            plantData = ScriptableObject.CreateInstance<QuestionData>();
        }

        plantData.ItemName = itemName;
        plantData.LatinName = latinName;
        plantData.info = info;
        plantData.sprites = sprites;
        plantData.root = edible[0];
        plantData.stem = edible[1];
        plantData.leaf = edible[2];
        plantData.flower = edible[3];
        plantData.seed = edible[4];
        plantData.fruit = edible[5];
        plantData.sap = edible[6];
        plantData.fungi = edible[7];
        plantData.avoid = edible[8];
        plantData.treeBush = treeBush;
        plantData.medicinal = medicinal;
        plantData.army = army;
        plantData.allergenic = allergenic;

        plantData.commonness = commonness;
        plantData.feral = feral;
        plantData.protectedPlant = protectedPlant;

        plantData.CreateCategories();

        if (!dataExists)
            AssetDatabase.CreateAsset(plantData, assetPath);

        // Make sure the changes are stored
        EditorUtility.SetDirty(plantData);

        return plantData;
    }
    */
}
