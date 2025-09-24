using System;
using UnityEngine;
using UnityEngine.UI;

public class SmileyButton : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] Image image;

    // On mouse down show pressed button, on release start new game

    public static SmileyButton Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        //FirestoreManager.OnLevelCollectionListChange += SetSmileyTypeFromLevelAmountLoaded;
    }
    
    public void Click()
    {
        Debug.Log("Clicking Smiley!");
        LevelCreator.Instance.RestartGame(true);
        image.sprite = sprites[1];
    }

    public void ShowWin()
    {
        image.sprite = sprites[3];
    }
    
    public void ShowBust()
    {
        Debug.Log("Show Bust?");
        image.sprite = sprites[2];
    }
    
    public void ShowNormal()
    {
        image.sprite = sprites[0];
    }



}
