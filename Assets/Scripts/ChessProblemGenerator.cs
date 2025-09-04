using System;
using TMPro;
using UnityEditor;
using UnityEngine;
public class ChessProblemGenerator : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField ratingInput;


    public static ChessProblemGenerator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void ClickTest()
    {
        Debug.Log("CLICKED REGISTERED");
    }
    
    /*

    // Runs when in editor and play mode
#if UNITY_EDITOR
    [ContextMenu("Create Puzzle Asset")]
    public void CreatePuzzleAsset()
    {
        // If player entered a name use that, else take random
        string name = "Puzzle" + UnityEngine.Random.Range(0, 100000);
        int diff = 1000;

        if(nameInput.text != "")
            name = nameInput.text;

        
        if (ratingInput.text != "" && Int32.TryParse(ratingInput.text, out diff)) {
            Debug.Log("Rating is Valid: "+diff);
        }

        var puzzle = ScriptableObject.CreateInstance<ChessPuzzleData>();
        puzzle.rating = diff;

        // Read the board
        int[] boardData = Chess.Instance.GetBoardData();
        puzzle.setup = boardData;

        int[] solution = Chess.Instance.GetSolution();
        puzzle.solution = solution;

        Debug.Log("GENERATED ASSET: "+name);

        AssetDatabase.CreateAsset(puzzle, "Assets/ScriptableObjects/Games/Chess/"+name+".asset");
        AssetDatabase.SaveAssets();
    }
#endif
    */
}
