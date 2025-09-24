using UnityEngine;

public class GameAreaMaster : MonoBehaviour
{
    [SerializeField] public GameArea MainGameArea;
    [SerializeField] public GameArea MiniViewGameArea;

    public static GameAreaMaster Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
}
