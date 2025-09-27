using System;
using System.Collections.Generic;

public enum BoardTypes{Slider,Beginner,Intermediate,Expert}
public enum GameType{Normal,Challenge,Create,Test,Favourite}
public static class USerInfo
{	
	public static string userName = "";
	public static string email = "";
	public static string uid = "00";
    public static string levelID;

	public static GameType currentType = GameType.Normal;

	public static int BoardDifficulty => CalculateDifficulty();

    // Minesweeper
    public static int[] NormalDifficultys = { 10, 30, 100 };
    public static float[] MinesweeperRatingDrops = { 1f, 0.8f, 0.5f, 0.3f, 0.1f };
    public static float[] MinesweeperRatingDropLevels = { 1400, 1600, 1800, 2000, 3000 };

    private static int CalculateDifficulty()
    {
        if(BoardType == BoardTypes.Slider) {
			// this is defined by game size

			// Do a caluclation here?
			return ActiveBordSize;
		}
		// BoardType 1,2,3 is Beginner,Inermediate,Expert
        return NormalDifficultys[(int)BoardType-1];

    }

    public static int EditMode { get; set; } = 0;

    //public static USerInfo Instance { get; private set; }
	public static BoardTypes BoardType { get; set; } = BoardTypes.Slider;
    public static int ActiveBordSize { get; set; } = 6;
    public static int LastUsedNormalBordSize { get; set; } = 6;
	public static int Sensitivity { get; set; } = 15;
	public static float SensitivityMS => Sensitivity / 100f;

	public static bool UsePending { get; set; } = false;
	public static int Theme { get; set; } = 0;
	public static bool UseRotatedExpert { get; set; } = false;
    public static bool IsPlayerLoggedIn { get; set; } = false;
	public static bool WaitForFirstMove { get; set; } = true;
    public static string Collection { get; set; }
	public static List<string> ActiveCollections { get; set; } = new();
	public static List<string> InactiveCollections { get; set; } = new();
	public static bool LoadRandom { get; internal set; } = true;

	public static Action BoardSizeChange;

}
