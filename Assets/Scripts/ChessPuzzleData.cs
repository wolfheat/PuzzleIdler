using UnityEngine;

[CreateAssetMenu(fileName = "fileName", menuName = "ScriptableObjects/ChessPuzzle")]
public class ChessPuzzleData : ScriptableObject
{
    public int[] setup = new int[64];
    public int[] solution = new int[4];
    public int rating = 1000;
}
