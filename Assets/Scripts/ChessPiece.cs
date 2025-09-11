using System;
using UnityEngine;
using UnityEngine.UI;
public class ChessPiece : MonoBehaviour
{
    [SerializeField] private ChessPieceData chessPieceData;
    [SerializeField] private Image image;
    public int Type { get; private set; }
    private bool dragging = false;

    public int Color => Type / 6;

    public Vector2Int Pos { get; private set; }
    public Vector3 HomePosition { get; private set; }

    internal void SetType(int type)
    {
        Type = type;
        // Set its visual
        image.sprite = chessPieceData.Sprites[Type];
    }

    internal void SetPositionAndType(Vector3Int piecePosition, Vector3 homePos)
    {
        //Debug.Log("SET POSITION AND TYPE");
        // Set its index
        Pos = new Vector2Int(piecePosition.x,piecePosition.y);

        // Place it correctly
        HomePosition = homePos;
        transform.localPosition = homePos;

        // Set type
        Type = piecePosition.z;

        // Set its visual
        image.sprite = chessPieceData.Sprites[Type];
    }

    internal void Hide(bool v) => image.enabled = !v;

    internal void ChangeType(int type)
    {
        Type = type;

        // Set its visual
        image.sprite = chessPieceData.Sprites[Type];
    }

    internal bool EnPassent(ChessMove playersMove, ChessPiece enpassentedPiece)
    {
        Debug.Log("Enpassanted piece = "+enpassentedPiece);
        // Was even a piece in the en passent piosition?
        if(enpassentedPiece == null) return false;
        

        // Check if this was a pawn that moved
        if(!(Type == 5 || Type == 11)) return false;

        Debug.Log("En passent type = "+Type);

        int sideSteps = Math.Abs(playersMove.from.x - playersMove.to.x);

        Debug.Log("sidesteps = "+sideSteps);
        Debug.Log("PLayer move from  = "+playersMove.from.y);

        // Possible enPassentMove
        if((playersMove.from.y == 3 || playersMove.from.y == 4) && sideSteps == 1){ // A piece is in the en passent position - Piece moved is a pawn - It moves from correct row to be an en passent - it steps one step to the side
            return true;
        }
        // This only checks if an en passent was made - if so the oponents pawn needs to be removed
        return false;
    }

    internal bool PlayerCastle(ChessMove playersMove)
    {
        // King is in start position and is moved 2 steps to left or right - does not check for pieces in the way or no rooks
        return (playersMove.from.x == 4 && playersMove.from.y == 0 && playersMove.to.y == 0 && (playersMove.to.x == 2 || playersMove.to.x == 6)) ||
               (playersMove.from.x == 3 && playersMove.from.y == 0 && playersMove.to.y == 0 && (playersMove.to.x == 1 || playersMove.to.x == 5));
    }
    internal bool ComputerCastle(ChessMove computerMove)
    {
        // King is in start position and is moved 2 steps to left or right - does not check for pieces in the way or no rooks
        return (computerMove.from.x == 4 && computerMove.from.y == 7 && computerMove.to.y == 7 && (computerMove.to.x == 2 || computerMove.to.x == 6)) ||
               (computerMove.from.x == 3 && computerMove.from.y == 7 && computerMove.to.y == 7 && (computerMove.to.x == 1 || computerMove.to.x == 5));
    }
}
