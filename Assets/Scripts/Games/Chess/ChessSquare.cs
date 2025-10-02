using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChessSquare : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Sprite[] squareTypes;
    [SerializeField] private Image image;
    public int Type { get; private set; }
    public Vector2Int Pos { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Mouse up on Square: "+Pos.x+","+Pos.y);
    }

    internal void SetToType(int z, Vector2Int pos)
    {
        Pos = pos;
        Type = z;       
        image.sprite = squareTypes[Type];
    }
}
