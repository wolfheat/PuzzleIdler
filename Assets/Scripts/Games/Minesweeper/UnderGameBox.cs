using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UnderGameBox : MonoBehaviour
{
    // Make this clickable
    public Vector2Int Pos { get; set; }
    public int value { get; set; }
    [SerializeField] Image image;
    //[SerializeField] Collider2D boxCollider;


    //public bool Active => boxCollider.enabled;
    public bool Marked { get; set; } = false;
    public bool Busted { get; set; } = false;

    private int boxType = 0;

    private void Start()
    {
//Inputs.Instance.Controls.Main.Mouse.started += MouseDown;
    }

    public void SetType(int type, bool overLayer = true)
    {
        boxType = type;

        value = type;

        Debug.Log("Updating Under sprite to type "+boxType+" = "+(int)boxType);

        if(type == -1) {
            image.sprite = ThemePicker.Instance.current.flags[(int)MineBoxType.Busted];
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

    public void Click()
    {
        if (GameAreaMaster.Instance.MainGameArea.LevelBusted)
            return;
        // Need this?

    }

    public void RemoveAndSetUnderActive()
    {
        //if (!boxCollider.enabled) return;
        MakeInteractable(false);
        transform.gameObject.SetActive(false);
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

    internal void UpdateSprite() => image.sprite = ThemePicker.Instance.current.numbers[(int)boxType];
}
