using System;
using UnityEngine;

public class BlocksPuzzleGhostController : MonoBehaviour
{

    [SerializeField] private GameObject[] tetrisPieces;

    private int activeType = 0;

        public static BlocksPuzzleGhostController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivatePiece(TetrisBlockType.S);    
    }

    public void ActivatePiece(TetrisBlockType newType)
    {
        activeType = (int)newType - 2;
        Debug.Log("Activating "+newType);
        for (int i = 0; i < tetrisPieces.Length; i++) {
            tetrisPieces[i].gameObject.SetActive(i == activeType);
        }
    }

    internal void Hide()
    {
        if(activeType >= 0)
            tetrisPieces[activeType].gameObject.SetActive(false);
        activeType = -1;
    }
}
