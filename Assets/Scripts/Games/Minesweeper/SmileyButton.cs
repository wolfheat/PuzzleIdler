using System;
using UnityEngine;

public class SmileyButton : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BoxCollider2D boxCollider;

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

    public void SetColliderWidth(float newWidth) => boxCollider.size = new Vector2(newWidth, boxCollider.size.y);
    
    public void Click()
    {
        Debug.Log("Clicking Smiley!");
        LevelCreator.Instance.RestartGame(true);
        spriteRenderer.sprite = sprites[1];
    }

    public void ShowWin()
    {
        spriteRenderer.sprite = sprites[3];
    }
    
    public void ShowBust()
    {
        Debug.Log("Show Bust?");
        spriteRenderer.sprite = sprites[2];
    }
    
    public void ShowNormal()
    {
        spriteRenderer.sprite = sprites[0];
    }



}
