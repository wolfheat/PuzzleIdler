using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PiecesHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private BlocksPuzzleGhostController ghostController;
    [SerializeField] private RectTransform gameAreaRect;
    
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
            Vector2Int endPosIndex = WolfheatProductions.Converter.GetMouseLocalPositionIndex(gameAreaRect,BlocksPuzzle.BlockSize*BlocksPuzzle.BlockScale);
            Debug.Log("Drop Piece at index [" + endPosIndex.x + "," + endPosIndex.y + "]");
            bool placed = BlocksPuzzle.Instance.TryPlacePiece(activePiece, endPosIndex);

            activePiece.gameObject.SetActive(true);
            ghostController.Hide();
            activePiece = null;
            // reshow the moved piece

        }

        Vector2 pos = Mouse.current.position.ReadValue();

        //Debug.Log("Mouse Position: Mouse at screen position ["+pos.x+","+pos.y+"]");

        // Get mouse local position and move ghost there if active
        pos = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>());
        //Debug.Log("Block Puzzle: Mouse local position inside Border rect is: ["+pos.x+","+pos.y+"]");
        ghostController.transform.localPosition = pos;

    }

    public void StartMovePiece(PointerEventData eventData, MovablePiece piece)
    {
        //OnPointerDown(eventData);
        //Debug.Log("Start To move piece "+piece.name);
        activePiece = piece;
        ghostController.ActivatePiece(piece.Type);
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
