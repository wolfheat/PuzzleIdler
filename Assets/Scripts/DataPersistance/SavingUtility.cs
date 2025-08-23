using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

public class SavingUtility : MonoBehaviour
{
    private const string PlayerDataSaveFile = "/player-data.txt";
    private const string GameSettingsDataSaveFile = "/player-settings.txt";
    public static SavingUtility Instance { get; private set; }

    public static Action LoadingComplete;  

    public static PlayerGameData playerGameData;
    public static GameSettingsData gameSettingsData;
    public static bool HasLoaded = false;


    private void Awake()
    {
        Debug.Log("Savingutility - Awake");
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;    
    }

    private void Start()
    {
        Debug.Log("Savingutility - Start");
        StartCoroutine(LoadFromFile());
    }

    public void PlayerInitiatedQuit()
    {
        Debug.Log("Saving on Quit");
        PlayerGameData.SaveNeeded?.Invoke();
        StartCoroutine(DelayedQuit());
    }

    private IEnumerator DelayedQuit()
    {
        yield return new WaitForSeconds(1.5f);
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
        Debug.Log("Quit");
    }

    public void ResetSaveFile()
    {
        Debug.Log("Resetting save data, keeps the game settings data");
        playerGameData = new PlayerGameData();
        IDataService dataService = new JsonDataService();
        if (dataService.SaveData(PlayerDataSaveFile, playerGameData, false))
            Debug.Log("Player save file was reset: " + PlayerDataSaveFile);
        else
            Debug.LogError("Could not reset file.");

        LoadingComplete?.Invoke(); // Call this to update all ingame values
    }

    public void SaveAllDataToFile()
    {
        SavePlayerDataToFile();
        SaveSettingsDataToFile();
    }
    public void SavePlayerDataToFile()
    {
        IDataService dataService = new JsonDataService();
        if (dataService.SaveData(PlayerDataSaveFile, playerGameData, false))
        {
            Debug.Log("Saved player data in: "+PlayerDataSaveFile);
            // Makes sure saved data is loaded when reentering game
        }
        else
            Debug.LogError("Could not save file: PlayerData");        
    }
    
    public void SaveSettingsDataToFile()
    {
        IDataService dataService = new JsonDataService();
        if (dataService.SaveData(GameSettingsDataSaveFile, gameSettingsData, false))
            Debug.Log("Saved settings data in: " + GameSettingsDataSaveFile);
        else
            Debug.LogError("Could not save file: GameSettingsData");
    }

    public IEnumerator LoadFromFile()
    {
        // Hold the load so Game has time to load
        yield return new WaitForSeconds(0.4f);

        IDataService dataService = new JsonDataService();
        try
        {
            Debug.Log("** Trying To load data from file. **");
            PlayerGameData data = dataService.LoadData<PlayerGameData>(PlayerDataSaveFile, false);
            if (data != null)
            {
                Debug.Log("  PlayerGameData loaded - Valid data!");
                playerGameData = data;
                Debug.Log("*** Loaded coins held "+playerGameData.coins);
                Debug.Log("*** mantissa "+playerGameData.coins.Mantissa+" e"+playerGameData.coins.Exponent);
                Debug.Log("*** Loaded play time "+playerGameData.PlayTime);
                Debug.Log("*** Save time "+playerGameData.SaveTime);
            }
            else
            {
                playerGameData = new PlayerGameData();
            }            
        }
        catch   
        {
            Debug.Log("  Could not load data, set default: ");
            playerGameData = new PlayerGameData();                        
        }
        try
        {
            Debug.Log("** Trying To load settings data from file. ** ");
            GameSettingsData data = dataService.LoadData<GameSettingsData>(GameSettingsDataSaveFile, false);
            if (data != null)
            {
                Debug.Log("  SettingsData loaded - Valid data!");
                gameSettingsData = data;
            }
            else
            {
                gameSettingsData = new GameSettingsData();
            }
        }
        catch
        {
            Debug.Log("  Could not load settings data, set default: ");
            gameSettingsData = new GameSettingsData();
        }
        finally
        {
            // Add listener to update of data to save
            PlayerGameData.SaveNeeded += SavePlayerDataToFile;
            GameSettingsData.SaveNeeded += SaveSettingsDataToFile;

            HasLoaded = true;

            // Load Up settings with this data? No do it 

            Debug.Log(" -- Loading From File Completed --");

            // Handle AwayIncome

            LoadingComplete?.Invoke();
            //SoundMaster.Instance.ReadDataFromSave(); // Used to set sound settings aftyer loading from file

            StartCoroutine(KeepTrackOfPlaytime());

        }
    }

    private IEnumerator KeepTrackOfPlaytime()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
            playerGameData.AddPlayTimeMinutes(1);
            Debug.Log("Tick ONE minute played Total: "+playerGameData.PlayTime);
        }
    }

    public static Vector3 V3AsVector3(float[] v3)
    {
        return new Vector3(v3[0], v3[1], v3[2]);
    }
    public static float[] Vector3AsV3(Vector3 v)
    {
        return new float[3]{ v.x, v.y, v.z};
    }
    
    public static Quaternion V4AsQuaternion(float[] v4)
    {
        return new Quaternion(v4[0], v4[1], v4[2], v4[3]);
    }
    public static float[] QuaternionAsV4(Quaternion q)
    {
        return new float[4]{ q.x, q.y, q.z, q.w};
    }

    public void ClearGameData()
    {
        Debug.Log("Clear Game Data");
        IDataService dataService = new JsonDataService();
        dataService.RemoveData(PlayerDataSaveFile);
        // After this clear the game data in game
        playerGameData = null;
    }
}
