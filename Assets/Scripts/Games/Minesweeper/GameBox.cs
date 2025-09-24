using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameBox : MonoBehaviour
{
    // Make this clickable
    public Vector2Int Pos { get; set; }
    public int value { get; set; }
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D boxCollider;


    public bool Active => boxCollider.enabled;
    public bool Marked { get; set; } = false;
    public bool Busted { get; set; } = false;

    private MineBoxType boxType = MineBoxType.Unflagged;

    private void Start()
    {
//Inputs.Instance.Controls.Main.Mouse.started += MouseDown;
    }

    public void SetType(int type,bool keepColliderEnable = false)
    {
        value = type;
        if(value == -1)
        {
            boxType = MineBoxType.Busted;
            UpdateSprite();
        }
        else
        {
           // spriteRenderer.sprite = ThemePicker.Instance.current.numbers[type];
            //return;
        }
            //spriteRenderer.sprite = cleared[type];

        if(!keepColliderEnable)
            boxCollider.enabled = false;
    }

    public void MakeInteractable(bool doSet = true)
    {
        boxCollider.enabled = doSet;
    }

    private void MouseDown(InputAction.CallbackContext context)
    {
        //Debug.Log("Mouse triggered");
    }

    public void Click()
    {
        if (GameAreaMaster.Instance.MainGameArea.LevelBusted)
            return;



        Debug.Log("Clicking Box value = "+value+" when Gametype 0 "+USerInfo.Instance.currentType);

        if (USerInfo.Instance.currentType == GameType.Create)
        {
            GameAreaMaster.Instance.MainGameArea.OpenBoxCreate(Pos);
            return;
        }
        // If Busted Level disallow any click on Area



        // This workds for normal gameplay keep it
        if (value > 0)
        {
            bool wasted = Chord();
            if(wasted)
                GameAreaMaster.Instance.MainGameArea.AddClicks();
            return;
        }
        if (Marked) {
            return;
        }

        if (GameAreaMaster.Instance.MainGameArea.OpenBox(Pos))
        {
            //RemoveAndSetUnderActive();
        }
        else
        {
            Debug.Log("Show a busted mine here");
            boxType = MineBoxType.Busted;
            UpdateSprite();            
        }
    }

    public void RemoveAndSetUnderActive()
    {
        if (!boxCollider.enabled) return;
        MakeInteractable(false);
        transform.gameObject.SetActive(false);
    }

    private bool Chord()
    {
        return GameAreaMaster.Instance.MainGameArea.Chord(Pos);
    }

    public void Mark()
    {
        if (Marked) return;
        Marked = true;
        boxType = MineBoxType.Flagged;
        UpdateSprite(); 
        GameAreaMaster.Instance.MainGameArea.DecreaseMineCount();
    }

    public void RightClick(bool hidden = false)
    {

        // Dont flag non mines in Create B
        if (hidden && USerInfo.EditMode == 1 && !GameAreaMaster.Instance.MainGameArea.IsMine(Pos)) // Edit mode 1 == EDIT mode B
        {
            Click();
            return;
        }

        Debug.Log("Clicking this box at " + Pos + " mark or demark as mine value =" + value+" ");
        if (value > 0) return;
        Marked = !Marked;

        if (Marked)
            SetAsFlaggedMine();
        else
        {
            if(hidden)
                SetAsHiddenMine();
            else
                SetAsUnFlagged();
        }
        Debug.Log("Right Clicking Box, hidden = " + hidden);

        if (LevelCreator.Instance.EditMode)
        {
            GameAreaMaster.Instance.MainGameArea.UpdateMineCount();
            return; // Breaks if in edit mode and placing Mines
        }

        // Rightclicking is never a wasted click?
        //GameAreaMaster.Instance.MainGameArea.AddClicks();

        if (!Marked)
            GameAreaMaster.Instance.MainGameArea.IncreaseMineCount();
        else
            GameAreaMaster.Instance.MainGameArea.DecreaseMineCount();
    }

    public void SetAsHiddenMine()
    {
        boxType = MineBoxType.HiddenMine;
        UpdateSprite();
    }

    public void SetAsFlaggedMine()
    {
        boxType = MineBoxType.Flagged;
        UpdateSprite();
    }

    public void SetAsUnFlagged()
    {
        boxType = MineBoxType.Unflagged;
        UpdateSprite();
    }

    public void Bust()
    {
        boxType = MineBoxType.Busted;
        UpdateSprite();
        Busted = true;
    }

    public void ShowWrongFlag()
    {
        boxType = MineBoxType.WrongFlag;
        UpdateSprite();
    }

    public void ShowMine()
    {
        boxType = MineBoxType.Mine;
        UpdateSprite();        
    }

    public bool UnSolved()
    {
        return boxCollider.enabled;
    }

    public void Reset()
    {
        transform.gameObject.SetActive(true);
        Busted = false;
        Marked = false;
        //spriteRenderer.sprite = ThemePicker.Instance.current.flags[(int)MineBoxType.Unflagged];
        boxCollider.enabled = true;
    }

    internal void SetOrderingLeyer(int v) => spriteRenderer.sortingOrder = v;

    internal void UpdateSprite() => spriteRenderer.sprite = ThemePicker.Instance.current.flags[(int)boxType];
}
