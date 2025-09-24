using System.Collections.Generic;
using UnityEngine;

public enum MineBoxType {Unflagged,Open,Flagged,Busted,WrongFlag,Mine,HiddenMine,NotUsed}
[CreateAssetMenu(fileName = "X_Theme",menuName = "NumberTheme")]
public class NumberThemeSO : ScriptableObject
{
    public List<Sprite> flags = new List<Sprite>();
    public List<Sprite> numbers = new List<Sprite>();
}
