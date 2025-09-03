using System;
using UnityEngine;

public class PieceSelector : MonoBehaviour
{
    [SerializeField] private GameObject selector; 
    [SerializeField] private SelectionChessPiece chessPiecePrefab; 
    [SerializeField] private GameObject left; 
    [SerializeField] private GameObject right; 

    int activeType = 0;

    public int ActiveType => activeType;

    public static PieceSelector Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CreateSelectablePieces();
    }

    private void CreateSelectablePieces()
    {
        for (int i = 0; i<12; i++) {
            SelectionChessPiece piece = Instantiate(chessPiecePrefab, i < 6 ? left.transform : right.transform);
            piece.ChangeType(i);
            if (i == 0) {
                // Set start selector at 0
                ChangeSelected(piece);
            }
        } 
    }

    public void ChangeSelected(ChessPiece selected)
    {        
        activeType = selected.Type;
        Debug.Log("Changed to type "+activeType);
        selector.transform.position = selected.transform.position;
    }
    
    public void ChangeSelected(int selectedType)
    {        
        activeType = selectedType;
        Debug.Log("Left size = "+ left.GetComponentsInChildren<Transform>(false).Length);
        GameObject selected = selectedType < 6 ? left.GetComponentsInChildren<Transform>(false)[selectedType+1].gameObject : right.GetComponentsInChildren<Transform>(false)[selectedType % 6+1].gameObject;
        Debug.Log("Selector set to "+selected.name);
        selector.transform.position = selected.transform.position;
    }
}
