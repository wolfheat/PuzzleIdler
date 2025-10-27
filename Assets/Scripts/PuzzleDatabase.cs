using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MiniGames/PuzzleDatabase", fileName = "fileName")]
public class PuzzleDatabase : ScriptableObject
{
    public List<StringList> data;
}

[System.Serializable]
public class StringList
{
    public List<string> values = new List<string>();
}
