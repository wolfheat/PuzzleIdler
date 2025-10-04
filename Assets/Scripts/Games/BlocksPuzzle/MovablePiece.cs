using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovablePiece : MonoBehaviour, IPointerDownHandler//, IPointerUpHandler, IPointerMoveHandler
{
    public TetrisBlockType Type = TetrisBlockType.Empty;

    public Vector2 home = Vector2.zero;

    public TetrisBlock[] GetAllTetrisBlocks => transform.GetComponentsInChildren<TetrisBlock>();

    private void Start()
    {
        SetHome();
    }

    private void SetHome()
    {
        home = transform.localPosition;
    }
    public void SendHome()
    {
        transform.position = home;
    }

    public void OnPointerDown(PointerEventData eventData)
    {


        //Debug.Log("Blocks Puzzle:Start Move Piece");    
        //Vector2 GridPosition = WolfheatProductions.Converter.GetMouseLocalPosition(eventData, this.GetComponentInParent<RectTransform>());
        //Debug.Log("Blocks Puzzle: Start Move Piece ["+GridPosition.x+","+GridPosition.y+"]");

        PiecesHandler.Instance.StartMovePiece(eventData, this);

        gameObject.SetActive(false);
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
