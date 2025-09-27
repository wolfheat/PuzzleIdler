using System;
using UnityEngine;
using UnityEngine.EventSystems;
using WolfheatProductions;
using static UnityEditor.PlayerSettings;
public class Soduko : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler
{

    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private SodukoBox[] allBoxes; 
    [SerializeField] private SodukoBox boxPrefab; 
    [SerializeField] private GameObject boxHolder;

    private const int BoxSize = 65;

    private SodukoBox[,] boxes = new SodukoBox[9,9];

    SodukoBox hoverBox;


    void Start()
    {
        SetBoxes2DArray();

        ResetGame();    
    }

    private void SetBoxes2DArray()
    {
        for (int j = 0; j < boxes.GetLength(0); j++) {
            for (int i = 0; i < boxes.GetLength(1); i++) {
                SodukoBox box = Instantiate(boxPrefab, boxHolder.transform);
                boxes[j, i] = box;
            }
        }
    }

    private void ResetGame()
    {
        
    }

    private void OnEnable()
    {
        Inputs.NumberPressed += OnNumberPressed;
    }
    private void OnDisable()
    {
        Inputs.NumberPressed -= OnNumberPressed;
    }

    private void OnNumberPressed(int number)
    {
        Debug.Log("Pressing number in Soduko "+number);
        if (hoverBox == null)
            return;

        hoverBox.RequestChangeType(number);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Clicking should loop through all numbers?
        Vector2Int pos = Converter.GetMouseLocalPositionIndex(eventData, rectTransform, BoxSize);
        Debug.Log("Mouse Down on soduko ["+pos.y+","+pos.x+"]");
        // Find this box and update to next number?

        SodukoBox activeBox = boxes[pos.y, pos.x];

        bool rightClick = eventData.button == PointerEventData.InputButton.Right;

        //Check if this is fixed
        if (!activeBox.RequestNextNumber(rightClick)) {
            Debug.Log("This box is fixed, cant change number");
            return;
        }

        // The box was changed
        Debug.Log("Box changed to "+activeBox.Number);

        
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // When moving over and pressing any number that though appear?
        // Update active soduko box

        Vector2Int pos = Converter.GetMouseLocalPositionIndex(eventData, rectTransform, BoxSize);
        if (!ValidPos(pos))
            return;

        hoverBox = boxes[pos.y, pos.x];
    }

    private bool ValidPos(Vector2Int pos) => pos.x >= 0 && pos.y >= 0 && pos.x < 9 && pos.y < 9;
}
