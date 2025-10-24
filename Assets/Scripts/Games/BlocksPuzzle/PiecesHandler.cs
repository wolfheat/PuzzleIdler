using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PiecesHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerMoveHandler
{
    [SerializeField] private BlocksPuzzleGhostController ghostController;
    [SerializeField] private RectTransform gameAreaRect;
    [SerializeField] private RectTransform rectTransform;

    public Vector2 Offset { get; private set; } = new Vector2();
    public Vector2 UnrotatedOffset { get; private set; } = new Vector2();

    private MovablePiece activePiece;
    enum PlayerInputRotation { Left, Right, Up, None };

    public static PiecesHandler Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        Inputs.Instance.PlayerControls.Player.Rotate.performed += OnPlayerRotateInput;

        //ResetGame();

    }

    private void OnPlayerRotateInput(InputAction.CallbackContext context)
    {
        if (!BlocksPuzzle.Instance.GameActive) return;

        ReadRotationInput(context);

        Debug.Log("Rotating: " + rotationPerformed);
            
        PerformRotation();

    }

    private PlayerInputRotation rotationPerformed;
    private void ReadRotationInput(InputAction.CallbackContext context)
    {

        // Read the rotation value
        Vector2 value = context.ReadValue<Vector2>();
        if (value.x < 0)
            rotationPerformed = PlayerInputRotation.Left;
        else if (value.x > 0)
            rotationPerformed = PlayerInputRotation.Right;
        else if (value.y > 0)
            rotationPerformed = PlayerInputRotation.Left;
        else if (value.y < 0)
            rotationPerformed = PlayerInputRotation.Right;
        else {
            rotationPerformed = PlayerInputRotation.None;
        }
    }

    // Amount of steps to rotate 90°
    private int[] ActualRotationMapping = { -1, 1, 2, 0 };
    private void PerformRotation()
    {
        if (rotationPerformed == PlayerInputRotation.None)
            return;

        int rotations = ActualRotationMapping[(int)rotationPerformed];

        // Do the rotation
        TryRotate(rotations);


        // Activate the ghost using the offset
        ghostController.UpdateOffset(Offset);

        // Unset the rotation
        rotationPerformed = PlayerInputRotation.None;
    }

    private void TryRotate(int rotations = 0)
    {        
        RotateCurrentPiece(rotations);
    }

    private void RotateCurrentPiece(int rotations)
    {
        if (activePiece == null) return;
        
        // Recalculate the offset
        Offset = activePiece.Rotate(rotations, UnrotatedOffset);

        ghostController.MimicRotation(activePiece);
    }


    public void ResetGame()
    {
        // Make playr able to interract with board again
        BlocksPuzzle.Instance.GameActive = true;

        DropPiece(false);

        Debug.Log("Blocks: Reset game");

        ghostController.Hide();

        // Load a new problem of correct difficulty level
        //(int[,] level, int diff) = SudokuProblemDatas.Instance.GetRandomProblem(Stats.MiniGameRating(GameType));


        //ClearBlocks();

        // Reset
        //ResetGameStats();

        // Set new first block
        //nextBlockType = RandomBlockType();

        //PlaceNextBlock();
        //Debug.Log("Loaded level " + level[0,0]+" level "+level.GetLength(0)+","+level.GetLength(1));

        // Remove Win Screen Notice
        //winNotice.gameObject.SetActive(false);

        //LoadLevel(level);


        //UpdateLevelRating(diff);
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

        Vector2 mouseLocalPosition = WolfheatProductions.Converter.GetMouseLocalPositionSpecificCamera(Camera.main,rectTransform);
        Debug.Log("Mouse is locally at "+mouseLocalPosition.x+", "+mouseLocalPosition.y);
        ghostController.transform.localPosition = mouseLocalPosition;

    }

    private void DropPiece(bool valid)
    {
        if (activePiece == null) return;

        activePiece.gameObject.SetActive(true);
        ghostController.Hide();
        activePiece = null;
    }

    public void StartMovePiece(PointerEventData eventData, MovablePiece piece)
    {
        activePiece = piece;


        // Read the rotation and change ghost accordingly

        //int piecerotation = piece.Rotation;

        // Lifting a piece that was placed
        //BlocksPuzzle.Instance.ClearBoardSpots(activePiece);

        if(piece == null)
            Debug.Log("Piece null");
        if(piece.RectTransform == null)
            Debug.Log("Piece Recttransform null");

        // Convert mouse position to pieceHandler position
        Vector2 pieceOffestPosition = WolfheatProductions.Converter.GetMouseLocalPositionSpecificCamera(Camera.main, piece.RectTransform, eventData);
        Vector2 mouseLocalPosition = WolfheatProductions.Converter.GetMouseLocalPositionSpecificCamera(Camera.main, gameAreaRect, eventData);

        // Calculate local offset within the piece
        Offset = BlocksPuzzle.Snap ? Vector2.zero : pieceOffestPosition;

        // This issnt set correctly for rotated piece
        UnrotatedOffset = BlocksPuzzle.Snap ? Vector2.zero : piece.GetUnrotatedOffesetForPoint(pieceOffestPosition);
        

        //Offset = piece.RectTransform.InverseTransformPoint(pieceHandlerMousePos);

        // Activate the ghost using the offset
        ghostController.ActivatePiece(piece, Offset);

        BlocksPuzzle.Instance.ClearBoardSpots(activePiece);

        ghostController.transform.localPosition = BlocksPuzzle.Snap ? Vector2.zero : mouseLocalPosition;
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

    public void OnPointerMove(PointerEventData eventData)
    {
    }
}

//public class PiecesHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
//{
//    [SerializeField] private BlocksPuzzleGhostController ghostController;
//    [SerializeField] private RectTransform gameAreaRect;

//    public Vector2 Offset { get; private set; } = new Vector2();

//    private MovablePiece activePiece;

//    public static PiecesHandler Instance { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null) {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//    }

//    private void Update()
//    {
//        if (activePiece == null) return;

//        if (Mouse.current.leftButton.wasReleasedThisFrame) {

//            // Place if possible
//            bool valid = BlocksPuzzle.Instance.TryPlacePiece(activePiece);

//            DropPiece(valid);
//            // reshow the moved piece

//        }

//        Vector2 pos = Mouse.current.position.ReadValue();

//        //Debug.Log("Mouse Position: Mouse at screen position ["+pos.x+","+pos.y+"]");

//        // Get mouse local position and move ghost there if active
//        pos = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>());
//        //Debug.Log("Block Puzzle: Mouse local position inside Border rect is: ["+pos.x+","+pos.y+"]");
//        ghostController.transform.localPosition = pos;

//    }

//    private void DropPiece(bool valid)
//    {
//        activePiece.gameObject.SetActive(true);
//        ghostController.Hide();
//        activePiece = null;
//    }

//    public void StartMovePiece(PointerEventData eventData, MovablePiece piece)
//    {
//        activePiece = piece;
//        BlocksPuzzle.Instance.ClearBoardSpots(activePiece);

//        activePiece = piece;

//        // Convert mouse position to world position
//        Vector3 worldMousePos;
//        RectTransformUtility.ScreenPointToWorldPointInRectangle(
//            piece.RectTransform,
//            eventData.position,
//            eventData.pressEventCamera,
//            out worldMousePos
//        );

//        // Calculate local offset within the piece
//        Offset = piece.RectTransform.InverseTransformPoint(worldMousePos);

//        // Activate the ghost using the offset
//        ghostController.ActivatePiece(piece.Type, Offset);

//        BlocksPuzzle.Instance.ClearBoardSpots(activePiece);
//    }

//    public void OnPointerUp(PointerEventData eventData)
//    {
//        // Need this to happen even on mouse release outside?
//        //Debug.Log("STOP Moving Piece");
//        activePiece = null;
//    }

//    public void OnPointerDown(PointerEventData eventData)
//    {
//        //Vector2 pos = WolfheatProductions.Converter.GetMouseLocalPosition(GetComponent<RectTransform>());
//        //Debug.Log("Mouse eventdata Position: [" + eventData.position.x + "," + eventData.position.y + "] Block Puzzle: Mouse read local position: [" + pos.x + ", " + pos.y + "]");
//    }
//}
