using UnityEngine;

[CreateAssetMenu(menuName = "MiniGames/Chess/ChessPuzzle", fileName = "ChessPuzzle")]
public class ChessPuzzleData : ScriptableObject
{
    public int[] setup = new int[64];
    public int[] solution;
    public int rating = 1000;
}
