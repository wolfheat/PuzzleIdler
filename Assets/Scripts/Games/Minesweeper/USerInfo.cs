using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoardTypes{Slider,Beginner,Intermediate,Expert}
public enum GameType{Normal,Challenge,Create,Test,Favourite}
public class USerInfo : MonoBehaviour
{
	
	public string userName = "";
	public string email = "";
	public string uid = "00";
    public string levelID;

	public GameType currentType = GameType.Normal;
	public static int EditMode { get; set; } = 0;

    public static USerInfo Instance { get; private set; }
	public BoardTypes BoardType { get; set; } = BoardTypes.Slider;
    public int ActiveBordSize { get; set; } = 6;
    public int LastUsedNormalBordSize { get; set; } = 6;
	public int Sensitivity { get; set; } = 15;
	public float SensitivityMS => Sensitivity / 100f;

	public bool UsePending { get; set; } = false;
	public int Theme { get; set; } = 0;
	public bool UseRotatedExpert { get; set; } = false;
    public bool IsPlayerLoggedIn { get; set; } = false;
	public bool WaitForFirstMove { get; set; } = true;
    public string Collection { get; set; }
	public List<string> ActiveCollections { get; set; } = new();
	public List<string> InactiveCollections { get; set; } = new();
	public bool LoadRandom { get; internal set; } = true;

    private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		//SavingUtility.LoadingComplete += SetDataFromSaveFile;
	}

	public static Action BoardSizeChange;

}
