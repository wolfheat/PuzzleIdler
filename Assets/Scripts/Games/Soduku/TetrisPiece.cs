using System.Collections.Generic;
using UnityEngine;

public class TetrisPiece
{
    public Vector2Int pos = new Vector2Int(0,0);
    public List<List<Vector2Int>> occupySpots;
    public int activeRotation = 0;
    public TetrisBlockType type;

    public List<Vector2Int> CurrentRotationSpots => occupySpots[activeRotation];
    public List<Vector2Int> NextRotationSpots => occupySpots[(activeRotation == occupySpots.Count-1) ? 0 : activeRotation+1];
    public void Rotate() => activeRotation = (activeRotation == occupySpots.Count - 1) ? 0 : (activeRotation + 1);
    public TetrisBlockType Type => type;
}

public class OPiece : TetrisPiece
{
    public OPiece() {
        type = TetrisBlockType.O;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
        occupySpots = new List<List<Vector2Int>>() { rotA };
        // Cant be rotated
    }
}

public class IPiece : TetrisPiece
{
    public IPiece() {
        type = TetrisBlockType.I;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> {new Vector2Int(0, -2), new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1)};
        List<Vector2Int> rotB = new List<Vector2Int> {new Vector2Int(-2, 0), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0)};
        //  O       
        //  O
        //  X   O O X O
        //  O
        occupySpots = new List<List<Vector2Int>>() { rotA, rotB };
    }
}

public class JPiece : TetrisPiece
{
    public JPiece() {
        type = TetrisBlockType.J;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, -1) };
        List<Vector2Int> rotB = new List<Vector2Int> { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1) };
        List<Vector2Int> rotC = new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) };
        List<Vector2Int> rotD = new List<Vector2Int> { new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, -1) };
        //  O       
        //  O X 0

        //    O       
        //    X       
        //  O O 

        occupySpots = new List<List<Vector2Int>>() { rotA, rotB, rotC, rotD };
    }
}

public class LPiece : TetrisPiece
{
    public LPiece() {
        type = TetrisBlockType.L;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> {new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1)};
        List<Vector2Int> rotB = new List<Vector2Int> {new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)};
        List<Vector2Int> rotC = new List<Vector2Int> {new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1)};
        List<Vector2Int> rotD = new List<Vector2Int> {new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(-1, -1)};
        //      O       
        //  O X 0

        //    O       
        //    X       
        //    O O 

        occupySpots = new List<List<Vector2Int>>() { rotA, rotB, rotC, rotD };
    }
}

public class SPiece : TetrisPiece
{
    public SPiece() {
        type = TetrisBlockType.S;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> {new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)};
        List<Vector2Int> rotB = new List<Vector2Int> {new Vector2Int(-1,-1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1)};
        //    O O       
        //  O X 
        //  O       
        //  O X       
        //    O 

        occupySpots = new List<List<Vector2Int>>() { rotA, rotB };
    }
}

public class ZPiece : TetrisPiece
{
    public ZPiece() {
        type = TetrisBlockType.Z;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> {new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(1, 0)};
        List<Vector2Int> rotB = new List<Vector2Int> {new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,1)};
        //  O O       
        //    X O
        //    O       
        //  O X       
        //  O 

        occupySpots = new List<List<Vector2Int>>() { rotA, rotB };
    }
}

public class TPiece : TetrisPiece
{
    public TPiece() {
        type = TetrisBlockType.Z;
        // Add the rotations positions
        List<Vector2Int> rotA = new List<Vector2Int> {new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(1, 0)};
        List<Vector2Int> rotB = new List<Vector2Int> {new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0,-1)};
        List<Vector2Int> rotC = new List<Vector2Int> {new Vector2Int( 1, 0), new Vector2Int(0,-1), new Vector2Int(0, 0), new Vector2Int(0, 1)};
        List<Vector2Int> rotD = new List<Vector2Int> {new Vector2Int( 1, 0), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0,-1)};
        //    O       
        //  O X O
        //    O       
        //  O X       
        //    O 

        occupySpots = new List<List<Vector2Int>>() { rotA, rotB ,rotC, rotD};
    }
}
