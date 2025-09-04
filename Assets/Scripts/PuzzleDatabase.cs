using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fileName", menuName = "ScriptableObjects/PuzzleDatabase")]
public class PuzzleDatabase : ScriptableObject
{
    public List<StringList> data;
}

[System.Serializable]
public class StringList
{
    public List<string> values = new List<string>();
}
