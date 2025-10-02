public class SnakeBlock : TetrisBlock
{
    public bool IsEmpty => boxType == TetrisBlockType.Empty;
    public bool IsSnake => boxType == TetrisBlockType.S;
    public bool IsApple => boxType == TetrisBlockType.Z;
}
