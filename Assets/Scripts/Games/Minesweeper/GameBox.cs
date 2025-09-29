using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
// Make this clickable


public class GameBox : MonoBehaviour
{
    // Make this clickable
    public Vector2Int Pos { get; set; }
    public int value { get; set; }
    [SerializeField] Image image;
    //[SerializeField] Collider2D boxCollider;

    //public bool Active => boxCollider.enabled;

    public bool Marked { get; set; } = false;
    public bool Busted { get; set; } = false;
    public bool IsOpen { get; private set; } = false;

    private MineBoxType boxType = MineBoxType.Unflagged;

    private void Start()
    {
//Inputs.Instance.Controls.Main.Mouse.started += MouseDown;
    }

    public void SetType(int type, bool overLayer = true)
    {
        boxType = (MineBoxType)type;
        Debug.Log("Updating sprite to type "+boxType+" = "+(int)boxType);
        if(type == -1) {
            image.sprite = ThemePicker.Instance.currentMinesweeper.flags[(int)MineBoxType.Busted];
            return;
        }
        UpdateSprite();
    }

    public void MakeInteractable(bool doSet = true)
    {
        //boxCollider.enabled = doSet;
    }

    private void MouseDown(InputAction.CallbackContext context)
    {
        //Debug.Log("Mouse triggered");
    }



    public void Click() => Open();

    public bool IsClickable() => (!IsOpen && !Marked);

    private bool Opened() => IsOpen;

    private void Open()
    {
        IsOpen = true;

        // Hide it
        image.enabled = false;
        //Debug.Log("Disabling Image for Gamebox at "+Pos+" enable = "+image+" "+image?.enabled);

    }

    public void RemoveAndSetUnderActive()
    {
        //if (!boxCollider.enabled) return;
        MakeInteractable(false);
        transform.gameObject.SetActive(false);
    }

    public void Mark()
    {
        if (Marked) return;
        Marked = true;
        boxType = MineBoxType.Flagged;
        UpdateSprite(); 
        GameArea.Instance.DecreaseMineCount();
    }

    public void RightClick()
    {

        if (value > 0) return;
        Marked = !Marked;

        if (Marked)
            SetAsFlaggedMine();
        else
        {
            SetAsUnFlagged();
        }
        
        if (!Marked)
            GameArea.Instance.IncreaseMineCount();
        else
            GameArea.Instance.DecreaseMineCount();
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
        return true;
        //return boxCollider.enabled;
    }

    public void Reset()
    {
        transform.gameObject.SetActive(true);
        Busted = false;
        Marked = false;
        //spriteRenderer.sprite = ThemePicker.Instance.current.flags[(int)MineBoxType.Unflagged];
        //boxCollider.enabled = true;
    }

    //internal void SetOrderingLeyer(int v) => spriteRenderer.sortingOrder = v;

    internal void UpdateSprite() => image.sprite = ThemePicker.Instance.currentMinesweeper.flags[(int)boxType];

    internal void SetAsPressed()
    {
        boxType = MineBoxType.Open;
        UpdateSprite();
    }

    internal void UnPress()
    {
        if(boxType != MineBoxType.Unflagged) {
            boxType = MineBoxType.Unflagged;
            UpdateSprite();
        }
    }
}
