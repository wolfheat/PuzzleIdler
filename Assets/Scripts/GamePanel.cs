using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    [SerializeField] private Image gameImage;  
    [SerializeField] private Sprite[] gameImages;  
    [SerializeField] private GameObject panel;  
    
    private int gameIndex = 0;

    public void OpenStartGame(int index)
    {
        panel.SetActive(true);
        gameImage.sprite = gameImages[index];
        gameImage.preserveAspect = true;
        gameIndex = index;
    }

    public void RequestStartGame()
    {
        Debug.Log("Starting game "+gameIndex);
        panel.SetActive(false);
    }


}
