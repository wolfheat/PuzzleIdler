using UnityEngine;

public class GamePanel : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;  
    
    private int gameIndex = 0;

    public void OpenStartGame(int index)
    {
        for (int i = 0; i < panels.Length; i++) {
            panels[i].SetActive(index == i);
        }
    }
}
