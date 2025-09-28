using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmileyButton : MonoBehaviour, IPointerDownHandler
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
    
    public void MouseDown()
    {
        Debug.Log("Clicking Smiley!");
        MineSweeper.Instance.RestartGame(true);
        image.sprite = sprites[1];
        pressedTimer = PressTime;

    }

    float pressedTimer = 0;
    private const float PressTime = 0.25f;

    private void Update()
    {
        if(pressedTimer > 0) {
            pressedTimer -= Time.deltaTime;
            if(pressedTimer <= 0) {
                pressedTimer = 0;
                ShowNormal();
            }
        }
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

    public void OnPointerDown(PointerEventData eventData)
    {
        MouseDown();
    }
}
