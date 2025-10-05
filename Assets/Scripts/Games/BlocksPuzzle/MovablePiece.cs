using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovablePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TetrisBlockType Type = TetrisBlockType.Empty;

    public Vector2 home = Vector2.zero;

    public RectTransform RectTransform;

    public TetrisBlock[] GetAllTetrisBlocks => transform.GetComponentsInChildren<TetrisBlock>();
    public Vector2Int[] OccupySpots { get; private set; } = new Vector2Int[0];

    private void Start()
    {
        RectTransform = GetComponent<RectTransform>();
        SetHome();
    }

    public void SetHome()
    {
        home = transform.localPosition;
    }
    public void ReturnHome()
    {
        transform.localPosition = home;
        OccupySpots = new Vector2Int[0];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Blocks Puzzle:Start Move Piece");    
        //Vector2 GridPosition = WolfheatProductions.Converter.GetMouseLocalPosition(eventData, this.GetComponentInParent<RectTransform>());
        //Debug.Log("Blocks Puzzle: Start Move Piece ["+GridPosition.x+","+GridPosition.y+"]");

        PiecesHandler.Instance.StartMovePiece(eventData, this);

        gameObject.SetActive(false);
    }

    internal void SetOccupySpots(Vector2Int[] indexPositions)
    {
        OccupySpots = indexPositions;
        Debug.Log("Adding occypySpots " + OccupySpots?.Length);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Blocks Puzzle:Drop Piece");    
    }
    /*
public void OnPointerMove(PointerEventData eventData)
{
Vector2 GridPosition = WolfheatProductions.Converter.GetMouseLocalPosition(eventData, this.GetComponentInParent<RectTransform>(), BlocksPuzzle.BlockSize);
Debug.Log("Blocks Puzzle: Move Piece ["+GridPosition.x+","+GridPosition.y+"]");
transform.localPosition = GridPosition;
}

public void OnPointerUp(PointerEventData eventData)
{
Vector2 GridPosition = WolfheatProductions.Converter.GetMouseLocalPosition(eventData, this.GetComponentInParent<RectTransform>(), BlocksPuzzle.BlockSize);
Debug.Log("Blocks Puzzle: Stop Move Piece ["+GridPosition.x+","+GridPosition.y+"]");
//Debug.Log("Blocks Puzzle:  Stop Move Piece");
}*/
}
