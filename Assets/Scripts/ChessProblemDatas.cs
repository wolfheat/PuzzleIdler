using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ChessProblemDatas : MonoBehaviour
{

    [SerializeField] private PuzzleDatabase database;
    [SerializeField] private string promotionExampleProblem;
    [SerializeField] private string blackPromotion;
    [SerializeField] private string computerPromotion;
    [SerializeField] private string computerBlackPromotion;

    [SerializeField] private string playerCastle;
    [SerializeField] private string playerCastleLong;
    [SerializeField] private string computerCastle;

    [SerializeField] private string computerEnPassent;
    [SerializeField] private string playerEnPassent;
    [SerializeField] private string multipleEndMoves;
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


    public ChessPuzzleData GetSpecificProblem(int problemType = 0)
    {
        string problem = problemType switch {
            1 => promotionExampleProblem,
            2 => blackPromotion,
            3 => computerPromotion,
            4 => computerBlackPromotion,
            5 => playerCastle,
            6 => playerCastleLong,
            7 => computerCastle,
            8 => playerEnPassent,
            9 => computerEnPassent,
            10 => multipleEndMoves,
            _ => ""
        };

        return GetStringAsPuzzleData(problem);
    }

    public ChessPuzzleData GetPromotionProblem(int rating = 1000)
    {
        int section = rating / 100;

        // string selectedLevel = promotionExampleProblem;
        // string selectedLevel = blackPromotion;
         //string selectedLevel = computerPromotion;
         string selectedLevel = computerBlackPromotion;

        // Here we have the compact level as one string
        ChessPuzzleData data = GetStringAsPuzzleData(selectedLevel);

        return data;
    }
        
    public ChessPuzzleData GetRandomProblem(int rating = 1000)
    {
        int section = rating / 100;


        // Section 3-30

        // Make sure it is within the range of lists
        section = Math.Clamp(section, 3, 30);


        //section = 14;

        //Debug.Log("X");
        //Debug.Log("database.data " + database.data);
        //Debug.Log("database.data.CountLength " + database.data.Count);
        //Debug.Log("database.data[section] " + database.data[section]);
        //Debug.Log("database.data[section].Count " + database.data[section].values.Count);

        // Finds next available problem with rollover
        while(database.data[section].values.Count == 0) {
            section++;

            // If reaching end of list - go backwards
            if (section == database.data.Count) {
                section--;
                while (database.data[section].values.Count == 0) {
                    section--;
                }
            }
        }
        // Should have correct list here



        string selectedLevel = database?.data[section].values[UnityEngine.Random.Range(0, database.data[section].values.Count)];
        //Debug.Log("Selected random level from section: "+section+" that consists of "+ database.data[section].values.Count+" levels.");

        Debug.Log("LOADING LEVEL: "+selectedLevel);

        // Here we have the compact level as one string
        ChessPuzzleData data = GetStringAsPuzzleData(selectedLevel);

        Debug.Log("PLayer Rating: "+rating+" Problem: "+data.rating);

        return data;
    }

    private ChessPuzzleData GetStringAsPuzzleData(string selectedLevel)
    {
        string[] levelParts = selectedLevel.Split(',');

        ChessPuzzleData data = new ChessPuzzleData();

        data.name = levelParts[0];

        data.rating = Int32.Parse(levelParts[3]);

        data.setup = GetSetup(levelParts[1]);

        data.solution = GetSolution(levelParts[2], data.setup[64] == 0);
        //Debug.Log("Solution: "+ levelParts[2]+" => " + data.solution[0]+ data.solution[1]+ data.solution[2]+ data.solution[3]);
        
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

        if(ans[index] == 0) {

            int[] copy = new int[65];
            copy[64] = ans[64];

            // Player plays black Rotate all
            for (int row = 0; row < 8; row++) {
                for (int col = 0; col < 8; col++) {
                    copy[row*8+col] = ans[(7-row)*8+(7-col)];
                }
            }
            ans = copy;
        }

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
            'Q' => 3,
            'K' => 4,
            'P' => 5,
            'r' => 6,
            'n' => 7,
            'b' => 8,
            'q' => 9,
            'k' => 10,
            'p' => 11,
            _ => 12
        };
    }

    private int[] GetSolution(string v, bool flip)
    {
        // Parse the solution int correct values
        string[] moves = v.Split(" ");

        // Using only first step
        // Got a move in first spot that looks like d4e6
        //Debug.Log("Chars = " + moves[0]+" full solution  = "+v);
        List<int> ans = new();

        int index = 0;

        foreach(string move in moves) {
            char[] chars = move.ToCharArray();
            for (int i = 0; i < 4; i++) {
                char c = chars[i];
                int asInt = i%2==0 ? c - 'a' : (c-'1');
                //Debug.Log("ans["+index+"] = "+asInt+" flip = "+flip);

                ans.Add(flip ? (7-asInt) : asInt);
                index++;
            }

            if(chars.Length >= 5) {
                ans.Add(PromotionIndexConverter(chars[4]));
                //Debug.Log("<color=red>Added promotion: </color>"+ ans[ans.Count-1]);
                //Debug.Log("Full solution  = "+v);
            }
        }
        return ans.ToArray();
    }

    private int PromotionIndexConverter(char v)
    {
        return v switch
        {
            'r' => 8,
            'n' => 9,
            'b' => 10,
            'q' => 11,
            _ => 0
            /*
            'r' => 12,
            'n' => 13,
            'b' => 14,
            'q' => 15,
            */
        };
    }

    internal void FindEnPassentProblem()
    {
        Debug.Log("Finding En Passent Problem");

        int index = 0;

        int section = 14;


        for (int k = 9; k < 20; k++) {

            int totAmt = database.data[section].values.Count;
            for (int i = 0; i < totAmt; i++) {

                string selectedLevel = database?.data[section].values[i];
                
                string[] parts = selectedLevel.Split(',');
            
                string[] setup = parts[1].Split(' ');

                //Debug.Log("Checkingif 'b': " + setup[1]);
                
                index++;

                bool computerBlack = setup[1] == "b";
                if (!computerBlack) continue;


                //ChessPuzzleData data = GetStringAsPuzzleData(selectedLevel);


                string[] solution = parts[2].Split(" ");

                // Find an enemy move that is e7e5 - where player has a pawn on f5

                for (int j = 1; j < solution.Length; j+=2) {
                    string part = solution[j];
                    if (solution[j-1] == "e7e5" &&  (solution[j] == "f5e6" || solution[j] == "d5e6")){
                        Debug.Log(""+selectedLevel);
                        break;
                    }
                }
            }
            section++;
        }
        Debug.Log("END");
        // Found promotion problem show it
    }
    internal void FindPromotionProblem()
    {
        Debug.Log("Finding Promotion Problem");

        int index = 0;

        int section = 14;


        for (int k = 10; k < 12; k++) {

            int totAmt = database.data[section].values.Count;
            for (int i = 0; i < totAmt; i++) {
                string selectedLevel = database?.data[section].values[index];
                //Debug.Log("["+section+","+index+"] "+"Level: "+selectedLevel);

                string[] parts = selectedLevel.Split(',');
            
                string[] setup = parts[1].Split(' ');

                //Debug.Log("Checkingif 'b': " + setup[1]);
                
                index++;

                bool computerBlack = setup[1] == "b";
                if (!computerBlack) continue;


                string[] solution = parts[2].Split(" ");

                for (int j = 0; j < solution.Length; j+=2) {
                    string part = solution[j];
                    if (part.Length == 5) {
                        Debug.Log(""+selectedLevel);
                        break;
                    }
                }
            }
            section++;
        }
        Debug.Log("END");
        // Found promotion problem show it
    }
    internal void FindCastleProblem()
    {
        Debug.Log("Finding Castle Problem");

        int section = 11;
        
        while(section < 28) {

            int totAmt = database.data[section].values.Count;
            for (int i = 0; i < totAmt; i++) {

                string selectedLevel = database?.data[section].values[i];
                
                string[] parts = selectedLevel.Split(',');
            
                i++;

                string[] solution = parts[2].Split(" ");

                for (int j = 1; j < solution.Length; j+=2) {
                    string part = solution[j];
                    if (part == "e1c1") {
                        Debug.Log(""+selectedLevel);
                        break;
                    }
                }
            }
            section++;
        
        }
        Debug.Log("END");
        // Found promotion problem show it
    }
}
