using System;
using UnityEngine;

public class BlocksPuzzleGhostController : MonoBehaviour
{
    [SerializeField] private GameObject[] tetrisPieces;
    [SerializeField] private GameObject ghostHolder;

    private int activeType = 0;
    public int Rotation { get; set; } = 0;

    public static BlocksPuzzleGhostController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //public void MoveToPosition(Vector2 pos) => transform.localPosition = pos;

    public void ActivatePiece(TetrisBlockType newType, Vector2 offset)
    {
        activeType = (int)newType - 2;
        Debug.Log("Activating "+newType);
        for (int i = 0; i < tetrisPieces.Length; i++) {
            tetrisPieces[i].gameObject.SetActive(i == activeType);
        }

        // Offset the pickup Point for the ghost
        ghostHolder.transform.localPosition = - offset;   
    }

    internal void Hide()
    {
        if(activeType >= 0)
            tetrisPieces[activeType].gameObject.SetActive(false);
        activeType = -1;
    }

    internal void SetRotation(int rotations)
    {
        Rotation = rotations;
        ghostHolder.transform.rotation = Quaternion.Euler(0, 0, Rotation*90);
    }

    internal void Rotate(int rotations)
    {
        Rotation = (Rotation + 1) % 4;
        ghostHolder.transform.rotation = Quaternion.Euler(0, 0, Rotation*90);
    }
}
