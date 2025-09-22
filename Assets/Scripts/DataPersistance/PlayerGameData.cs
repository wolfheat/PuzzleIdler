using System;
using System.Collections.Generic;
using BreakInfinity;

public class AchievementData
{
    public bool[] Data { get; set; } = new bool[0];
}

public class MissionsSaveData
{
    public Dictionary<int,MissionSaveData> Data { get; set; } = new Dictionary<int, MissionSaveData>();
}

public class MissionSaveData
{
    public bool everCompleted = false;
    public bool active = true;
    public int amount = 0;
    public DateTime lastCompletion = DateTime.MinValue;


    public static Action MissionUpdate;

    public void SetMissionCompletionInfo()
    {
        // Set new last completiontime
        lastCompletion = DateTime.UtcNow;
        everCompleted = true;
        amount = 0;

        MissionUpdate?.Invoke();
    }

    public bool CompleteStepForMission(int completeAmount)
    {
        amount++;

        // Invoke if not completed (if completed UpdateMissionCompletion will be called which invokes the save)
        MissionUpdate?.Invoke();

        return amount >= completeAmount;
    }

}

[Serializable]
public class LightSettings
{
    public float LightIntensity { get; set; } = 1;
}

[Serializable]
public class PlayerInputSettings
{
    public float MouseSensitivity { get; set; } = 0.36f;
}

[Serializable]
public class SoundSettings
{
    public bool GlobalMaster{ get; set; } = true;
    public bool UseMaster { get; set; } = true;
    public bool UseMusic { get; set; } = true;
    public float MasterVolume { get; set; } = 0.5f;
    public float MusicVolume { get; set; } = 0.2f;
    public bool UseSFX { get; set; } = true;
    public float SFXVolume { get; set; } = 0.4f;
}


[Serializable]
public class SaveEnemy : SaveItem
{
    public int health; 
}

[Serializable]
public class InventorySave
{
    public int[] resources;
    public InventorySaveItem[] inventorySaveItems;
}

[Serializable]
public class InventorySaveItem
{
    public int mainType;
    public int subType;
    public int[] gridPosition;
}

[Serializable]
public class SaveItem
{
    public int id; 
    public float[] position; 
    public float[] rotation;
    public SaveItem() { }
}

[Serializable]
public class SaveDroppedItem
{
    public int mainType; 
    public int subType; 
    public float[] position; 
    public float[] rotation;
}

[Serializable]
public class PlayerGameData
{
    public PlayerGameData()
    {
        PlayTime = 0;
    }

    // MINI GAMES STATS
    public int PlayerChessRating { get; set; } = 1000;
    public int PlayerMinesweeperRating { get; set; } = 1000;

    
    // Held coins
    public BigDouble coins{ get; set; } 

    // Held Gems
    public BigDouble gems{ get; set; } 

    // Save buildings levels
    public int[] buildings = new int[0];

    // Save bought upgrades
    public Dictionary<string, bool> upgrades = new Dictionary<string, bool>();

    // Save bought upgrades
    public Dictionary<string, int> researches = new Dictionary<string, int>();
    
    public const int AutoSaveInterval = 15;

    public int PlayTime { get; set; } 
    public DateTime SaveTime { get; set; } = DateTime.Now;

    // Action Events
    public static Action SaveNeeded;
    public static Action MinuteWatched;

    public void TriggerSave()
    {
        // also have this just trigger the even that happens on lateUpdate ones
        NoticeController.Instance.ShowDebugText("TRIGGER SAVE");
        SaveTime = DateTime.Now;
        SaveNeeded?.Invoke();
    }

    public void AddPlayTimeMinutes(int amt)
    {
        PlayTime += amt;
        MinuteWatched?.Invoke();
        if(PlayTime % AutoSaveInterval == 0) {
            TriggerSave();
        }
    }
}

[Serializable]
public class GameSettingsData
{
    // General Game Settings
    public NumberNotation ActiveNumberNotation  { get; set; } // Having these private set wont let the load method write these values

    public int ActiveTouchControl { get; set; } // Having these private set wont let the load method write these values
    public int CameraPos { get; set; } // Having these private set wont let the load method write these values

    public SoundSettings soundSettings = new SoundSettings();
    public LightSettings lightSettings = new LightSettings();
    public PlayerInputSettings playerInputSettings = new PlayerInputSettings(); // Use shake etc

    // Action Events
    public static Action SaveNeeded;

    // General Settings - methods
    public void SetSoundSettings(float master, float music, float SFX,bool setFromFile=false)
    {
        soundSettings.MasterVolume = master;
        soundSettings.MusicVolume = music;
        soundSettings.SFXVolume = SFX;
        if(!setFromFile)
            SaveNeeded?.Invoke();
    }
}
