using System;
using UnityEngine;
using UnityEngine.UI;

public enum TetrisBlockType { Empty, Fixed, I, J, L, O, S, T, Z, Ghost}
public class TetrisBlock : MonoBehaviour
{

    protected TetrisBlockType boxType = TetrisBlockType.Empty;


    [SerializeField] Image image;

    public void SetType(int type, bool overLayer = true)
    {
        boxType = (TetrisBlockType)type;        
        UpdateSprite();
    }

    internal void UpdateSprite() => image.sprite = ImagesIcons.Instance.GetTetrisIcon((int)boxType);

}
