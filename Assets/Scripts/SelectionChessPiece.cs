public class SelectionChessPiece : ChessPiece
{
    public void OnSelectThisPiece()
    {
        PieceSelector.Instance.ChangeSelected(this);
    }

}
