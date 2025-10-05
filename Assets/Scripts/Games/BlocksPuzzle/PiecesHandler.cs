using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PiecesHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private BlocksPuzzleGhostController ghostController;
    [SerializeField] private RectTransform gameAreaRect;

    public Vector2 Offset { get; private set; } = new Vector2();

    private MovablePiece activePiece;

    public static PiecesHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (activePiece == null) return;

        if (Mouse.current.leftButton.wasReleasedThisFrame) {

            // Place if possible
            bool valid = BlocksPuzzle.Instance.TryPlacePiece(activePiece);

            DropPiece(valid);
            // reshow the moved piece

        }

        Vector2 pos = Mouse.current.position.ReadValue();

        //Debug.Log("Mouse Position: Mouse at screen position ["+pos.x+","+pos.y+"]");

        // Get mouse local position and move ghost there if active
        pos = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>());
        //Debug.Log("Block Puzzle: Mouse local position inside Border rect is: ["+pos.x+","+pos.y+"]");
        ghostController.transform.localPosition = pos;

    }

    private void DropPiece(bool valid)
    {
        activePiece.gameObject.SetActive(true);
        ghostController.Hide();
        activePiece = null;
    }

    public void StartMovePiece(PointerEventData eventData, MovablePiece piece)
    {
        activePiece = piece;
        BlocksPuzzle.Instance.ClearBoardSpots(activePiece);

        activePiece = piece;

        // Convert mouse position to world position
        Vector3 worldMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            piece.RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldMousePos
        );

        // Calculate local offset within the piece
        Offset = piece.RectTransform.InverseTransformPoint(worldMousePos);

        // Activate the ghost using the offset
        ghostController.ActivatePiece(piece.Type, Offset);

        BlocksPuzzle.Instance.ClearBoardSpots(activePiece);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Need this to happen even on mouse release outside?
        //Debug.Log("STOP Moving Piece");
        activePiece = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Vector2 pos = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>());
        //Debug.Log("Mouse eventdata Position: [" + eventData.position.x + "," + eventData.position.y + "] Block Puzzle: Mouse read local position: [" + pos.x + ", " + pos.y + "]");
    }
}
