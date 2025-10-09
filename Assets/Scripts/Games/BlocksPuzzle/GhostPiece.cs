using UnityEngine;

public class GhostPiece : BasePiece
{
    // Ghost Method for copying a piece
    internal void MimicTypeAndRotation(MovablePiece piece)
    {
        // Dont need this for the ghost???
        Rotation = piece.Rotation;

        // Get the other pieces blocks
        TetrisBlock[] otherBlocks = piece.TetrisBlocks;

        Debug.Log("Mimic tetrisblock = null: "+(TetrisBlocks == null));

        // copy each blocks position
        for (int i = 0; i < TetrisBlocks.Length; i++) {
            TetrisBlocks[i].transform.localPosition = otherBlocks[i].transform.localPosition;
            TetrisBlocks[i].SetType((int)piece.Type);
        }
    }
}
