using UnityEngine;

public class DifficultButtons : MonoBehaviour
{


    public void LoadNewGameDifficulty(int type = 0)
    {
        GameArea.Instance.ChangeGameSize(type);
    }
}
